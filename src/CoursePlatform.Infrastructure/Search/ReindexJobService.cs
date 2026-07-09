using System.Collections.Concurrent;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Admin.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoursePlatform.Infrastructure.Search;

public sealed class ReindexJobService : IReindexJobService
{
    private const int DefaultBatchSize = 500;
    private const int MaxLogEntries = 500;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReindexJobService> _logger;

    private readonly object _stateLock = new();
    private ReindexJobState? _current;
    private CancellationTokenSource? _jobCts;

    public ReindexJobService(IServiceScopeFactory scopeFactory, ILogger<ReindexJobService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public ReindexJobState? GetCurrent()
    {
        lock (_stateLock)
        {
            return _current;
        }
    }

    public Task<ReindexStartResult> StartAsync(CancellationToken cancellationToken)
    {
        lock (_stateLock)
        {
            if (_current is { Status: ReindexStatus.Running })
            {
                return Task.FromResult(ReindexStartResult.AlreadyInProgress(_current));
            }

            var jobId = Guid.NewGuid();
            var startedAt = DateTimeOffset.UtcNow;
            var emptyLogs = Array.Empty<ReindexLogEntry>();
            var emptyProgress = new ReindexProgress(0, 0, 0, DefaultBatchSize, 0, 0, 0);

            _jobCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _current = new ReindexJobState(
                JobId: jobId,
                Status: ReindexStatus.Running,
                Phase: ReindexPhase.Counting,
                Progress: emptyProgress,
                Logs: emptyLogs,
                StartedAt: startedAt,
                FinishedAt: null,
                ErrorMessage: null);

            _ = Task.Run(() => RunAsync(jobId, _jobCts.Token), CancellationToken.None);
        }

        return Task.FromResult(ReindexStartResult.Started(GetCurrent()!));
    }

    private async Task RunAsync(Guid jobId, CancellationToken token)
    {
        var logBag = new ConcurrentQueue<ReindexLogEntry>();
        var startedAt = DateTimeOffset.UtcNow;

        var progressReporter = new Progress<ReindexProgress>(p => UpdateProgress(jobId, p));
        var logReporter = new Progress<ReindexLogEntry>(entry =>
        {
            logBag.Enqueue(entry);
            TrimAndStoreLogs(jobId, logBag);
        });

        try
        {
            AddLog(jobId, ReindexLogLevel.Info, "Starting reindex job.", logBag);
            UpdatePhase(jobId, ReindexPhase.Counting);

            using var scope = _scopeFactory.CreateScope();
            var indexing = scope.ServiceProvider.GetRequiredService<ICourseIndexingService>();

            int total;
            try
            {
                total = await CountCoursesAsync(scope.ServiceProvider, token);
                AddLog(jobId, ReindexLogLevel.Info, $"Found {total} course(s) to index.", logBag);
            }
            catch (Exception ex)
            {
                AddLog(jobId, ReindexLogLevel.Error, $"Failed to count courses: {ex.Message}", logBag);
                Fail(jobId, ex.Message, startedAt);
                return;
            }

            UpdateProgress(jobId, new ReindexProgress(total, 0, 0, DefaultBatchSize, 0, (total + DefaultBatchSize - 1) / DefaultBatchSize, 0));

            UpdatePhase(jobId, ReindexPhase.PreparingIndex);
            AddLog(jobId, ReindexLogLevel.Info, "Preparing new index.", logBag);

            try
            {
                await indexing.ReindexCoursesChunkedAsync(progressReporter, logReporter, DefaultBatchSize, token);
            }
            catch (OperationCanceledException)
            {
                AddLog(jobId, ReindexLogLevel.Warning, "Reindex was cancelled.", logBag);
                Cancel(jobId);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reindex job {JobId} failed.", jobId);
                AddLog(jobId, ReindexLogLevel.Error, $"Reindex failed: {ex.Message}", logBag);
                Fail(jobId, ex.Message, startedAt);
                return;
            }

            UpdatePhase(jobId, ReindexPhase.Done);
            AddLog(jobId, ReindexLogLevel.Info, "Reindex completed successfully.", logBag);
            Succeed(jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in reindex job {JobId}.", jobId);
            AddLog(jobId, ReindexLogLevel.Error, $"Unhandled error: {ex.Message}", logBag);
            Fail(jobId, ex.Message, startedAt);
        }
    }

    private static async Task<int> CountCoursesAsync(IServiceProvider sp, CancellationToken token)
    {
        var db = sp.GetRequiredService<CoursePlatform.Application.Common.Interfaces.IApplicationDbContext>();
        return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(db.Courses, token);
    }

    private void UpdatePhase(Guid jobId, ReindexPhase phase)
    {
        lock (_stateLock)
        {
            if (_current == null || _current.JobId != jobId) return;
            _current = _current with { Phase = phase };
        }
    }

    private void UpdateProgress(Guid jobId, ReindexProgress progress)
    {
        lock (_stateLock)
        {
            if (_current == null || _current.JobId != jobId) return;
            _current = _current with { Progress = progress };
        }
    }

    private void AddLog(Guid jobId, ReindexLogLevel level, string message, ConcurrentQueue<ReindexLogEntry> bag)
    {
        bag.Enqueue(new ReindexLogEntry(DateTimeOffset.UtcNow, level, message));
        TrimAndStoreLogs(jobId, bag);
    }

    private void TrimAndStoreLogs(Guid jobId, ConcurrentQueue<ReindexLogEntry> bag)
    {
        var snapshot = bag.ToArray();
        if (snapshot.Length > MaxLogEntries)
        {
            snapshot = snapshot[^MaxLogEntries..];
        }
        lock (_stateLock)
        {
            if (_current == null || _current.JobId != jobId) return;
            _current = _current with { Logs = snapshot };
        }
    }

    private void Succeed(Guid jobId)
    {
        lock (_stateLock)
        {
            if (_current == null || _current.JobId != jobId) return;
            _current = _current with
            {
                Status = ReindexStatus.Succeeded,
                FinishedAt = DateTimeOffset.UtcNow,
                Phase = ReindexPhase.Done
            };
        }
    }

    private void Fail(Guid jobId, string message, DateTimeOffset startedAt)
    {
        lock (_stateLock)
        {
            if (_current == null || _current.JobId != jobId) return;
            _current = _current with
            {
                Status = ReindexStatus.Failed,
                FinishedAt = DateTimeOffset.UtcNow,
                ErrorMessage = message,
                Phase = ReindexPhase.Done
            };
        }
    }

    private void Cancel(Guid jobId)
    {
        lock (_stateLock)
        {
            if (_current == null || _current.JobId != jobId) return;
            _current = _current with
            {
                Status = ReindexStatus.Cancelled,
                FinishedAt = DateTimeOffset.UtcNow,
                Phase = ReindexPhase.Done
            };
        }
    }
}
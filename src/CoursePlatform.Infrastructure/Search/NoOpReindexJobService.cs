using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Admin.Search;

namespace CoursePlatform.Infrastructure.Search;

internal sealed class NoOpReindexJobService : IReindexJobService
{
    public ReindexJobState? GetCurrent() => null;

    public Task<ReindexStartResult> StartAsync(CancellationToken cancellationToken)
    {
        var state = new ReindexJobState(
            JobId: Guid.Empty,
            Status: ReindexStatus.Failed,
            Phase: ReindexPhase.Done,
            Progress: new ReindexProgress(0, 0, 0, 0, 0, 0, 0),
            Logs: new[]
            {
                new ReindexLogEntry(DateTimeOffset.UtcNow, ReindexLogLevel.Warning,
                    "Elasticsearch is disabled in this environment. Reindex is unavailable.")
            },
            StartedAt: DateTimeOffset.UtcNow,
            FinishedAt: DateTimeOffset.UtcNow,
            ErrorMessage: "Elasticsearch is disabled.");

        return Task.FromResult(ReindexStartResult.Started(state));
    }
}
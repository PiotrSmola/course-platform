using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Admin.Search;

namespace CoursePlatform.Infrastructure.Search;

internal sealed class NoOpCourseIndexingService : ICourseIndexingService
{
    public Task EnsureIndexAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task IndexCourseAsync(Guid courseId, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task ReindexCoursesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<CourseIndexStats> GetStatsAsync(CancellationToken cancellationToken)
        => Task.FromResult(new CourseIndexStats(
            Exists: false,
            Enabled: false,
            IndexName: string.Empty,
            AliasName: string.Empty,
            ConcreteIndexName: null,
            DocumentCount: 0,
            SizeBytes: null,
            Health: "disabled"));

    public Task ReindexCoursesChunkedAsync(
        IProgress<ReindexProgress>? progress,
        IProgress<ReindexLogEntry>? log,
        int batchSize,
        CancellationToken cancellationToken)
    {
        log?.Report(new ReindexLogEntry(DateTimeOffset.UtcNow, ReindexLogLevel.Warning,
            "Elasticsearch is disabled — reindex is a no-op."));
        progress?.Report(new ReindexProgress(0, 0, 0, batchSize, 0, 0, 100));
        return Task.CompletedTask;
    }
}
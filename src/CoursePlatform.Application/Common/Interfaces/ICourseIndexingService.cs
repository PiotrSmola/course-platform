using CoursePlatform.Application.Features.Admin.Search;

namespace CoursePlatform.Application.Common.Interfaces;

public interface ICourseIndexingService
{
    Task EnsureIndexAsync(CancellationToken cancellationToken);
    Task IndexCourseAsync(Guid courseId, CancellationToken cancellationToken);
    Task DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken);
    Task ReindexCoursesAsync(CancellationToken cancellationToken);

    Task<CourseIndexStats> GetStatsAsync(CancellationToken cancellationToken);

    Task ReindexCoursesChunkedAsync(
        IProgress<ReindexProgress>? progress,
        IProgress<ReindexLogEntry>? log,
        int batchSize,
        CancellationToken cancellationToken);
}
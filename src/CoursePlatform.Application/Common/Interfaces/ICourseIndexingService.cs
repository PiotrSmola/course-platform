namespace CoursePlatform.Application.Common.Interfaces;

public interface ICourseIndexingService
{
    Task EnsureIndexAsync(CancellationToken cancellationToken);
    Task IndexCourseAsync(Guid courseId, CancellationToken cancellationToken);
    Task DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken);
    Task ReindexCoursesAsync(CancellationToken cancellationToken);
}

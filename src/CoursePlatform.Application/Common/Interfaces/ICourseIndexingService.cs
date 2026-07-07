namespace CoursePlatform.Application.Common.Interfaces;

public interface ICourseIndexingService
{
    Task EnsureIndexAsync(CancellationToken cancellationToken);
    Task ReindexCoursesAsync(CancellationToken cancellationToken);
}


using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Infrastructure.Search;

internal sealed class NoOpCourseIndexingService : ICourseIndexingService
{
    public Task EnsureIndexAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task IndexCourseAsync(Guid courseId, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task ReindexCoursesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

using CoursePlatform.Application.Features.Courses.Queries.GetCourses;

namespace CoursePlatform.Application.Common.Interfaces;

public interface ICourseSearchService
{
    Task<CourseSearchPage> SearchAsync(CourseSearchCriteria criteria, CancellationToken cancellationToken);
}


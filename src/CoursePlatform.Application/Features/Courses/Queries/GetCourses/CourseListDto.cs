using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Courses.Queries.GetCourses;

public record CourseListDto(
    Guid Id,
    string Title,
    string ShortDescription,
    decimal Price,
    CourseLevel Level,
    string ThumbnailUrl,
    string InstructorName,
    int ModuleCount,
    int LessonCount,
    double AverageRating,
    int ReviewCount);

public record CoursesVm(IReadOnlyCollection<CourseListDto> Items, int TotalCount);

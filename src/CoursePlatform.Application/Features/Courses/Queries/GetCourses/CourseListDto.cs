using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Courses.Queries.GetCourses;

public record CourseListDto(
    Guid Id,
    string Title,
    string ShortDescription,
    decimal Price,
    CourseLevel Level,
    CourseStatus Status,
    string ThumbnailObjectKey,
    string InstructorName,
    string Language,
    IReadOnlyCollection<string> CategoryNames,
    IReadOnlyCollection<string> TechnologyNames,
    int ModuleCount,
    int LessonCount,
    double AverageRating,
    int ReviewCount);

public record CoursesVm(IReadOnlyCollection<CourseListDto> Items, int TotalCount);

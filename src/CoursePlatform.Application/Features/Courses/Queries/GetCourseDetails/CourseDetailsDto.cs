using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Courses.Queries.GetCourseDetails;

public record CourseDetailsDto(
    Guid Id,
    string Title,
    string Description,
    string ShortDescription,
    decimal Price,
    CourseLevel Level,
    CourseStatus Status,
    string ThumbnailUrl,
    Guid InstructorId,
    string InstructorName,
    DateTime CreatedAt,
    List<ModuleDto> Modules,
    double AverageRating,
    int ReviewCount,
    List<ReviewDto> Reviews);

public record ModuleDto(Guid Id, string Title, int Order, List<LessonDto> Lessons);
public record LessonDto(Guid Id, string Title, string? Description, int Duration, int Order, string VideoUrl);
public record ReviewDto(Guid Id, int Rating, string Comment, string AuthorName, DateTime CreatedAt);

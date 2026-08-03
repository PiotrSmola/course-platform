namespace CoursePlatform.Application.Features.Trials;

public record TrialEligibleCourseDto(
    Guid Id,
    string Title,
    string ShortDescription,
    string? ThumbnailUrl,
    string InstructorName,
    decimal Price,
    int LessonCount);

public record TrialAccessDto(
    Guid CourseId,
    DateTime ActivatedAt,
    Guid FirstLessonId,
    Guid SecondLessonId);

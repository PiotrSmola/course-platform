using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.LearningPaths.Queries.GetLearningPathBySlug;

public record LearningPathCourseItemDto(
    Guid Id,
    int Order,
    bool IsOptional,
    Guid CourseId,
    string CourseTitle,
    string CourseShortDescription,
    decimal CoursePrice,
    CourseLevel CourseLevel,
    string CourseThumbnailUrl,
    string CourseLanguage,
    string InstructorName);

public record LearningPathDetailsDto(
    Guid Id,
    string Title,
    string Slug,
    string ShortDescription,
    string Description,
    PathDifficultyLevel DifficultyLevel,
    int EstimatedHours,
    string ThumbnailUrl,
    DateTime CreatedAt,
    IReadOnlyList<LearningPathCourseItemDto> Courses);
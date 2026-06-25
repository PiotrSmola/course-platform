using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.LearningPaths.Queries.GetLearningPaths;

public record LearningPathListItemDto(
    Guid Id,
    string Title,
    string Slug,
    string ShortDescription,
    PathDifficultyLevel DifficultyLevel,
    int EstimatedHours,
    string ThumbnailUrl,
    int CourseCount);

public record LearningPathsVm(IReadOnlyList<LearningPathListItemDto> Items);
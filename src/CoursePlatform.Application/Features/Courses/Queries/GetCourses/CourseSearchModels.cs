using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Courses.Queries.GetCourses;

public sealed record CourseSearchCriteria(
    bool IsAdmin,
    string? SearchTerm,
    CourseLevel? Level,
    CourseStatus? Status,
    string? SortBy,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Language,
    IReadOnlyCollection<Guid>? CategoryIds,
    IReadOnlyCollection<Guid>? TechnologyIds,
    double? MinRating,
    int PageNumber,
    int PageSize);

public sealed record CourseListRow(
    Guid Id,
    string Title,
    string ShortDescription,
    decimal Price,
    CourseLevel Level,
    CourseStatus Status,
    string? ThumbnailObjectKey,
    string InstructorName,
    string Language,
    IReadOnlyCollection<string> CategoryNames,
    IReadOnlyCollection<string> TechnologyNames,
    int ModuleCount,
    int LessonCount,
    double AverageRating,
    int ReviewCount,
    IReadOnlyCollection<string> MatchedBy);

public sealed record CourseSearchPage(IReadOnlyCollection<CourseListRow> Items, int TotalCount);


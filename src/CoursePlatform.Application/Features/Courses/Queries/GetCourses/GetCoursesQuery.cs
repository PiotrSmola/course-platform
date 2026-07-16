using MediatR;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Courses.Queries.GetCourses;

public record GetCoursesQuery(
    string? SearchTerm,
    CourseLevel? Level,
    CourseStatus? Status,
    string? SortBy,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Language,
    List<Guid>? CategoryIds,
    List<Guid>? TechnologyIds,
    double? MinRating,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<CoursesVm>;

public class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, CoursesVm>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;
    private readonly ICourseSearchService _courseSearch;
    private readonly IAppCache _cache;

    public GetCoursesQueryHandler(
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage,
        ICourseSearchService courseSearch,
        IAppCache cache)
    {
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
        _courseSearch = courseSearch;
        _cache = cache;
    }

    public async Task<CoursesVm> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        var criteria = new CourseSearchCriteria(
            _currentUserService.IsAdmin,
            request.SearchTerm,
            request.Level,
            request.Status,
            request.SortBy,
            request.MinPrice,
            request.MaxPrice,
            request.Language,
            request.CategoryIds,
            request.TechnologyIds,
            request.MinRating,
            request.PageNumber,
            request.PageSize);

        var cacheable = !criteria.IsAdmin && string.IsNullOrWhiteSpace(criteria.SearchTerm);

        var page = cacheable
            ? await _cache.GetOrCreateAsync(
                BuildCacheKey(criteria),
                TimeSpan.FromSeconds(60),
                async ct => await _courseSearch.SearchAsync(criteria, ct),
                tags: new[] { "courses" },
                cancellationToken: cancellationToken)
            : await _courseSearch.SearchAsync(criteria, cancellationToken);

        var items = new List<CourseListDto>(page.Items.Count);
        foreach (var row in page.Items)
        {
            items.Add(new CourseListDto(
                row.Id,
                row.Title,
                row.ShortDescription,
                row.Price,
                row.Level,
                row.Status,
                await _fileStorage.GetThumbnailUrlOrNullAsync(row.ThumbnailObjectKey, cancellationToken),
                row.InstructorName,
                row.Language,
                row.CategoryNames,
                row.TechnologyNames,
                row.ModuleCount,
                row.LessonCount,
                row.AverageRating,
                row.ReviewCount,
                row.MatchedBy));
        }

        return new CoursesVm(items, page.TotalCount);
    }

    private static string BuildCacheKey(CourseSearchCriteria criteria)
    {
        var payload = System.Text.Json.JsonSerializer.Serialize(criteria);
        var hash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(payload)));
        return $"cp:courses:{hash}";
    }
}

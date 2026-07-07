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

    public GetCoursesQueryHandler(
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage,
        ICourseSearchService courseSearch)
    {
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
        _courseSearch = courseSearch;
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

        var page = await _courseSearch.SearchAsync(criteria, cancellationToken);

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
}

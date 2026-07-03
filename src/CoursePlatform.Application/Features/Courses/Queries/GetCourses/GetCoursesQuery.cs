using MediatR;
using Microsoft.EntityFrameworkCore;
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
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public GetCoursesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<CoursesVm> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Courses
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = $"%{request.SearchTerm.Trim().ToLower()}%";
            query = query.Where(c =>
                EF.Functions.Like(c.Title.ToLower(), search) ||
                EF.Functions.Like(c.ShortDescription.ToLower(), search) ||
                EF.Functions.Like(c.Description.ToLower(), search) ||
                c.Categories.Any(cat => EF.Functions.Like(cat.Name.ToLower(), search)) ||
                c.Technologies.Any(tech => EF.Functions.Like(tech.Name.ToLower(), search)));
        }

        if (request.Level.HasValue)
        {
            query = query.Where(c => c.Level == request.Level.Value);
        }

        if (_currentUserService.IsAdmin)
        {
            if (request.Status.HasValue)
            {
                query = query.Where(c => c.Status == request.Status.Value);
            }
        }
        else
        {
            query = query.Where(c => c.Status == CourseStatus.Published);
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(c => c.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(c => c.Price <= request.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Language))
        {
            var language = request.Language.Trim().ToLower();
            query = query.Where(c => c.Language.ToLower() == language);
        }

        if (request.CategoryIds != null && request.CategoryIds.Any())
        {
            query = query.Where(c => c.Categories.Any(cat => request.CategoryIds.Contains(cat.Id)));
        }

        if (request.TechnologyIds != null && request.TechnologyIds.Any())
        {
            query = query.Where(c => c.Technologies.Any(tech => request.TechnologyIds.Contains(tech.Id)));
        }

        if (request.MinRating.HasValue)
        {
            query = query.Where(c =>
                c.Reviews.Any() &&
                c.Reviews.Average(r => r.Rating) >= request.MinRating.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortBy switch
        {
            "price-asc" => query.OrderBy(c => c.Price),
            "price-desc" => query.OrderByDescending(c => c.Price),
            "rating" => query.OrderByDescending(c => c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0),
            "popular" => query.OrderByDescending(c => c.Reviews.Count),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var rows = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.ShortDescription,
                c.Price,
                c.Level,
                c.Status,
                c.ThumbnailObjectKey,
                InstructorName = $"{c.Instructor.FirstName} {c.Instructor.LastName}",
                c.Language,
                CategoryNames = c.Categories.Select(cat => cat.Name).ToList(),
                TechnologyNames = c.Technologies.Select(tech => tech.Name).ToList(),
                ModuleCount = c.Modules.Count,
                LessonCount = c.Modules.SelectMany(m => m.Lessons).Count(),
                AverageRating = c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = c.Reviews.Count
            })
            .ToListAsync(cancellationToken);

        var items = new List<CourseListDto>(rows.Count);
        foreach (var row in rows)
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
                row.ReviewCount));
        }

        return new CoursesVm(items, totalCount);
    }
}

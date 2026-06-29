using MediatR;
using Microsoft.EntityFrameworkCore;
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

    public GetCoursesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CoursesVm> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Courses
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(c =>
                c.Title.ToLower().Contains(search) ||
                c.ShortDescription.ToLower().Contains(search) ||
                c.Description.ToLower().Contains(search) ||
                c.Categories.Any(cat => cat.Name.ToLower().Contains(search)) ||
                c.Technologies.Any(tech => tech.Name.ToLower().Contains(search)));
        }

        if (request.Level.HasValue)
        {
            query = query.Where(c => c.Level == request.Level.Value);
        }

        if (request.Status.HasValue)
        {
            if (_currentUserService.IsAdmin)
            {
                query = query.Where(c => c.Status == request.Status.Value);
            }
            else if (_currentUserService.UserId.HasValue)
            {
                query = query.Where(c => c.Status == request.Status.Value && c.InstructorId == _currentUserService.UserId.Value);
            }
            else
            {
                query = query.Where(c => c.Status == CourseStatus.Published);
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

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseListDto(
                c.Id,
                c.Title,
                c.ShortDescription,
                c.Price,
                c.Level,
                c.ThumbnailUrl,
                $"{c.Instructor.FirstName} {c.Instructor.LastName}",
                c.Language,
                c.Categories.Select(cat => cat.Name).ToList(),
                c.Technologies.Select(tech => tech.Name).ToList(),
                c.Modules.Count,
                c.Modules.SelectMany(m => m.Lessons).Count(),
                c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0,
                c.Reviews.Count))
            .ToListAsync(cancellationToken);

        return new CoursesVm(items, totalCount);
    }
}

using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Courses.Queries.GetCourses;
using CoursePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Infrastructure.Search;

public sealed class EfCourseSearchService : ICourseSearchService
{
    private readonly IApplicationDbContext _context;

    public EfCourseSearchService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseSearchPage> SearchAsync(CourseSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var query = _context.Courses
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var search = $"%{criteria.SearchTerm.Trim().ToLower()}%";
            query = query.Where(c =>
                EF.Functions.Like(c.Title.ToLower(), search) ||
                EF.Functions.Like(c.ShortDescription.ToLower(), search) ||
                EF.Functions.Like(c.Description.ToLower(), search) ||
                c.Categories.Any(cat => EF.Functions.Like(cat.Name.ToLower(), search)) ||
                c.Technologies.Any(tech => EF.Functions.Like(tech.Name.ToLower(), search)));
        }

        if (criteria.Level.HasValue)
        {
            query = query.Where(c => c.Level == criteria.Level.Value);
        }

        if (criteria.IsAdmin)
        {
            if (criteria.Status.HasValue)
            {
                query = query.Where(c => c.Status == criteria.Status.Value);
            }
        }
        else
        {
            query = query.Where(c => c.Status == CourseStatus.Published);
        }

        if (criteria.MinPrice.HasValue)
        {
            query = query.Where(c => c.Price >= criteria.MinPrice.Value);
        }

        if (criteria.MaxPrice.HasValue)
        {
            query = query.Where(c => c.Price <= criteria.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Language))
        {
            var language = criteria.Language.Trim().ToLower();
            query = query.Where(c => c.Language.ToLower() == language);
        }

        if (criteria.CategoryIds != null && criteria.CategoryIds.Count > 0)
        {
            query = query.Where(c => c.Categories.Any(cat => criteria.CategoryIds.Contains(cat.Id)));
        }

        if (criteria.TechnologyIds != null && criteria.TechnologyIds.Count > 0)
        {
            query = query.Where(c => c.Technologies.Any(tech => criteria.TechnologyIds.Contains(tech.Id)));
        }

        if (criteria.MinRating.HasValue)
        {
            query = query.Where(c =>
                c.Reviews.Any() &&
                c.Reviews.Average(r => r.Rating) >= criteria.MinRating.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = criteria.SortBy switch
        {
            "price-asc" => query.OrderBy(c => c.Price),
            "price-desc" => query.OrderByDescending(c => c.Price),
            "rating" => query.OrderByDescending(c => c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0),
            "popular" => query.OrderByDescending(c => c.Reviews.Count),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var rows = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(c => new CourseListRow(
                c.Id,
                c.Title,
                c.ShortDescription,
                c.Price,
                c.Level,
                c.Status,
                c.ThumbnailObjectKey,
                $"{c.Instructor.FirstName} {c.Instructor.LastName}",
                c.Language,
                c.Categories.Select(cat => cat.Name).ToList(),
                c.Technologies.Select(tech => tech.Name).ToList(),
                c.Modules.Count,
                c.Modules.SelectMany(m => m.Lessons).Count(),
                c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0,
                c.Reviews.Count,
                Array.Empty<string>()))
            .ToListAsync(cancellationToken);

        return new CourseSearchPage(rows, totalCount);
    }
}


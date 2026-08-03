using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Courses.Queries.GetCourses;
using CoursePlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Application.Features.Courses.Queries.GetPopularCourses;

public record GetPopularCoursesQuery : IRequest<IReadOnlyList<CourseListDto>>;

public class GetPopularCoursesQueryHandler : IRequestHandler<GetPopularCoursesQuery, IReadOnlyList<CourseListDto>>
{
    private const int PopularCoursesLimit = 8;

    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public GetPopularCoursesQueryHandler(
        IApplicationDbContext context,
        IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<IReadOnlyList<CourseListDto>> Handle(
        GetPopularCoursesQuery request,
        CancellationToken cancellationToken)
    {
        var since = DateTime.UtcNow.AddDays(-90);

        var ranking = await _context.Courses
            .AsNoTracking()
            .Where(course => course.Status == CourseStatus.Published)
            .Select(course => new
            {
                course.Id,
                EnrollmentCount = course.Enrollments.Count(enrollment => enrollment.EnrolledAt >= since),
                ReviewCount = course.Reviews.Count,
                AverageRating = course.Reviews.Count == 0
                    ? 0d
                    : course.Reviews.Average(review => (double)review.Rating),
                course.CreatedAt
            })
            .OrderByDescending(course => course.EnrollmentCount)
            .ThenByDescending(course => course.AverageRating)
            .ThenByDescending(course => course.ReviewCount)
            .ThenByDescending(course => course.CreatedAt)
            .Take(PopularCoursesLimit)
            .ToListAsync(cancellationToken);

        if (ranking.Count == 0)
        {
            return Array.Empty<CourseListDto>();
        }

        var ids = ranking.Select(course => course.Id).ToList();
        var courses = await _context.Courses
            .AsNoTracking()
            .Include(course => course.Instructor)
            .Include(course => course.Categories)
            .Include(course => course.Technologies)
            .Include(course => course.Modules)
            .ThenInclude(module => module.Lessons)
            .Include(course => course.Reviews)
            .Where(course => ids.Contains(course.Id))
            .ToDictionaryAsync(course => course.Id, cancellationToken);

        var result = new List<CourseListDto>(ranking.Count);
        foreach (var ranked in ranking)
        {
            if (!courses.TryGetValue(ranked.Id, out var course))
            {
                continue;
            }

            result.Add(new CourseListDto(
                course.Id,
                course.Title,
                course.ShortDescription,
                course.Price,
                course.Level,
                course.Status,
                await _fileStorage.GetThumbnailUrlOrNullAsync(course.ThumbnailObjectKey, cancellationToken),
                $"{course.Instructor.FirstName} {course.Instructor.LastName}".Trim(),
                course.Language,
                course.Categories.Select(category => category.Name).ToList(),
                course.Technologies.Select(technology => technology.Name).ToList(),
                course.Modules.Count,
                course.Modules.Sum(module => module.Lessons.Count),
                ranked.AverageRating,
                ranked.ReviewCount,
                Array.Empty<string>()));
        }

        return result;
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;

namespace CoursePlatform.Application.Features.Courses.Queries.GetCourseDetails;

public record GetCourseDetailsQuery(Guid Id) : IRequest<CourseDetailsDto>;

public class GetCourseDetailsQueryHandler : IRequestHandler<GetCourseDetailsQuery, CourseDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetCourseDetailsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseDetailsDto> Handle(GetCourseDetailsQuery request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .Include(c => c.Instructor)
            .Include(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .Include(c => c.Reviews)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (course == null)
        {
            throw new NotFoundException($"Course {request.Id} not found.");
        }

        var modules = course.Modules.OrderBy(m => m.Order).Select(m => new ModuleDto(
            m.Id,
            m.Title,
            m.Order,
            m.Lessons.OrderBy(l => l.Order).Select(l => new LessonDto(
                l.Id,
                l.Title,
                l.Description,
                l.Duration,
                l.Order,
                l.VideoUrl)).ToList())).ToList();

        var reviews = course.Reviews.OrderByDescending(r => r.CreatedAt).Select(r => new ReviewDto(
            r.Id,
            r.Rating,
            r.Comment,
            $"{r.User.FirstName} {r.User.LastName}",
            r.CreatedAt)).ToList();

        return new CourseDetailsDto(
            course.Id,
            course.Title,
            course.Description,
            course.ShortDescription,
            course.Price,
            course.Level,
            course.Status,
            course.ThumbnailUrl,
            course.InstructorId,
            $"{course.Instructor.FirstName} {course.Instructor.LastName}",
            course.CreatedAt,
            modules,
            course.Reviews.Any() ? course.Reviews.Average(r => r.Rating) : 0,
            course.Reviews.Count,
            reviews);
    }
}

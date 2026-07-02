using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;

namespace CoursePlatform.Application.Features.Courses.Queries.GetCourseDetails;

public record GetCourseDetailsQuery(Guid Id) : IRequest<CourseDetailsDto>;

public class GetCourseDetailsQueryHandler : IRequestHandler<GetCourseDetailsQuery, CourseDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCourseDetailsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CourseDetailsDto> Handle(GetCourseDetailsQuery request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .Include(c => c.Instructor)
            .Include(c => c.Categories)
            .Include(c => c.Technologies)
            .Include(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .Include(c => c.Reviews)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (course == null)
        {
            throw new NotFoundException($"Course {request.Id} not found.");
        }

        if (course.Status != Domain.Enums.CourseStatus.Published)
        {
            if (!_currentUserService.UserId.HasValue ||
                (_currentUserService.UserId.Value != course.InstructorId && !_currentUserService.IsAdmin))
            {
                throw new NotFoundException($"Course {request.Id} not found.");
            }
        }

        var isEnrolled = false;
        var hasUserReviewed = false;
        HashSet<Guid> completedLessonIds = new();

        if (_currentUserService.UserId.HasValue)
        {
            var userId = _currentUserService.UserId.Value;
            isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == request.Id, cancellationToken);

            hasUserReviewed = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.CourseId == request.Id, cancellationToken);

            if (isEnrolled)
            {
                var lessonIds = course.Modules.SelectMany(m => m.Lessons).Select(l => l.Id).ToList();
                completedLessonIds = (await _context.LessonProgresses
                    .Where(lp => lp.UserId == userId && lessonIds.Contains(lp.LessonId) && lp.IsCompleted)
                    .Select(lp => lp.LessonId)
                    .ToListAsync(cancellationToken))
                    .ToHashSet();
            }
        }

        var canManage = _currentUserService.UserId.HasValue &&
            (_currentUserService.UserId.Value == course.InstructorId || _currentUserService.IsAdmin);

        var modules = course.Modules.OrderBy(m => m.Order).Select(m => new ModuleDto(
            m.Id,
            m.Title,
            m.Order,
            m.Lessons.OrderBy(l => l.Order).Select(l => new LessonListDto(
                l.Id,
                l.Title,
                l.Description,
                l.Duration,
                l.Order,
                completedLessonIds.Contains(l.Id),
                canManage ? l.VideoObjectKey : null)).ToList())).ToList();

        var reviews = course.Reviews.OrderByDescending(r => r.CreatedAt).Select(r => new ReviewDto(
            r.Id,
            r.Rating,
            r.Comment,
            $"{r.User.FirstName} {r.User.LastName}",
            r.CreatedAt)).ToList();

        var canReview = isEnrolled && !hasUserReviewed;

        return new CourseDetailsDto(
            course.Id,
            course.Title,
            course.Description,
            course.ShortDescription,
            course.Price,
            course.Level,
            course.Status,
            course.ThumbnailObjectKey,
            course.Language,
            course.InstructorId,
            $"{course.Instructor.FirstName} {course.Instructor.LastName}",
            course.CreatedAt,
            course.Categories.Select(c => c.Name).ToList(),
            course.Technologies.Select(t => t.Name).ToList(),
            modules,
            course.Reviews.Any() ? course.Reviews.Average(r => r.Rating) : 0,
            course.Reviews.Count,
            reviews,
            isEnrolled,
            hasUserReviewed,
            canReview);
    }
}

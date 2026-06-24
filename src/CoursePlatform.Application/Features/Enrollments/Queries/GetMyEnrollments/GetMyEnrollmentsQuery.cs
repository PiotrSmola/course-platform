using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Enrollments.Queries.GetMyEnrollments;

public record EnrollmentDto(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    string CourseThumbnailUrl,
    CourseLevel CourseLevel,
    DateTime EnrolledAt,
    int CompletedLessons,
    int TotalLessons,
    double ProgressPercentage);

public record GetMyEnrollmentsQuery : IRequest<List<EnrollmentDto>>;

public class GetMyEnrollmentsQueryHandler : IRequestHandler<GetMyEnrollmentsQuery, List<EnrollmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyEnrollmentsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<EnrollmentDto>> Handle(GetMyEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null) return new List<EnrollmentDto>();

        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .ThenInclude(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .Where(e => e.UserId == _currentUserService.UserId.Value)
            .ToListAsync(cancellationToken);

        var lessonIds = enrollments.SelectMany(e => e.Course.Modules.SelectMany(m => m.Lessons.Select(l => l.Id))).ToList();
        var progress = await _context.LessonProgresses
            .AsNoTracking()
            .Where(lp => lp.UserId == _currentUserService.UserId.Value && lessonIds.Contains(lp.LessonId))
            .ToListAsync(cancellationToken);

        var result = enrollments.Select(e =>
        {
            var totalLessons = e.Course.Modules.SelectMany(m => m.Lessons).Count();
            var completedLessons = progress.Count(p => e.Course.Modules.SelectMany(m => m.Lessons).Any(l => l.Id == p.LessonId) && p.IsCompleted);
            return new EnrollmentDto(
                e.Id,
                e.CourseId,
                e.Course.Title,
                e.Course.ThumbnailUrl,
                e.Course.Level,
                e.EnrolledAt,
                completedLessons,
                totalLessons,
                totalLessons > 0 ? (double)completedLessons / totalLessons * 100 : 0);
        }).ToList();

        return result;
    }
}

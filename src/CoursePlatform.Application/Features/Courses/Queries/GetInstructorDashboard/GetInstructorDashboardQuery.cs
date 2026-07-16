using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Courses.Queries.GetInstructorDashboard;

public record InstructorCourseStatsDto(
    Guid Id,
    string Title,
    CourseStatus Status,
    int EnrollmentCount,
    decimal Revenue,
    double CompletionRate);

public record InstructorDashboardDto(
    int TotalStudents,
    decimal TotalRevenue,
    double AverageCompletionRate,
    IReadOnlyList<InstructorCourseStatsDto> Courses);

public record GetInstructorDashboardQuery : IRequest<InstructorDashboardDto>;

public class GetInstructorDashboardQueryHandler : IRequestHandler<GetInstructorDashboardQuery, InstructorDashboardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetInstructorDashboardQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<InstructorDashboardDto> Handle(GetInstructorDashboardQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var instructorId = _currentUserService.UserId.Value;

        var courses = await _context.Courses
            .AsNoTracking()
            .Where(c => c.InstructorId == instructorId)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Status,
                EnrollmentCount = c.Enrollments.Count,
                LessonIds = c.Modules.SelectMany(m => m.Lessons).Select(l => l.Id).ToList(),
                Revenue = c.Enrollments
                    .SelectMany(e => e.User.Payments.Where(p => p.CourseId == c.Id && p.Status == Domain.Enums.PaymentStatus.Completed))
                    .Sum(p => p.Amount)
            })
            .ToListAsync(cancellationToken);

        var courseIds = courses.Select(c => c.Id).ToList();
        var allLessonIds = courses.SelectMany(c => c.LessonIds).ToList();

        var completedLessonsByCourse = await _context.LessonProgresses
            .AsNoTracking()
            .Where(lp => lp.IsCompleted && allLessonIds.Contains(lp.LessonId))
            .GroupBy(lp => lp.LessonId)
            .Select(g => new { LessonId = g.Key, CompletedCount = g.Count() })
            .ToListAsync(cancellationToken);

        var completedLookup = completedLessonsByCourse.ToDictionary(x => x.LessonId, x => x.CompletedCount);

        var courseStats = new List<InstructorCourseStatsDto>(courses.Count);
        double totalCompletionRate = 0;
        int coursesWithLessons = 0;

        foreach (var course in courses)
        {
            var totalLessons = course.LessonIds.Count;
            var totalCompleted = course.LessonIds.Sum(lessonId => completedLookup.GetValueOrDefault(lessonId));
            var completionRate = totalLessons > 0
                ? (double)totalCompleted / (course.EnrollmentCount * totalLessons) * 100
                : 0;

            if (totalLessons > 0)
            {
                totalCompletionRate += completionRate;
                coursesWithLessons++;
            }

            courseStats.Add(new InstructorCourseStatsDto(
                course.Id,
                course.Title,
                course.Status,
                course.EnrollmentCount,
                course.Revenue,
                Math.Round(completionRate, 2)));
        }

        var averageCompletionRate = coursesWithLessons > 0
            ? Math.Round(totalCompletionRate / coursesWithLessons, 2)
            : 0;

        return new InstructorDashboardDto(
            courses.Sum(c => c.EnrollmentCount),
            courses.Sum(c => c.Revenue),
            averageCompletionRate,
            courseStats);
    }
}

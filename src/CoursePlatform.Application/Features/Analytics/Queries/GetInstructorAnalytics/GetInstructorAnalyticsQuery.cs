using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Analytics.Queries.GetInstructorAnalytics;

public record LessonDropOffDto(
    Guid LessonId,
    string LessonTitle,
    Guid ModuleId,
    string ModuleTitle,
    int OrderIndex,
    int ReachedCount,
    int CompletedCount,
    double DropOffPercent);

public record InstructorCourseAnalyticsDto(
    Guid CourseId,
    string Title,
    int EnrollmentCount,
    decimal Revenue,
    double CompletionRate,
    IReadOnlyList<LessonDropOffDto> DropOff);

public record InstructorAnalyticsDto(
    decimal TotalRevenue,
    double AverageCompletionRate,
    IReadOnlyList<InstructorCourseAnalyticsDto> Courses);

public record GetInstructorAnalyticsQuery(Guid? CourseId = null) : IRequest<InstructorAnalyticsDto>;

public class GetInstructorAnalyticsQueryHandler : IRequestHandler<GetInstructorAnalyticsQuery, InstructorAnalyticsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetInstructorAnalyticsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<InstructorAnalyticsDto> Handle(
        GetInstructorAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var instructorId = _currentUserService.UserId.Value;

        var coursesQuery = _context.Courses
            .AsNoTracking()
            .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
            .Where(c => c.InstructorId == instructorId);

        if (request.CourseId.HasValue)
        {
            coursesQuery = coursesQuery.Where(c => c.Id == request.CourseId.Value);
        }

        var courses = await coursesQuery.ToListAsync(cancellationToken);

        if (request.CourseId.HasValue && courses.Count == 0)
        {
            throw new NotFoundException($"Course {request.CourseId} not found.");
        }

        var courseIds = courses.Select(c => c.Id).ToList();
        var allLessonIds = courses
            .SelectMany(c => c.Modules.SelectMany(m => m.Lessons.Select(l => l.Id)))
            .ToList();

        var revenues = await _context.Payments
            .AsNoTracking()
            .Where(p => courseIds.Contains(p.CourseId) && p.Status == PaymentStatus.Completed)
            .GroupBy(p => p.CourseId)
            .Select(g => new { CourseId = g.Key, Revenue = g.Sum(p => p.Amount) })
            .ToListAsync(cancellationToken);

        var revenueByCourse = revenues.ToDictionary(x => x.CourseId, x => x.Revenue);

        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Where(e => courseIds.Contains(e.CourseId))
            .Select(e => new { e.CourseId, e.UserId })
            .ToListAsync(cancellationToken);

        var completedProgress = await _context.LessonProgresses
            .AsNoTracking()
            .Where(lp => lp.IsCompleted && allLessonIds.Contains(lp.LessonId))
            .Select(lp => new { lp.UserId, lp.LessonId })
            .ToListAsync(cancellationToken);

        var anyProgress = await _context.LessonProgresses
            .AsNoTracking()
            .Where(lp => allLessonIds.Contains(lp.LessonId))
            .Select(lp => new { lp.UserId, lp.LessonId })
            .ToListAsync(cancellationToken);

        var lessonToCourseId = courses
            .SelectMany(c => c.Modules.SelectMany(m => m.Lessons.Select(l => (l.Id, CourseId: c.Id))))
            .ToDictionary(x => x.Id, x => x.CourseId);

        var learnersByCourse = enrollments
            .GroupBy(e => e.CourseId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.UserId).ToHashSet());

        foreach (var progress in anyProgress)
        {
            if (!lessonToCourseId.TryGetValue(progress.LessonId, out var courseId))
            {
                continue;
            }

            if (!learnersByCourse.TryGetValue(courseId, out var learners))
            {
                learners = new HashSet<Guid>();
                learnersByCourse[courseId] = learners;
            }

            learners.Add(progress.UserId);
        }

        var completedByLesson = completedProgress
            .GroupBy(p => p.LessonId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.UserId).ToHashSet());

        var resultCourses = new List<InstructorCourseAnalyticsDto>();

        foreach (var course in courses)
        {
            var lessons = course.Modules
                .OrderBy(m => m.Order)
                .SelectMany(m => m.Lessons
                    .OrderBy(l => l.Order)
                    .Select(l => (Lesson: l, Module: m)))
                .ToList();

            var learnerUserIds = learnersByCourse.GetValueOrDefault(course.Id) ?? new HashSet<Guid>();
            var enrollmentCount = enrollments.Count(e => e.CourseId == course.Id);
            var learnerCount = learnerUserIds.Count;
            var totalLessons = lessons.Count;

            var fullyCompletedUsers = 0;
            if (totalLessons > 0 && learnerCount > 0)
            {
                fullyCompletedUsers = learnerUserIds.Count(userId =>
                    lessons.All(item =>
                        completedByLesson.TryGetValue(item.Lesson.Id, out var users)
                        && users.Contains(userId)));
            }

            var completionRate = learnerCount > 0
                ? Math.Round(fullyCompletedUsers * 100.0 / learnerCount, 2)
                : 0;

            var dropOff = new List<LessonDropOffDto>();
            for (var i = 0; i < lessons.Count; i++)
            {
                var (lesson, module) = lessons[i];
                int reachedCount;
                if (i == 0)
                {
                    reachedCount = learnerCount;
                }
                else
                {
                    var previousId = lessons[i - 1].Lesson.Id;
                    reachedCount = completedByLesson.TryGetValue(previousId, out var prevUsers)
                        ? prevUsers.Count(u => learnerUserIds.Contains(u))
                        : 0;
                }

                var completedCount = completedByLesson.TryGetValue(lesson.Id, out var doneUsers)
                    ? doneUsers.Count(u => learnerUserIds.Contains(u))
                    : 0;

                var dropOffPercent = reachedCount > 0
                    ? Math.Round((reachedCount - completedCount) * 100.0 / reachedCount, 2)
                    : 0;

                dropOff.Add(new LessonDropOffDto(
                    lesson.Id,
                    lesson.Title,
                    module.Id,
                    module.Title,
                    i + 1,
                    reachedCount,
                    completedCount,
                    dropOffPercent));
            }

            resultCourses.Add(new InstructorCourseAnalyticsDto(
                course.Id,
                course.Title,
                enrollmentCount,
                revenueByCourse.GetValueOrDefault(course.Id),
                completionRate,
                dropOff));
        }

        var averageCompletion = resultCourses.Count > 0
            ? Math.Round(resultCourses.Average(c => c.CompletionRate), 2)
            : 0;

        return new InstructorAnalyticsDto(
            resultCourses.Sum(c => c.Revenue),
            averageCompletion,
            resultCourses);
    }
}

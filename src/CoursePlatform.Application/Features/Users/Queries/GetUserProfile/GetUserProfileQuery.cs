using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;

namespace CoursePlatform.Application.Features.Users.Queries.GetUserProfile;

public record GetUserProfileQuery : IRequest<UserProfileDto>;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUserProfileQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
            throw new ForbiddenAccessException();

        var userId = _currentUserService.UserId.Value;

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new NotFoundException("User", userId.ToString());

        var statistics = await ComputeStatisticsAsync(userId, cancellationToken);

        return new UserProfileDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CreatedAt,
            statistics
        );
    }

    private async Task<UserStatisticsDto> ComputeStatisticsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Select(e => new
            {
                e.EnrolledAt,
                Lessons = e.Course.Modules
                    .SelectMany(m => m.Lessons)
                    .Select(l => new { l.Id, l.Duration })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var completedProgress = await _context.LessonProgresses
            .AsNoTracking()
            .Where(lp => lp.UserId == userId && lp.IsCompleted)
            .Select(lp => new { lp.LessonId, lp.CompletedAt })
            .ToListAsync(cancellationToken);

        var completedAtByLesson = completedProgress
            .GroupBy(p => p.LessonId)
            .ToDictionary(g => g.Key, g => g.Max(x => x.CompletedAt));
        var completedLessonIds = completedAtByLesson.Keys.ToHashSet();

        var latestReviewAt = await _context.Reviews
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .Select(r => (DateTime?)r.CreatedAt)
            .OrderByDescending(d => d)
            .FirstOrDefaultAsync(cancellationToken);

        int totalEnrollments = enrollments.Count;
        int totalLessonsAvailable = 0;
        int totalLessonsCompleted = 0;
        int completedCourses = 0;
        int totalLearningTimeSeconds = 0;
        DateTime? lastActivityAt = latestReviewAt;

        foreach (var enrollment in enrollments)
        {
            totalLessonsAvailable += enrollment.Lessons.Count;

            var completedInCourse = enrollment.Lessons
                .Where(l => completedLessonIds.Contains(l.Id))
                .ToList();

            totalLessonsCompleted += completedInCourse.Count;
            totalLearningTimeSeconds += completedInCourse.Sum(l => l.Duration);

            if (enrollment.Lessons.Count > 0 && completedInCourse.Count == enrollment.Lessons.Count)
            {
                completedCourses++;
            }

            if (lastActivityAt == null || enrollment.EnrolledAt > lastActivityAt)
            {
                lastActivityAt = enrollment.EnrolledAt;
            }

            foreach (var lesson in completedInCourse)
            {
                var completedAt = completedAtByLesson[lesson.Id];
                if (completedAt.HasValue && (lastActivityAt == null || completedAt.Value > lastActivityAt))
                {
                    lastActivityAt = completedAt.Value;
                }
            }
        }

        double averageProgress = totalLessonsAvailable > 0
            ? (double)totalLessonsCompleted / totalLessonsAvailable * 100
            : 0;

        return new UserStatisticsDto(
            totalEnrollments,
            completedCourses,
            totalLessonsCompleted,
            totalLessonsAvailable,
            Math.Round(averageProgress, 2),
            totalLearningTimeSeconds,
            completedCourses,
            lastActivityAt
        );
    }
}

public record UserProfileDto(
    Guid Id,
    string? Email,
    string FirstName,
    string LastName,
    DateTime CreatedAt,
    UserStatisticsDto Statistics
);

public record UserStatisticsDto(
    int TotalEnrollments,
    int CompletedCourses,
    int TotalLessonsCompleted,
    int TotalLessonsAvailable,
    double AverageProgressPercentage,
    int TotalLearningTimeSeconds,
    int CertificatesEarned,
    DateTime? LastActivityAt
);

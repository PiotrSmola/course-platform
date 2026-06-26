using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Domain.Entities;
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

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Statistics)
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value, cancellationToken);

        if (user == null)
            throw new NotFoundException("User", _currentUserService.UserId.Value.ToString());

        var stats = user.Statistics;

        if (stats == null)
        {
            stats = await ComputeStatisticsAsync(user.Id, cancellationToken);
        }

        return new UserProfileDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CreatedAt,
            new UserStatisticsDto(
                stats.TotalEnrollments,
                stats.CompletedCourses,
                stats.TotalLessonsCompleted,
                stats.TotalLessonsAvailable,
                stats.AverageProgressPercentage,
                stats.TotalLearningTimeSeconds,
                stats.CertificatesEarned,
                stats.LastActivityAt
            )
        );
    }

    private async Task<UserStatistics> ComputeStatisticsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Include(e => e.Course)
            .ThenInclude(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .ToListAsync(cancellationToken);

        var lessonProgress = await _context.LessonProgresses
            .AsNoTracking()
            .Where(lp => lp.UserId == userId)
            .ToListAsync(cancellationToken);

        var completedLessonIds = lessonProgress.Where(lp => lp.IsCompleted).Select(lp => lp.LessonId).ToHashSet();

        int totalEnrollments = enrollments.Count;
        int totalLessonsAvailable = 0;
        int totalLessonsCompleted = completedLessonIds.Count;
        int completedCourses = 0;
        int totalLearningTimeSeconds = 0;

        foreach (var enrollment in enrollments)
        {
            var courseLessons = enrollment.Course.Modules.SelectMany(m => m.Lessons).ToList();
            totalLessonsAvailable += courseLessons.Count;

            var courseLessonIds = courseLessons.Select(l => l.Id).ToHashSet();
            var completedInCourse = completedLessonIds.Count(cid => courseLessonIds.Contains(cid));

            if (courseLessons.Count > 0 && completedInCourse == courseLessons.Count)
            {
                completedCourses++;
            }

            foreach (var lesson in courseLessons.Where(l => completedLessonIds.Contains(l.Id)))
            {
                totalLearningTimeSeconds += lesson.Duration;
            }
        }

        double averageProgress = totalLessonsAvailable > 0
            ? (double)totalLessonsCompleted / totalLessonsAvailable * 100
            : 0;

        var stats = new UserStatistics
        {
            UserId = userId,
            TotalEnrollments = totalEnrollments,
            CompletedCourses = completedCourses,
            TotalLessonsCompleted = totalLessonsCompleted,
            TotalLessonsAvailable = totalLessonsAvailable,
            AverageProgressPercentage = Math.Round(averageProgress, 2),
            TotalLearningTimeSeconds = totalLearningTimeSeconds,
            CertificatesEarned = completedCourses,
            LastActivityAt = DateTime.UtcNow
        };

        _context.UserStatistics.Add(stats);
        await _context.SaveChangesAsync(cancellationToken);

        return stats;
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

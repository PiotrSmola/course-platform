using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Application.Common.Helpers;

public static class CourseContentAccessHelper
{
    public static async Task<CourseContentAccess> GetAsync(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        Guid courseId,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId == null)
        {
            return new CourseContentAccess(CourseContentAccessLevel.None);
        }

        if (currentUser.IsAdmin)
        {
            return new CourseContentAccess(CourseContentAccessLevel.Full);
        }

        var userId = currentUser.UserId.Value;
        var isOwner = await context.Courses
            .AnyAsync(course => course.Id == courseId && course.InstructorId == userId, cancellationToken);

        if (isOwner)
        {
            return new CourseContentAccess(CourseContentAccessLevel.Full);
        }

        var isEnrolled = await context.Enrollments
            .AnyAsync(enrollment => enrollment.UserId == userId && enrollment.CourseId == courseId, cancellationToken);

        if (isEnrolled || await CourseAccessHelper.HasActiveSubscriptionAsync(context, userId, cancellationToken))
        {
            return new CourseContentAccess(CourseContentAccessLevel.Full);
        }

        var hasTrial = await context.TrialAccesses
            .AsNoTracking()
            .AnyAsync(access => access.UserId == userId && access.CourseId == courseId, cancellationToken);

        return new CourseContentAccess(
            hasTrial ? CourseContentAccessLevel.Trial : CourseContentAccessLevel.None);
    }

    public static async Task EnsureCanViewLessonAsync(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        Guid courseId,
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var access = await GetAsync(context, currentUser, courseId, cancellationToken);
        if (!access.CanViewLessons)
        {
            throw new ForbiddenAccessException("You are not enrolled in this course.");
        }

        if (access.HasFullAccess)
        {
            await ProgressGateHelper.EnsureLessonUnlockedAsync(
                context, currentUser, courseId, lessonId, cancellationToken);
            return;
        }

        var trialLessonIds = await GetTrialLessonIdsAsync(context, courseId, cancellationToken);
        if (!trialLessonIds.Contains(lessonId))
        {
            throw new ForbiddenAccessException("This lesson is not included in your trial.");
        }
    }

    public static async Task<Dictionary<Guid, (bool IsLocked, string? LockReason)>> GetTrialLockStatesAsync(
        IApplicationDbContext context,
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var orderedLessonIds = await ProgressGateHelper.GetOrderedLessonIdsAsync(
            context, courseId, cancellationToken);
        var trialLessonIds = orderedLessonIds.Take(2).ToHashSet();

        return orderedLessonIds.ToDictionary(
            lessonId => lessonId,
            lessonId => trialLessonIds.Contains(lessonId)
                ? (false, (string?)null)
                : (true, (string?)"Dostępne w wersji próbnej są tylko dwie pierwsze lekcje."));
    }

    private static async Task<List<Guid>> GetTrialLessonIdsAsync(
        IApplicationDbContext context,
        Guid courseId,
        CancellationToken cancellationToken)
    {
        return (await ProgressGateHelper.GetOrderedLessonIdsAsync(context, courseId, cancellationToken))
            .Take(2)
            .ToList();
    }
}

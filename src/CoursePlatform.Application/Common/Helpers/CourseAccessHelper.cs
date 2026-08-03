using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Common.Helpers;

public enum CourseContentAccessLevel
{
    None = 0,
    Trial = 1,
    Full = 2
}

public readonly record struct CourseContentAccess(CourseContentAccessLevel Level)
{
    public bool CanViewLessons => Level != CourseContentAccessLevel.None;
    public bool HasFullAccess => Level == CourseContentAccessLevel.Full;
    public bool IsTrial => Level == CourseContentAccessLevel.Trial;
}

public static class CourseAccessHelper
{
    public static async Task<Course> GetManagedCourseAsync(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        Guid courseId,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var course = await context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);

        if (course == null)
        {
            throw new NotFoundException($"Course {courseId} not found.");
        }

        if (!currentUser.IsAdmin && course.InstructorId != currentUser.UserId.Value)
        {
            throw new ForbiddenAccessException("You are not the instructor of this course.");
        }

        return course;
    }

    public static async Task<bool> CanAccessCourseContentAsync(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var access = await CourseContentAccessHelper.GetAsync(
            context, currentUser, courseId, cancellationToken);
        return access.HasFullAccess;
    }

    public static async Task<bool> HasActiveSubscriptionAsync(
        IApplicationDbContext context,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        return await context.Subscriptions.AnyAsync(
            s => s.UserId == userId
                && s.CurrentPeriodEnd > now
                && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.PastDue),
            cancellationToken);
    }

    public static async Task<Module> GetManagedModuleAsync(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        Guid courseId,
        Guid moduleId,
        CancellationToken cancellationToken)
    {
        await GetManagedCourseAsync(context, currentUser, courseId, cancellationToken);

        var module = await context.Modules
            .FirstOrDefaultAsync(m => m.Id == moduleId && m.CourseId == courseId, cancellationToken);

        if (module == null)
        {
            throw new NotFoundException($"Module {moduleId} not found.");
        }

        return module;
    }
}

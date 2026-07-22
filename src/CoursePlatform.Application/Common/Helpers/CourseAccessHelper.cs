using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Common.Helpers;

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
        if (currentUser.UserId == null)
        {
            return false;
        }

        if (currentUser.IsAdmin)
        {
            return true;
        }

        var userId = currentUser.UserId.Value;

        var isOwner = await context.Courses
            .AnyAsync(c => c.Id == courseId && c.InstructorId == userId, cancellationToken);

        if (isOwner)
        {
            return true;
        }

        var isEnrolled = await context.Enrollments
            .AnyAsync(e => e.CourseId == courseId && e.UserId == userId, cancellationToken);

        if (isEnrolled)
        {
            return true;
        }

        return await HasActiveSubscriptionAsync(context, userId, cancellationToken);
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

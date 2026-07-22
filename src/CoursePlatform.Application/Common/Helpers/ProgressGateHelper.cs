using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Common.Helpers;

public static class ProgressGateHelper
{
    public static async Task<List<Guid>> GetOrderedLessonIdsAsync(
        IApplicationDbContext context,
        Guid courseId,
        CancellationToken cancellationToken)
    {
        return await context.Modules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.Order)
            .SelectMany(m => m.Lessons.OrderBy(l => l.Order).Select(l => l.Id))
            .ToListAsync(cancellationToken);
    }

    public static async Task EnsureLessonUnlockedAsync(
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

        if (await IsBypassedAsync(context, currentUser, courseId, cancellationToken))
        {
            return;
        }

        var ordered = await GetOrderedLessonIdsAsync(context, courseId, cancellationToken);
        var index = ordered.IndexOf(lessonId);
        if (index < 0)
        {
            throw new NotFoundException($"Lesson {lessonId} not found.");
        }

        if (index == 0)
        {
            return;
        }

        var previousLessonId = ordered[index - 1];
        var unlocked = await IsLessonRequirementMetAsync(
            context, currentUser.UserId.Value, previousLessonId, cancellationToken);

        if (!unlocked)
        {
            throw new ForbiddenAccessException(
                "Complete the previous lesson (and pass its quiz if required) to unlock this lesson.");
        }
    }

    public static async Task EnsureCanCompleteLessonAsync(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        Guid courseId,
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        await EnsureLessonUnlockedAsync(context, currentUser, courseId, lessonId, cancellationToken);

        if (currentUser.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        if (await IsBypassedAsync(context, currentUser, courseId, cancellationToken))
        {
            return;
        }

        var hasQuiz = await context.Quizzes.AnyAsync(q => q.LessonId == lessonId, cancellationToken);
        if (!hasQuiz)
        {
            return;
        }

        var passed = await context.QuizAttempts.AnyAsync(
            a => a.Quiz.LessonId == lessonId
                 && a.UserId == currentUser.UserId.Value
                 && a.Passed,
            cancellationToken);

        if (!passed)
        {
            throw new ForbiddenAccessException("Pass the lesson quiz before marking it as completed.");
        }
    }

    public static async Task<Dictionary<Guid, (bool IsLocked, string? LockReason)>> GetLessonLockStatesAsync(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var ordered = await GetOrderedLessonIdsAsync(context, courseId, cancellationToken);
        var result = ordered.ToDictionary(id => id, _ => (false, (string?)null));

        if (currentUser.UserId == null
            || await IsBypassedAsync(context, currentUser, courseId, cancellationToken))
        {
            return result;
        }

        var userId = currentUser.UserId.Value;
        var completedIds = await context.LessonProgresses
            .AsNoTracking()
            .Where(lp => lp.UserId == userId && lp.IsCompleted && ordered.Contains(lp.LessonId))
            .Select(lp => lp.LessonId)
            .ToListAsync(cancellationToken);

        var completed = completedIds.ToHashSet();

        var quizLessonIds = await context.Quizzes
            .AsNoTracking()
            .Where(q => ordered.Contains(q.LessonId))
            .Select(q => q.LessonId)
            .ToListAsync(cancellationToken);

        var quizLessons = quizLessonIds.ToHashSet();

        var passedQuizLessonIds = await context.QuizAttempts
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.Passed && quizLessons.Contains(a.Quiz.LessonId))
            .Select(a => a.Quiz.LessonId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var passedQuizzes = passedQuizLessonIds.ToHashSet();

        for (var i = 1; i < ordered.Count; i++)
        {
            var previousId = ordered[i - 1];
            var currentId = ordered[i];
            var previousOk = completed.Contains(previousId)
                && (!quizLessons.Contains(previousId) || passedQuizzes.Contains(previousId));

            if (!previousOk)
            {
                result[currentId] = (true, quizLessons.Contains(previousId) && !passedQuizzes.Contains(previousId)
                    ? "Najpierw ukończ poprzednią lekcję i zalicz quiz"
                    : "Najpierw ukończ poprzednią lekcję");
            }
        }

        return result;
    }

    private static async Task<bool> IsLessonRequirementMetAsync(
        IApplicationDbContext context,
        Guid userId,
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var completed = await context.LessonProgresses.AnyAsync(
            lp => lp.UserId == userId && lp.LessonId == lessonId && lp.IsCompleted,
            cancellationToken);

        if (!completed)
        {
            return false;
        }

        var hasQuiz = await context.Quizzes.AnyAsync(q => q.LessonId == lessonId, cancellationToken);
        if (!hasQuiz)
        {
            return true;
        }

        return await context.QuizAttempts.AnyAsync(
            a => a.Quiz.LessonId == lessonId && a.UserId == userId && a.Passed,
            cancellationToken);
    }

    private static async Task<bool> IsBypassedAsync(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        Guid courseId,
        CancellationToken cancellationToken)
    {
        if (currentUser.IsAdmin)
        {
            return true;
        }

        if (currentUser.UserId == null)
        {
            return false;
        }

        return await context.Courses.AnyAsync(
            c => c.Id == courseId && c.InstructorId == currentUser.UserId.Value,
            cancellationToken);
    }
}

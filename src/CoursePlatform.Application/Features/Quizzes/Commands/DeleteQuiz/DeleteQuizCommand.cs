using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Quizzes.Commands.DeleteQuiz;

public record DeleteQuizCommand(Guid CourseId, Guid LessonId) : IRequest;

public class DeleteQuizCommandHandler : IRequestHandler<DeleteQuizCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteQuizCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteQuizCommand request, CancellationToken cancellationToken)
    {
        await CourseAccessHelper.GetManagedCourseAsync(
            _context, _currentUserService, request.CourseId, cancellationToken);

        var quiz = await _context.Quizzes
            .Include(q => q.Attempts)
                .ThenInclude(a => a.Answers)
            .Include(q => q.Questions)
                .ThenInclude(qq => qq.Options)
            .FirstOrDefaultAsync(
                q => q.LessonId == request.LessonId && q.Lesson.Module.CourseId == request.CourseId,
                cancellationToken);

        if (quiz == null)
        {
            throw new NotFoundException($"Quiz for lesson {request.LessonId} not found.");
        }

        foreach (var attempt in quiz.Attempts)
        {
            _context.QuizAttemptAnswers.RemoveRange(attempt.Answers);
        }
        _context.QuizAttempts.RemoveRange(quiz.Attempts);

        foreach (var question in quiz.Questions)
        {
            _context.QuizOptions.RemoveRange(question.Options);
        }
        _context.QuizQuestions.RemoveRange(quiz.Questions);
        _context.Quizzes.Remove(quiz);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

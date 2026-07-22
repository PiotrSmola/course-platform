using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Quizzes.Commands.UpsertQuiz;

public record UpsertQuizCommand(
    Guid CourseId,
    Guid LessonId,
    string Title,
    int PassThresholdPercent,
    IReadOnlyList<QuizQuestionInputDto> Questions) : IRequest<Guid>;

public class UpsertQuizCommandHandler : IRequestHandler<UpsertQuizCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHtmlSanitizer _htmlSanitizer;

    public UpsertQuizCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IHtmlSanitizer htmlSanitizer)
    {
        _context = context;
        _currentUserService = currentUserService;
        _htmlSanitizer = htmlSanitizer;
    }

    public async Task<Guid> Handle(UpsertQuizCommand request, CancellationToken cancellationToken)
    {
        await CourseAccessHelper.GetManagedCourseAsync(
            _context, _currentUserService, request.CourseId, cancellationToken);

        var lesson = await _context.Lessons
            .Include(l => l.Quiz!)
                .ThenInclude(q => q.Questions)
                    .ThenInclude(qq => qq.Options)
            .FirstOrDefaultAsync(
                l => l.Id == request.LessonId && l.Module.CourseId == request.CourseId,
                cancellationToken);

        if (lesson == null)
        {
            throw new NotFoundException($"Lesson {request.LessonId} not found.");
        }

        foreach (var question in request.Questions)
        {
            if (question.Options.Count < 2)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("Questions", "Każde pytanie musi mieć co najmniej 2 opcje.")
                });
            }

            if (!question.Options.Any(o => o.IsCorrect))
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("Questions", "Każde pytanie musi mieć co najmniej jedną poprawną odpowiedź.")
                });
            }
        }

        Quiz quiz;
        if (lesson.Quiz == null)
        {
            quiz = new Quiz { LessonId = lesson.Id };
            _context.Quizzes.Add(quiz);
        }
        else
        {
            quiz = lesson.Quiz;

            var attemptIds = await _context.QuizAttempts
                .Where(a => a.QuizId == quiz.Id)
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);

            if (attemptIds.Count > 0)
            {
                var answers = await _context.QuizAttemptAnswers
                    .Where(a => attemptIds.Contains(a.AttemptId))
                    .ToListAsync(cancellationToken);
                _context.QuizAttemptAnswers.RemoveRange(answers);

                var attempts = await _context.QuizAttempts
                    .Where(a => a.QuizId == quiz.Id)
                    .ToListAsync(cancellationToken);
                _context.QuizAttempts.RemoveRange(attempts);
            }

            var existingQuestions = quiz.Questions.ToList();
            foreach (var existing in existingQuestions)
            {
                _context.QuizOptions.RemoveRange(existing.Options);
            }
            _context.QuizQuestions.RemoveRange(existingQuestions);
        }

        quiz.Title = _htmlSanitizer.Sanitize(request.Title);
        quiz.PassThresholdPercent = request.PassThresholdPercent;
        quiz.MarkUpdated();

        foreach (var questionInput in request.Questions.OrderBy(q => q.Order))
        {
            var question = new QuizQuestion
            {
                Quiz = quiz,
                Prompt = _htmlSanitizer.Sanitize(questionInput.Prompt),
                Order = questionInput.Order
            };

            foreach (var optionInput in questionInput.Options.OrderBy(o => o.Order))
            {
                question.Options.Add(new QuizOption
                {
                    Text = _htmlSanitizer.Sanitize(optionInput.Text),
                    IsCorrect = optionInput.IsCorrect,
                    Order = optionInput.Order
                });
            }

            quiz.Questions.Add(question);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return quiz.Id;
    }
}

using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Quizzes.Commands.SubmitQuizAttempt;

public record SubmitQuizAttemptCommand(
    Guid CourseId,
    Guid LessonId,
    IReadOnlyList<QuizAttemptAnswerInputDto> Answers) : IRequest<QuizAttemptResultDto>;

public class SubmitQuizAttemptCommandHandler : IRequestHandler<SubmitQuizAttemptCommand, QuizAttemptResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SubmitQuizAttemptCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<QuizAttemptResultDto> Handle(SubmitQuizAttemptCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var hasAccess = await CourseAccessHelper.CanAccessLessonContentAsync(
            _context, _currentUserService, request.CourseId, request.LessonId, cancellationToken);

        if (!hasAccess)
        {
            throw new ForbiddenAccessException("You must be enrolled in the course to take the quiz.");
        }

        await ProgressGateHelper.EnsureLessonUnlockedAsync(
            _context, _currentUserService, request.CourseId, request.LessonId, cancellationToken);

        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
                .ThenInclude(qq => qq.Options)
            .FirstOrDefaultAsync(
                q => q.LessonId == request.LessonId && q.Lesson.Module.CourseId == request.CourseId,
                cancellationToken);

        if (quiz == null)
        {
            throw new NotFoundException($"Quiz for lesson {request.LessonId} not found.");
        }

        if (quiz.Questions.Count == 0)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("Answers", "Quiz nie zawiera pytań.")
            });
        }

        var answersByQuestion = request.Answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.Last());

        if (answersByQuestion.Count != quiz.Questions.Count
            || quiz.Questions.Any(q => !answersByQuestion.ContainsKey(q.Id)))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("Answers", "Musisz odpowiedzieć na wszystkie pytania.")
            });
        }

        var correctCount = 0;
        var attemptAnswers = new List<QuizAttemptAnswer>();

        foreach (var question in quiz.Questions)
        {
            var selectedOptionId = answersByQuestion[question.Id].SelectedOptionId;
            var selectedOption = question.Options.FirstOrDefault(o => o.Id == selectedOptionId);

            if (selectedOption == null)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("Answers", "Wybrano nieprawidłową opcję odpowiedzi.")
                });
            }

            if (selectedOption.IsCorrect)
            {
                correctCount++;
            }

            attemptAnswers.Add(new QuizAttemptAnswer
            {
                QuestionId = question.Id,
                SelectedOptionId = selectedOption.Id
            });
        }

        var scorePercent = (int)Math.Round(correctCount * 100.0 / quiz.Questions.Count);
        var passed = scorePercent >= quiz.PassThresholdPercent;

        var attempt = new QuizAttempt
        {
            QuizId = quiz.Id,
            UserId = _currentUserService.UserId.Value,
            ScorePercent = scorePercent,
            Passed = passed,
            SubmittedAt = DateTime.UtcNow,
            Answers = attemptAnswers
        };

        _context.QuizAttempts.Add(attempt);
        await _context.SaveChangesAsync(cancellationToken);

        return new QuizAttemptResultDto(attempt.Id, scorePercent, passed, attempt.SubmittedAt);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Quizzes.Queries.GetMyQuizAttempts;

public record GetMyQuizAttemptsQuery(Guid CourseId, Guid LessonId) : IRequest<IReadOnlyList<QuizAttemptListItemDto>>;

public class GetMyQuizAttemptsQueryHandler
    : IRequestHandler<GetMyQuizAttemptsQuery, IReadOnlyList<QuizAttemptListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyQuizAttemptsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<QuizAttemptListItemDto>> Handle(
        GetMyQuizAttemptsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var hasAccess = await CourseAccessHelper.CanAccessCourseContentAsync(
            _context, _currentUserService, request.CourseId, cancellationToken);

        if (!hasAccess)
        {
            throw new ForbiddenAccessException("You must be enrolled in the course to view quiz attempts.");
        }

        var quizId = await _context.Quizzes
            .Where(q => q.LessonId == request.LessonId && q.Lesson.Module.CourseId == request.CourseId)
            .Select(q => (Guid?)q.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (quizId == null)
        {
            return Array.Empty<QuizAttemptListItemDto>();
        }

        return await _context.QuizAttempts
            .AsNoTracking()
            .Where(a => a.QuizId == quizId && a.UserId == _currentUserService.UserId.Value)
            .OrderByDescending(a => a.SubmittedAt)
            .Select(a => new QuizAttemptListItemDto(a.Id, a.ScorePercent, a.Passed, a.SubmittedAt))
            .ToListAsync(cancellationToken);
    }
}

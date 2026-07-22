using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Quizzes.Queries.GetLessonQuiz;

public record GetLessonQuizQuery(Guid CourseId, Guid LessonId) : IRequest<LessonQuizStudentDto?>;

public class GetLessonQuizQueryHandler : IRequestHandler<GetLessonQuizQuery, LessonQuizStudentDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetLessonQuizQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<LessonQuizStudentDto?> Handle(GetLessonQuizQuery request, CancellationToken cancellationToken)
    {
        var hasAccess = await CourseAccessHelper.CanAccessCourseContentAsync(
            _context, _currentUserService, request.CourseId, cancellationToken);

        if (!hasAccess)
        {
            throw new ForbiddenAccessException("You must be enrolled in the course to view the quiz.");
        }

        await ProgressGateHelper.EnsureLessonUnlockedAsync(
            _context, _currentUserService, request.CourseId, request.LessonId, cancellationToken);

        var lessonExists = await _context.Lessons
            .AnyAsync(l => l.Id == request.LessonId && l.Module.CourseId == request.CourseId, cancellationToken);

        if (!lessonExists)
        {
            throw new NotFoundException($"Lesson {request.LessonId} not found.");
        }

        var quiz = await _context.Quizzes
            .AsNoTracking()
            .Where(q => q.LessonId == request.LessonId)
            .Select(q => new LessonQuizStudentDto(
                q.Id,
                q.Title,
                q.PassThresholdPercent,
                q.Questions
                    .OrderBy(qq => qq.Order)
                    .Select(qq => new QuizQuestionStudentDto(
                        qq.Id,
                        qq.Prompt,
                        qq.Order,
                        qq.Options
                            .OrderBy(o => o.Order)
                            .Select(o => new QuizOptionStudentDto(o.Id, o.Text, o.Order))
                            .ToList()
                    ))
                    .ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return quiz;
    }
}

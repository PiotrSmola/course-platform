using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Quizzes.Queries.GetLessonQuizForInstructor;

public record GetLessonQuizForInstructorQuery(Guid CourseId, Guid LessonId) : IRequest<LessonQuizInstructorDto?>;

public class GetLessonQuizForInstructorQueryHandler
    : IRequestHandler<GetLessonQuizForInstructorQuery, LessonQuizInstructorDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetLessonQuizForInstructorQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<LessonQuizInstructorDto?> Handle(
        GetLessonQuizForInstructorQuery request,
        CancellationToken cancellationToken)
    {
        await CourseAccessHelper.GetManagedCourseAsync(
            _context, _currentUserService, request.CourseId, cancellationToken);

        var lessonExists = await _context.Lessons
            .AnyAsync(l => l.Id == request.LessonId && l.Module.CourseId == request.CourseId, cancellationToken);

        if (!lessonExists)
        {
            throw new NotFoundException($"Lesson {request.LessonId} not found.");
        }

        var quiz = await _context.Quizzes
            .AsNoTracking()
            .Where(q => q.LessonId == request.LessonId)
            .Select(q => new LessonQuizInstructorDto(
                q.Id,
                q.Title,
                q.PassThresholdPercent,
                q.Questions
                    .OrderBy(qq => qq.Order)
                    .Select(qq => new QuizQuestionInstructorDto(
                        qq.Id,
                        qq.Prompt,
                        qq.Order,
                        qq.Options
                            .OrderBy(o => o.Order)
                            .Select(o => new QuizOptionInstructorDto(o.Id, o.Text, o.IsCorrect, o.Order))
                            .ToList()
                    ))
                    .ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return quiz;
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.LessonDiscussions.Commands.CreateLessonAnswer;

public record CreateLessonAnswerCommand(Guid CourseId, Guid LessonId, Guid QuestionId, string Body) : IRequest<Guid>;

public class CreateLessonAnswerCommandHandler : IRequestHandler<CreateLessonAnswerCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHtmlSanitizer _htmlSanitizer;

    public CreateLessonAnswerCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IHtmlSanitizer htmlSanitizer)
    {
        _context = context;
        _currentUserService = currentUserService;
        _htmlSanitizer = htmlSanitizer;
    }

    public async Task<Guid> Handle(CreateLessonAnswerCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var hasAccess = await CourseAccessHelper.CanAccessCourseContentAsync(
            _context, _currentUserService, request.CourseId, cancellationToken);

        if (!hasAccess)
        {
            throw new ForbiddenAccessException("You must be enrolled in the course to answer a question.");
        }

        var question = await _context.LessonQuestions
            .Include(q => q.Lesson)
                .ThenInclude(l => l.Module)
            .FirstOrDefaultAsync(
                q => q.Id == request.QuestionId
                     && q.LessonId == request.LessonId
                     && q.Lesson.Module.CourseId == request.CourseId
                     && !q.IsDeleted,
                cancellationToken);

        if (question == null)
        {
            throw new NotFoundException($"Question {request.QuestionId} not found.");
        }

        var isInstructorAnswer = _currentUserService.IsAdmin
            || await _context.Courses.AnyAsync(
                c => c.Id == request.CourseId && c.InstructorId == _currentUserService.UserId.Value,
                cancellationToken);

        var answer = new LessonAnswer
        {
            QuestionId = question.Id,
            AuthorId = _currentUserService.UserId.Value,
            Body = _htmlSanitizer.Sanitize(request.Body),
            IsInstructorAnswer = isInstructorAnswer
        };

        _context.LessonAnswers.Add(answer);
        await _context.SaveChangesAsync(cancellationToken);

        return answer.Id;
    }
}

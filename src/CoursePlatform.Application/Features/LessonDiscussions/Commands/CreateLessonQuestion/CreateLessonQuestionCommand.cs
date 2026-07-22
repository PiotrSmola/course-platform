using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.LessonDiscussions.Commands.CreateLessonQuestion;

public record CreateLessonQuestionCommand(Guid CourseId, Guid LessonId, string Body) : IRequest<Guid>;

public class CreateLessonQuestionCommandHandler : IRequestHandler<CreateLessonQuestionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHtmlSanitizer _htmlSanitizer;

    public CreateLessonQuestionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IHtmlSanitizer htmlSanitizer)
    {
        _context = context;
        _currentUserService = currentUserService;
        _htmlSanitizer = htmlSanitizer;
    }

    public async Task<Guid> Handle(CreateLessonQuestionCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var hasAccess = await CourseAccessHelper.CanAccessCourseContentAsync(
            _context, _currentUserService, request.CourseId, cancellationToken);

        if (!hasAccess)
        {
            throw new ForbiddenAccessException("You must be enrolled in the course to ask a question.");
        }

        var lessonExists = await _context.Lessons
            .AnyAsync(l => l.Id == request.LessonId && l.Module.CourseId == request.CourseId, cancellationToken);

        if (!lessonExists)
        {
            throw new NotFoundException($"Lesson {request.LessonId} not found.");
        }

        var question = new LessonQuestion
        {
            LessonId = request.LessonId,
            AuthorId = _currentUserService.UserId.Value,
            Body = _htmlSanitizer.Sanitize(request.Body)
        };

        _context.LessonQuestions.Add(question);
        await _context.SaveChangesAsync(cancellationToken);

        return question.Id;
    }
}

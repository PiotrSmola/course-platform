using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.LessonDiscussions.Commands.DeleteLessonAnswer;

public record DeleteLessonAnswerCommand(Guid CourseId, Guid LessonId, Guid QuestionId, Guid AnswerId) : IRequest;

public class DeleteLessonAnswerCommandHandler : IRequestHandler<DeleteLessonAnswerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteLessonAnswerCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteLessonAnswerCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var answer = await _context.LessonAnswers
            .Include(a => a.Question)
                .ThenInclude(q => q.Lesson)
                    .ThenInclude(l => l.Module)
                        .ThenInclude(m => m.Course)
            .FirstOrDefaultAsync(
                a => a.Id == request.AnswerId
                     && a.QuestionId == request.QuestionId
                     && a.Question.LessonId == request.LessonId
                     && a.Question.Lesson.Module.CourseId == request.CourseId
                     && !a.IsDeleted,
                cancellationToken);

        if (answer == null)
        {
            throw new NotFoundException($"Answer {request.AnswerId} not found.");
        }

        var userId = _currentUserService.UserId.Value;
        var canModerate = _currentUserService.IsAdmin
            || answer.Question.Lesson.Module.Course.InstructorId == userId;

        if (answer.AuthorId != userId && !canModerate)
        {
            throw new ForbiddenAccessException("You can only delete your own answer.");
        }

        answer.IsDeleted = true;
        answer.MarkUpdated();
        await _context.SaveChangesAsync(cancellationToken);
    }
}

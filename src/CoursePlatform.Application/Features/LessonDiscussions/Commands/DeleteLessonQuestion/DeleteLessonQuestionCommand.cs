using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.LessonDiscussions.Commands.DeleteLessonQuestion;

public record DeleteLessonQuestionCommand(Guid CourseId, Guid LessonId, Guid QuestionId) : IRequest;

public class DeleteLessonQuestionCommandHandler : IRequestHandler<DeleteLessonQuestionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteLessonQuestionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteLessonQuestionCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var question = await _context.LessonQuestions
            .Include(q => q.Lesson)
                .ThenInclude(l => l.Module)
                    .ThenInclude(m => m.Course)
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

        var userId = _currentUserService.UserId.Value;
        var canModerate = _currentUserService.IsAdmin
            || question.Lesson.Module.Course.InstructorId == userId;

        if (question.AuthorId != userId && !canModerate)
        {
            throw new ForbiddenAccessException("You can only delete your own question.");
        }

        question.IsDeleted = true;
        question.MarkUpdated();
        await _context.SaveChangesAsync(cancellationToken);
    }
}

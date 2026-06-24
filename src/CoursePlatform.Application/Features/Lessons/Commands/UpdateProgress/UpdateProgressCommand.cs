using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Lessons.Commands.UpdateProgress;

public record UpdateProgressCommand(Guid CourseId, Guid LessonId) : IRequest;

public class UpdateProgressCommandHandler : IRequestHandler<UpdateProgressCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProgressCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateProgressCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == _currentUserService.UserId.Value && e.CourseId == request.CourseId, cancellationToken);

        if (enrollment == null)
        {
            throw new ForbiddenAccessException("You are not enrolled in this course.");
        }

        var lesson = await _context.Lessons
            .Include(l => l.Module)
            .FirstOrDefaultAsync(l => l.Id == request.LessonId && l.Module.CourseId == request.CourseId, cancellationToken);

        if (lesson == null)
        {
            throw new NotFoundException($"Lesson {request.LessonId} not found.");
        }

        var progress = await _context.LessonProgresses
            .FirstOrDefaultAsync(lp => lp.UserId == _currentUserService.UserId.Value && lp.LessonId == request.LessonId, cancellationToken);

        if (progress == null)
        {
            progress = new LessonProgress
            {
                UserId = _currentUserService.UserId.Value,
                LessonId = request.LessonId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow
            };
            _context.LessonProgresses.Add(progress);
        }
        else if (!progress.IsCompleted)
        {
            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}

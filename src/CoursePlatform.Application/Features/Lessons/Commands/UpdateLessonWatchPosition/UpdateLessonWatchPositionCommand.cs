using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Validation;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Lessons.Commands.UpdateLessonWatchPosition;

public record UpdateLessonWatchPositionCommand(Guid CourseId, Guid LessonId, int PositionSeconds) : IRequest;

public class UpdateLessonWatchPositionCommandValidator : AbstractValidator<UpdateLessonWatchPositionCommand>
{
    public UpdateLessonWatchPositionCommandValidator()
    {
        RuleFor(x => x.CourseId).ValidGuid("Identyfikator kursu");
        RuleFor(x => x.LessonId).ValidGuid("Identyfikator lekcji");
        RuleFor(x => x.PositionSeconds).GreaterThanOrEqualTo(0);
    }
}

public class UpdateLessonWatchPositionCommandHandler : IRequestHandler<UpdateLessonWatchPositionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateLessonWatchPositionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateLessonWatchPositionCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var hasAccess = await CourseAccessHelper.CanAccessLessonContentAsync(
            _context, _currentUserService, request.CourseId, request.LessonId, cancellationToken);

        if (!hasAccess)
        {
            throw new ForbiddenAccessException("You are not enrolled in this course.");
        }

        await ProgressGateHelper.EnsureLessonUnlockedAsync(
            _context, _currentUserService, request.CourseId, request.LessonId, cancellationToken);

        var lessonExists = await _context.Lessons
            .AnyAsync(l => l.Id == request.LessonId && l.Module.CourseId == request.CourseId, cancellationToken);

        if (!lessonExists)
        {
            throw new NotFoundException($"Lesson {request.LessonId} not found.");
        }

        var userId = _currentUserService.UserId.Value;
        var progress = await _context.LessonProgresses
            .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == request.LessonId, cancellationToken);

        if (progress == null)
        {
            progress = new LessonProgress
            {
                UserId = userId,
                LessonId = request.LessonId,
                IsCompleted = false,
                LastPositionSeconds = request.PositionSeconds,
                LastWatchedAt = DateTime.UtcNow
            };
            _context.LessonProgresses.Add(progress);
        }
        else
        {
            progress.LastPositionSeconds = request.PositionSeconds;
            progress.LastWatchedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}

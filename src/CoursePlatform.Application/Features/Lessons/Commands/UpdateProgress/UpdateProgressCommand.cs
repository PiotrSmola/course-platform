using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Lessons.Commands.UpdateProgress;

public record UpdateProgressCommand(Guid CourseId, Guid LessonId) : IRequest;

public class UpdateProgressCommandHandler : IRequestHandler<UpdateProgressCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICertificateIssuer _certificateIssuer;
    private readonly ILogger<UpdateProgressCommandHandler> _logger;

    public UpdateProgressCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICertificateIssuer certificateIssuer,
        ILogger<UpdateProgressCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _certificateIssuer = certificateIssuer;
        _logger = logger;
    }

    public async Task Handle(UpdateProgressCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var hasAccess = await CourseAccessHelper.CanAccessCourseContentAsync(
            _context, _currentUserService, request.CourseId, cancellationToken);

        if (!hasAccess)
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

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var alreadyCompleted = await _context.LessonProgresses
                .AnyAsync(lp => lp.UserId == _currentUserService.UserId.Value && lp.LessonId == request.LessonId, cancellationToken);

            if (!alreadyCompleted)
            {
                throw;
            }
        }

        try
        {
            await _certificateIssuer.IssueIfCompletedAsync(
                _currentUserService.UserId.Value, request.CourseId, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Certificate issuance check failed for user {UserId}, course {CourseId}.",
                _currentUserService.UserId.Value, request.CourseId);
        }
    }
}

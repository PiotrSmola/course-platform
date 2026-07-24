using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Lessons.Commands.DeleteLesson;

public record DeleteLessonCommand(Guid CourseId, Guid ModuleId, Guid LessonId) : IRequest;

public class DeleteLessonCommandValidator : AbstractValidator<DeleteLessonCommand>
{
    public DeleteLessonCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.ModuleId).NotEmpty();
        RuleFor(x => x.LessonId).NotEmpty();
    }
}

public class DeleteLessonCommandHandler : IRequestHandler<DeleteLessonCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAppCache _cache;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<DeleteLessonCommandHandler> _logger;

    public DeleteLessonCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage,
        ILogger<DeleteLessonCommandHandler> logger,
        IAppCache cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cache = cache;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
    {
        await CourseAccessHelper.GetManagedModuleAsync(
            _context, _currentUserService, request.CourseId, request.ModuleId, cancellationToken);

        var lesson = await _context.Lessons
            .Include(l => l.Resources)
            .FirstOrDefaultAsync(l => l.Id == request.LessonId && l.ModuleId == request.ModuleId, cancellationToken);

        if (lesson == null)
        {
            throw new NotFoundException($"Lesson {request.LessonId} not found.");
        }

        var videoObjectKey = lesson.VideoObjectKey;
        var resourceObjectKeys = lesson.Resources
            .Select(r => r.ObjectKey)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Distinct()
            .ToList();

        _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.InvalidateTagAsync("courses", cancellationToken);

        foreach (var objectKey in resourceObjectKeys)
        {
            try
            {
                await _fileStorage.DeleteObjectAsync(objectKey, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete resource object {ObjectKey} for removed lesson {LessonId}.",
                    objectKey, request.LessonId);
            }
        }

        if (!string.IsNullOrWhiteSpace(videoObjectKey))
        {
            try
            {
                await _fileStorage.DeleteObjectAsync(videoObjectKey, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete video object {ObjectKey} for removed lesson {LessonId}.",
                    videoObjectKey, request.LessonId);
            }
        }
    }
}

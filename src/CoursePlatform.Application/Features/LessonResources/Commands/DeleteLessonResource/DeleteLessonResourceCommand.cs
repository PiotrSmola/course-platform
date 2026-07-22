using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.LessonResources.Commands.DeleteLessonResource;

public record DeleteLessonResourceCommand(
    Guid CourseId,
    Guid LessonId,
    Guid ResourceId) : IRequest;

public class DeleteLessonResourceCommandHandler : IRequestHandler<DeleteLessonResourceCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<DeleteLessonResourceCommandHandler> _logger;

    public DeleteLessonResourceCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage,
        ILogger<DeleteLessonResourceCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task Handle(DeleteLessonResourceCommand request, CancellationToken cancellationToken)
    {
        await CourseAccessHelper.GetManagedCourseAsync(
            _context, _currentUserService, request.CourseId, cancellationToken);

        var resource = await _context.LessonResources
            .FirstOrDefaultAsync(
                r => r.Id == request.ResourceId
                     && r.LessonId == request.LessonId
                     && r.Lesson.Module.CourseId == request.CourseId,
                cancellationToken);

        if (resource == null)
        {
            throw new NotFoundException($"Resource {request.ResourceId} not found.");
        }

        var objectKey = resource.ObjectKey;

        _context.LessonResources.Remove(resource);
        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(objectKey))
        {
            try
            {
                await _fileStorage.DeleteObjectAsync(objectKey, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete resource object {ObjectKey} for resource {ResourceId}.",
                    objectKey, request.ResourceId);
            }
        }
    }
}

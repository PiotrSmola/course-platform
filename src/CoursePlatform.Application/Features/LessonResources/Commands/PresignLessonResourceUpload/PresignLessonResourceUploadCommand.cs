using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.LessonResources.Commands.PresignLessonResourceUpload;

public record PresignLessonResourceUploadResult(Guid ResourceId, string ObjectKey, string Url);

public record PresignLessonResourceUploadCommand(
    Guid CourseId,
    Guid LessonId,
    string ContentType) : IRequest<PresignLessonResourceUploadResult>;

public class PresignLessonResourceUploadCommandValidator : AbstractValidator<PresignLessonResourceUploadCommand>
{
    public PresignLessonResourceUploadCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.LessonId).NotEmpty();
        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(ct => UploadLimits.AllowedLessonResourceContentTypes.Contains(ct))
            .WithMessage("Unsupported resource content type. Allowed: application/pdf, application/zip, text/plain, application/json.");
    }
}

public class PresignLessonResourceUploadCommandHandler
    : IRequestHandler<PresignLessonResourceUploadCommand, PresignLessonResourceUploadResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public PresignLessonResourceUploadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<PresignLessonResourceUploadResult> Handle(
        PresignLessonResourceUploadCommand request,
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

        var resourceId = Guid.NewGuid();
        var objectKey = ObjectKeyBuilder.LessonResourceObjectKey(
            request.CourseId, request.LessonId, resourceId);

        var url = await _fileStorage.GetPresignedPutUrlAsync(
            objectKey, request.ContentType, TimeSpan.FromMinutes(10), cancellationToken);

        return new PresignLessonResourceUploadResult(resourceId, objectKey, url);
    }
}

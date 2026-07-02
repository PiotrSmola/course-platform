using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Lessons.Commands.VideoUploads;

public record InitiateLessonVideoUploadResult(
    string ObjectKey,
    string UploadId,
    int PartSizeBytes,
    int MaxParts);

public record InitiateLessonVideoUploadCommand(
    Guid CourseId,
    Guid LessonId,
    string ContentType) : IRequest<InitiateLessonVideoUploadResult>;

public class InitiateLessonVideoUploadCommandValidator : AbstractValidator<InitiateLessonVideoUploadCommand>
{
    public InitiateLessonVideoUploadCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.LessonId).NotEmpty();
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(200);
    }
}

public class InitiateLessonVideoUploadCommandHandler
    : IRequestHandler<InitiateLessonVideoUploadCommand, InitiateLessonVideoUploadResult>
{
    private const int PartSizeBytes = 15 * 1024 * 1024;
    private const int MaxParts = 10000;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public InitiateLessonVideoUploadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<InitiateLessonVideoUploadResult> Handle(
        InitiateLessonVideoUploadCommand request,
        CancellationToken cancellationToken)
    {
        await CourseAccessHelper.GetManagedCourseAsync(_context, _currentUserService, request.CourseId, cancellationToken);

        var lessonExists = await _context.Lessons
            .AsNoTracking()
            .Include(l => l.Module)
            .AnyAsync(l => l.Id == request.LessonId && l.Module.CourseId == request.CourseId, cancellationToken);

        if (!lessonExists)
        {
            throw new NotFoundException($"Lesson {request.LessonId} not found.");
        }

        var objectKey = ObjectKeyBuilder.LessonVideoObjectKey(request.CourseId, request.LessonId);
        var uploadId = await _fileStorage.CreateMultipartUploadAsync(objectKey, request.ContentType, cancellationToken);

        return new InitiateLessonVideoUploadResult(objectKey, uploadId, PartSizeBytes, MaxParts);
    }
}


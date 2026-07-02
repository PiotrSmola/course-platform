using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Models;

namespace CoursePlatform.Application.Features.Lessons.Commands.VideoUploads;

public record CompleteLessonVideoUploadCommand(
    Guid CourseId,
    Guid LessonId,
    string UploadId,
    List<CompletedPart> Parts) : IRequest;

public class CompleteLessonVideoUploadCommandValidator : AbstractValidator<CompleteLessonVideoUploadCommand>
{
    public CompleteLessonVideoUploadCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.LessonId).NotEmpty();
        RuleFor(x => x.UploadId).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Parts).NotNull().NotEmpty();
        RuleForEach(x => x.Parts).SetValidator(new CompletedPartValidator());
    }

    private sealed class CompletedPartValidator : AbstractValidator<CompletedPart>
    {
        public CompletedPartValidator()
        {
            RuleFor(p => p.PartNumber).GreaterThan(0);
            RuleFor(p => p.ETag).NotEmpty().MaximumLength(200);
        }
    }
}

public class CompleteLessonVideoUploadCommandHandler : IRequestHandler<CompleteLessonVideoUploadCommand>
{
    private const long MaxVideoBytes = 200L * 1024 * 1024;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public CompleteLessonVideoUploadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task Handle(CompleteLessonVideoUploadCommand request, CancellationToken cancellationToken)
    {
        await CourseAccessHelper.GetManagedCourseAsync(_context, _currentUserService, request.CourseId, cancellationToken);

        var lesson = await _context.Lessons
            .Include(l => l.Module)
            .FirstOrDefaultAsync(l => l.Id == request.LessonId && l.Module.CourseId == request.CourseId, cancellationToken);

        if (lesson == null)
        {
            throw new NotFoundException($"Lesson {request.LessonId} not found.");
        }

        var objectKey = ObjectKeyBuilder.LessonVideoObjectKey(request.CourseId, request.LessonId);
        await _fileStorage.CompleteMultipartUploadAsync(objectKey, request.UploadId, request.Parts, cancellationToken);

        var stat = await _fileStorage.StatObjectAsync(objectKey, cancellationToken);
        if (stat == null)
        {
            throw new NotFoundException("Uploaded object not found after completion.");
        }

        if (stat.SizeBytes > MaxVideoBytes)
        {
            await _fileStorage.AbortMultipartUploadAsync(objectKey, request.UploadId, cancellationToken);
            throw new FluentValidation.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Video", "Video exceeds max size (200MB).")
            });
        }

        lesson.VideoObjectKey = objectKey;
        lesson.MarkUpdated();
        await _context.SaveChangesAsync(cancellationToken);
    }
}


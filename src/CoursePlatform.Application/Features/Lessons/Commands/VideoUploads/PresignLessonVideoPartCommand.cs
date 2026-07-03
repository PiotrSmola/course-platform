using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Lessons.Commands.VideoUploads;

public record PresignLessonVideoPartResult(string Url);

public record PresignLessonVideoPartCommand(
    Guid CourseId,
    Guid LessonId,
    string UploadId,
    int PartNumber) : IRequest<PresignLessonVideoPartResult>;

public class PresignLessonVideoPartCommandValidator : AbstractValidator<PresignLessonVideoPartCommand>
{
    public PresignLessonVideoPartCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.LessonId).NotEmpty();
        RuleFor(x => x.UploadId).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.PartNumber).InclusiveBetween(1, UploadLimits.MaxVideoParts);
    }
}

public class PresignLessonVideoPartCommandHandler : IRequestHandler<PresignLessonVideoPartCommand, PresignLessonVideoPartResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public PresignLessonVideoPartCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<PresignLessonVideoPartResult> Handle(PresignLessonVideoPartCommand request, CancellationToken cancellationToken)
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
        var url = await _fileStorage.GetPresignedUploadPartUrlAsync(
            objectKey,
            request.UploadId,
            request.PartNumber,
            TimeSpan.FromMinutes(20),
            cancellationToken);

        return new PresignLessonVideoPartResult(url);
    }
}


using FluentValidation;
using MediatR;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Courses.Commands.ThumbnailUploads;

public record ConfirmCourseThumbnailUploadCommand(
    Guid CourseId,
    string ObjectKey) : IRequest;

public class ConfirmCourseThumbnailUploadCommandValidator : AbstractValidator<ConfirmCourseThumbnailUploadCommand>
{
    public ConfirmCourseThumbnailUploadCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.ObjectKey).NotEmpty().MaximumLength(1024);
    }
}

public class ConfirmCourseThumbnailUploadCommandHandler : IRequestHandler<ConfirmCourseThumbnailUploadCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public ConfirmCourseThumbnailUploadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task Handle(ConfirmCourseThumbnailUploadCommand request, CancellationToken cancellationToken)
    {
        var course = await CourseAccessHelper.GetManagedCourseAsync(_context, _currentUserService, request.CourseId, cancellationToken);

        if (request.ObjectKey != ObjectKeyBuilder.CourseThumbnailObjectKey(request.CourseId))
        {
            throw new ForbiddenAccessException("Invalid object key.");
        }

        var stat = await _fileStorage.StatObjectAsync(request.ObjectKey, cancellationToken);
        if (stat == null)
        {
            throw new NotFoundException("Thumbnail object not found.");
        }

        if (stat.SizeBytes > UploadLimits.MaxThumbnailBytes)
        {
            await _fileStorage.DeleteObjectAsync(request.ObjectKey, cancellationToken);
            throw new FluentValidation.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Thumbnail", "Thumbnail exceeds max size (5MB).")
            });
        }

        course.ThumbnailObjectKey = request.ObjectKey;
        course.MarkUpdated();
        await _context.SaveChangesAsync(cancellationToken);
    }
}


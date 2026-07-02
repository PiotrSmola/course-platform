using FluentValidation;
using MediatR;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Courses.Commands.ThumbnailUploads;

public record PresignCourseThumbnailUploadResult(string ObjectKey, string Url);

public record PresignCourseThumbnailUploadCommand(
    Guid CourseId,
    string ContentType) : IRequest<PresignCourseThumbnailUploadResult>;

public class PresignCourseThumbnailUploadCommandValidator : AbstractValidator<PresignCourseThumbnailUploadCommand>
{
    public PresignCourseThumbnailUploadCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(200);
    }
}

public class PresignCourseThumbnailUploadCommandHandler
    : IRequestHandler<PresignCourseThumbnailUploadCommand, PresignCourseThumbnailUploadResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public PresignCourseThumbnailUploadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<PresignCourseThumbnailUploadResult> Handle(
        PresignCourseThumbnailUploadCommand request,
        CancellationToken cancellationToken)
    {
        await CourseAccessHelper.GetManagedCourseAsync(_context, _currentUserService, request.CourseId, cancellationToken);

        var objectKey = ObjectKeyBuilder.CourseThumbnailObjectKey(request.CourseId);
        var url = await _fileStorage.GetPresignedPutUrlAsync(objectKey, request.ContentType, TimeSpan.FromMinutes(10), cancellationToken);

        return new PresignCourseThumbnailUploadResult(objectKey, url);
    }
}


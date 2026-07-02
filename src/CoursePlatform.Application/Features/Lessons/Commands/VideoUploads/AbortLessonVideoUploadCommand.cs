using FluentValidation;
using MediatR;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Lessons.Commands.VideoUploads;

public record AbortLessonVideoUploadCommand(
    Guid CourseId,
    Guid LessonId,
    string UploadId) : IRequest;

public class AbortLessonVideoUploadCommandValidator : AbstractValidator<AbortLessonVideoUploadCommand>
{
    public AbortLessonVideoUploadCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.LessonId).NotEmpty();
        RuleFor(x => x.UploadId).NotEmpty().MaximumLength(2000);
    }
}

public class AbortLessonVideoUploadCommandHandler : IRequestHandler<AbortLessonVideoUploadCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public AbortLessonVideoUploadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task Handle(AbortLessonVideoUploadCommand request, CancellationToken cancellationToken)
    {
        await CourseAccessHelper.GetManagedCourseAsync(_context, _currentUserService, request.CourseId, cancellationToken);

        var objectKey = ObjectKeyBuilder.LessonVideoObjectKey(request.CourseId, request.LessonId);
        await _fileStorage.AbortMultipartUploadAsync(objectKey, request.UploadId, cancellationToken);
    }
}


using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.LessonResources.Commands.ConfirmLessonResourceUpload;

public record ConfirmLessonResourceUploadCommand(
    Guid CourseId,
    Guid LessonId,
    Guid ResourceId,
    string ObjectKey,
    string Title,
    string ContentType,
    int? Order) : IRequest<Guid>;

public class ConfirmLessonResourceUploadCommandValidator : AbstractValidator<ConfirmLessonResourceUploadCommand>
{
    public ConfirmLessonResourceUploadCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.LessonId).NotEmpty();
        RuleFor(x => x.ResourceId).NotEmpty();
        RuleFor(x => x.ObjectKey).NotEmpty().MaximumLength(1024);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(ct => UploadLimits.AllowedLessonResourceContentTypes.Contains(ct))
            .WithMessage("Unsupported resource content type.");
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0).When(x => x.Order.HasValue);
    }
}

public class ConfirmLessonResourceUploadCommandHandler : IRequestHandler<ConfirmLessonResourceUploadCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public ConfirmLessonResourceUploadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<Guid> Handle(ConfirmLessonResourceUploadCommand request, CancellationToken cancellationToken)
    {
        await CourseAccessHelper.GetManagedCourseAsync(
            _context, _currentUserService, request.CourseId, cancellationToken);

        var lessonExists = await _context.Lessons
            .AnyAsync(l => l.Id == request.LessonId && l.Module.CourseId == request.CourseId, cancellationToken);

        if (!lessonExists)
        {
            throw new NotFoundException($"Lesson {request.LessonId} not found.");
        }

        var expectedKey = ObjectKeyBuilder.LessonResourceObjectKey(
            request.CourseId, request.LessonId, request.ResourceId);

        if (request.ObjectKey != expectedKey)
        {
            throw new ForbiddenAccessException("Invalid object key.");
        }

        if (await _context.LessonResources.AnyAsync(r => r.Id == request.ResourceId, cancellationToken))
        {
            throw new ForbiddenAccessException("Resource already exists.");
        }

        var stat = await _fileStorage.StatObjectAsync(request.ObjectKey, cancellationToken);
        if (stat == null)
        {
            throw new NotFoundException("Resource object not found.");
        }

        if (stat.SizeBytes > UploadLimits.MaxLessonResourceBytes)
        {
            await _fileStorage.DeleteObjectAsync(request.ObjectKey, cancellationToken);
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Resource", "Resource exceeds max size (25MB).")
            });
        }

        var order = request.Order ?? (
            await _context.LessonResources
                .Where(r => r.LessonId == request.LessonId)
                .MaxAsync(r => (int?)r.Order, cancellationToken) ?? -1) + 1;

        var resource = LessonResource.Create(
            request.ResourceId,
            request.LessonId,
            request.Title.Trim(),
            request.ObjectKey,
            request.ContentType,
            stat.SizeBytes,
            order);

        _context.LessonResources.Add(resource);
        await _context.SaveChangesAsync(cancellationToken);

        return resource.Id;
    }
}

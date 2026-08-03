using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.LessonResources.Queries.GetLessonResourceDownloadUrl;

public record GetLessonResourceDownloadUrlQuery(
    Guid CourseId,
    Guid LessonId,
    Guid ResourceId) : IRequest<LessonResourceDownloadDto>;

public class GetLessonResourceDownloadUrlQueryValidator : AbstractValidator<GetLessonResourceDownloadUrlQuery>
{
    public GetLessonResourceDownloadUrlQueryValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.LessonId).NotEmpty();
        RuleFor(x => x.ResourceId).NotEmpty();
    }
}

public class GetLessonResourceDownloadUrlQueryHandler
    : IRequestHandler<GetLessonResourceDownloadUrlQuery, LessonResourceDownloadDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public GetLessonResourceDownloadUrlQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<LessonResourceDownloadDto> Handle(
        GetLessonResourceDownloadUrlQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var hasAccess = await CourseAccessHelper.CanAccessLessonContentAsync(
            _context, _currentUserService, request.CourseId, request.LessonId, cancellationToken);

        if (!hasAccess)
        {
            throw new ForbiddenAccessException("You are not enrolled in this course.");
        }

        await ProgressGateHelper.EnsureLessonUnlockedAsync(
            _context, _currentUserService, request.CourseId, request.LessonId, cancellationToken);

        var resource = await _context.LessonResources
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.Id == request.ResourceId
                     && r.LessonId == request.LessonId
                     && r.Lesson.Module.CourseId == request.CourseId,
                cancellationToken);

        if (resource == null)
        {
            throw new NotFoundException($"Resource {request.ResourceId} not found.");
        }

        var url = await _fileStorage.GetPresignedDownloadUrlAsync(
            resource.ObjectKey,
            TimeSpan.FromMinutes(15),
            cancellationToken);

        return new LessonResourceDownloadDto(url);
    }
}

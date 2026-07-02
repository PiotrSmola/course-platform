using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Courses.Queries.GetCourseThumbnailUrl;

public record CourseThumbnailUrlDto(string Url);

public record GetCourseThumbnailUrlQuery(Guid CourseId) : IRequest<CourseThumbnailUrlDto>;

public class GetCourseThumbnailUrlQueryValidator : AbstractValidator<GetCourseThumbnailUrlQuery>
{
    public GetCourseThumbnailUrlQueryValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
    }
}

public class GetCourseThumbnailUrlQueryHandler : IRequestHandler<GetCourseThumbnailUrlQuery, CourseThumbnailUrlDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public GetCourseThumbnailUrlQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<CourseThumbnailUrlDto> Handle(GetCourseThumbnailUrlQuery request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

        if (course == null)
        {
            throw new NotFoundException($"Course {request.CourseId} not found.");
        }

        if (course.Status != Domain.Enums.CourseStatus.Published)
        {
            if (!_currentUserService.UserId.HasValue ||
                (_currentUserService.UserId.Value != course.InstructorId && !_currentUserService.IsAdmin))
            {
                throw new NotFoundException($"Course {request.CourseId} not found.");
            }
        }

        if (string.IsNullOrWhiteSpace(course.ThumbnailObjectKey))
        {
            throw new NotFoundException("Thumbnail not available.");
        }

        var url = await _fileStorage.GetPresignedDownloadUrlAsync(course.ThumbnailObjectKey, TimeSpan.FromMinutes(10), cancellationToken);
        return new CourseThumbnailUrlDto(url);
    }
}


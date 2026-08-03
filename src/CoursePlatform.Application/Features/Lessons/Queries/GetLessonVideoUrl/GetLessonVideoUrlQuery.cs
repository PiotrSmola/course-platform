using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Lessons.Queries.GetLessonVideoUrl;

public record LessonVideoUrlDto(string Url);

public record GetLessonVideoUrlQuery(Guid CourseId, Guid LessonId) : IRequest<LessonVideoUrlDto>;

public class GetLessonVideoUrlQueryValidator : AbstractValidator<GetLessonVideoUrlQuery>
{
    public GetLessonVideoUrlQueryValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.LessonId).NotEmpty();
    }
}

public class GetLessonVideoUrlQueryHandler : IRequestHandler<GetLessonVideoUrlQuery, LessonVideoUrlDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public GetLessonVideoUrlQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<LessonVideoUrlDto> Handle(GetLessonVideoUrlQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        await CourseContentAccessHelper.EnsureCanViewLessonAsync(
            _context, _currentUserService, request.CourseId, request.LessonId, cancellationToken);

        var lesson = await _context.Lessons
            .AsNoTracking()
            .Include(l => l.Module)
            .FirstOrDefaultAsync(l => l.Id == request.LessonId && l.Module.CourseId == request.CourseId, cancellationToken);

        if (lesson == null)
        {
            throw new NotFoundException($"Lesson {request.LessonId} not found.");
        }

        if (string.IsNullOrWhiteSpace(lesson.VideoObjectKey))
        {
            throw new NotFoundException("Video not available.");
        }

        var url = await _fileStorage.GetPresignedDownloadUrlAsync(
            lesson.VideoObjectKey,
            TimeSpan.FromHours(6),
            cancellationToken);

        return new LessonVideoUrlDto(url);
    }
}


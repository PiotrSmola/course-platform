using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.LessonResources.Queries.GetLessonResources;

public record GetLessonResourcesQuery(Guid CourseId, Guid LessonId)
    : IRequest<IReadOnlyList<LessonResourceDto>>;

public class GetLessonResourcesQueryHandler
    : IRequestHandler<GetLessonResourcesQuery, IReadOnlyList<LessonResourceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetLessonResourcesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<LessonResourceDto>> Handle(
        GetLessonResourcesQuery request,
        CancellationToken cancellationToken)
    {
        var hasAccess = await CourseAccessHelper.CanAccessCourseContentAsync(
            _context, _currentUserService, request.CourseId, cancellationToken);

        if (!hasAccess)
        {
            throw new ForbiddenAccessException("You are not enrolled in this course.");
        }

        await ProgressGateHelper.EnsureLessonUnlockedAsync(
            _context, _currentUserService, request.CourseId, request.LessonId, cancellationToken);

        var lessonExists = await _context.Lessons
            .AnyAsync(l => l.Id == request.LessonId && l.Module.CourseId == request.CourseId, cancellationToken);

        if (!lessonExists)
        {
            throw new NotFoundException($"Lesson {request.LessonId} not found.");
        }

        return await _context.LessonResources
            .AsNoTracking()
            .Where(r => r.LessonId == request.LessonId)
            .OrderBy(r => r.Order)
            .Select(r => new LessonResourceDto(
                r.Id,
                r.Title,
                r.ContentType,
                r.SizeBytes,
                r.Order))
            .ToListAsync(cancellationToken);
    }
}

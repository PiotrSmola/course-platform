using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.LessonResources.Queries.GetLessonResourcesForInstructor;

public record GetLessonResourcesForInstructorQuery(Guid CourseId, Guid LessonId)
    : IRequest<IReadOnlyList<LessonResourceDto>>;

public class GetLessonResourcesForInstructorQueryHandler
    : IRequestHandler<GetLessonResourcesForInstructorQuery, IReadOnlyList<LessonResourceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetLessonResourcesForInstructorQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<LessonResourceDto>> Handle(
        GetLessonResourcesForInstructorQuery request,
        CancellationToken cancellationToken)
    {
        await CourseAccessHelper.GetManagedCourseAsync(
            _context, _currentUserService, request.CourseId, cancellationToken);

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

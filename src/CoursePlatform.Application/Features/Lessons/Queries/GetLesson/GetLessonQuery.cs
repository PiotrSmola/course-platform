using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;

namespace CoursePlatform.Application.Features.Lessons.Queries.GetLesson;

public record LessonDto(
    Guid Id,
    string Title,
    string? Description,
    string VideoObjectKey,
    int Duration,
    int Order,
    Guid ModuleId,
    string ModuleTitle,
    Guid CourseId,
    string CourseTitle,
    bool IsCompleted);

public record GetLessonQuery(Guid CourseId, Guid LessonId) : IRequest<LessonDto>;

public class GetLessonQueryHandler : IRequestHandler<GetLessonQuery, LessonDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetLessonQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<LessonDto> Handle(GetLessonQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var hasAccess = await CourseAccessHelper.CanAccessCourseContentAsync(
            _context, _currentUserService, request.CourseId, cancellationToken);

        if (!hasAccess)
        {
            throw new ForbiddenAccessException("You are not enrolled in this course.");
        }

        var lesson = await _context.Lessons
            .AsNoTracking()
            .Include(l => l.Module)
            .ThenInclude(m => m.Course)
            .FirstOrDefaultAsync(l => l.Id == request.LessonId && l.Module.CourseId == request.CourseId, cancellationToken);

        if (lesson == null)
        {
            throw new NotFoundException($"Lesson {request.LessonId} not found.");
        }

        var progress = await _context.LessonProgresses
            .FirstOrDefaultAsync(lp => lp.UserId == _currentUserService.UserId.Value && lp.LessonId == lesson.Id, cancellationToken);

        return new LessonDto(
            lesson.Id,
            lesson.Title,
            lesson.Description,
            lesson.VideoObjectKey,
            lesson.Duration,
            lesson.Order,
            lesson.ModuleId,
            lesson.Module.Title,
            lesson.Module.CourseId,
            lesson.Module.Course.Title,
            progress?.IsCompleted ?? false);
    }
}

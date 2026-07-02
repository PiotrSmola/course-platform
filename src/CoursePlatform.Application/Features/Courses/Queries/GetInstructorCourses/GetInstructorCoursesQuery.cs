using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Courses.Queries.GetInstructorCourses;

public record GetInstructorCoursesQuery() : IRequest<List<InstructorCourseDto>>;

public record InstructorCourseDto(
    Guid Id,
    string Title,
    CourseStatus Status,
    decimal Price,
    string ThumbnailObjectKey,
    int EnrollmentCount,
    int ModuleCount,
    int LessonCount,
    DateTime CreatedAt);

public class GetInstructorCoursesQueryHandler : IRequestHandler<GetInstructorCoursesQuery, List<InstructorCourseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetInstructorCoursesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<InstructorCourseDto>> Handle(GetInstructorCoursesQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var instructorId = _currentUserService.UserId.Value;

        return await _context.Courses
            .AsNoTracking()
            .Where(c => c.InstructorId == instructorId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new InstructorCourseDto(
                c.Id,
                c.Title,
                c.Status,
                c.Price,
                c.ThumbnailObjectKey,
                c.Enrollments.Count,
                c.Modules.Count,
                c.Modules.SelectMany(m => m.Lessons).Count(),
                c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}

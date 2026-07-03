using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Helpers;
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
    string? ThumbnailUrl,
    int EnrollmentCount,
    int ModuleCount,
    int LessonCount,
    DateTime CreatedAt);

public class GetInstructorCoursesQueryHandler : IRequestHandler<GetInstructorCoursesQuery, List<InstructorCourseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public GetInstructorCoursesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<List<InstructorCourseDto>> Handle(GetInstructorCoursesQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var instructorId = _currentUserService.UserId.Value;

        var rows = await _context.Courses
            .AsNoTracking()
            .Where(c => c.InstructorId == instructorId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Status,
                c.Price,
                c.ThumbnailObjectKey,
                EnrollmentCount = c.Enrollments.Count,
                ModuleCount = c.Modules.Count,
                LessonCount = c.Modules.SelectMany(m => m.Lessons).Count(),
                c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var result = new List<InstructorCourseDto>(rows.Count);
        foreach (var row in rows)
        {
            result.Add(new InstructorCourseDto(
                row.Id,
                row.Title,
                row.Status,
                row.Price,
                await _fileStorage.GetThumbnailUrlOrNullAsync(row.ThumbnailObjectKey, cancellationToken),
                row.EnrollmentCount,
                row.ModuleCount,
                row.LessonCount,
                row.CreatedAt));
        }

        return result;
    }
}

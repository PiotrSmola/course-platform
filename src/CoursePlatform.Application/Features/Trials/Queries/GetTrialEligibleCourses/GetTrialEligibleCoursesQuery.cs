using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Application.Features.Trials.Queries.GetTrialEligibleCourses;

public record GetTrialEligibleCoursesQuery : IRequest<IReadOnlyList<TrialEligibleCourseDto>>;

public class GetTrialEligibleCoursesQueryHandler
    : IRequestHandler<GetTrialEligibleCoursesQuery, IReadOnlyList<TrialEligibleCourseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public GetTrialEligibleCoursesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<IReadOnlyList<TrialEligibleCourseDto>> Handle(
        GetTrialEligibleCoursesQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;
        if (await _context.TrialAccesses
                .AsNoTracking()
                .AnyAsync(access => access.UserId == userId, cancellationToken)
            || await CourseAccessHelper.HasActiveSubscriptionAsync(_context, userId, cancellationToken))
        {
            return Array.Empty<TrialEligibleCourseDto>();
        }

        var courses = await _context.Courses
            .AsNoTracking()
            .Include(course => course.Instructor)
            .Include(course => course.Modules)
            .ThenInclude(module => module.Lessons)
            .Where(course => course.Status == CourseStatus.Published
                && course.Price > 0
                && course.InstructorId != userId
                && !course.Enrollments.Any(enrollment => enrollment.UserId == userId))
            .ToListAsync(cancellationToken);

        var result = new List<TrialEligibleCourseDto>();
        foreach (var course in courses)
        {
            var lessonCount = course.Modules.Sum(module => module.Lessons.Count);
            if (lessonCount < 2)
            {
                continue;
            }

            result.Add(new TrialEligibleCourseDto(
                course.Id,
                course.Title,
                course.ShortDescription,
                await _fileStorage.GetThumbnailUrlOrNullAsync(course.ThumbnailObjectKey, cancellationToken),
                $"{course.Instructor.FirstName} {course.Instructor.LastName}".Trim(),
                course.Price,
                lessonCount));
        }

        return result
            .OrderBy(course => course.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Enrollments.Queries.GetMyEnrollments;

public record EnrollmentDto(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    string? CourseThumbnailUrl,
    CourseLevel CourseLevel,
    DateTime EnrolledAt,
    int CompletedLessons,
    int TotalLessons,
    double ProgressPercentage,
    Guid? FirstLessonId,
    Guid? ContinueLessonId);

public record GetMyEnrollmentsQuery : IRequest<List<EnrollmentDto>>;

public class GetMyEnrollmentsQueryHandler : IRequestHandler<GetMyEnrollmentsQuery, List<EnrollmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public GetMyEnrollmentsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<List<EnrollmentDto>> Handle(GetMyEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null) return new List<EnrollmentDto>();

        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .ThenInclude(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .Where(e => e.UserId == _currentUserService.UserId.Value)
            .ToListAsync(cancellationToken);

        var lessonIds = enrollments.SelectMany(e => e.Course.Modules.SelectMany(m => m.Lessons.Select(l => l.Id))).ToList();
        var progress = await _context.LessonProgresses
            .AsNoTracking()
            .Where(lp => lp.UserId == _currentUserService.UserId.Value && lessonIds.Contains(lp.LessonId))
            .ToListAsync(cancellationToken);

        var result = new List<EnrollmentDto>(enrollments.Count);
        foreach (var e in enrollments)
        {
            var orderedLessons = e.Course.Modules
                .OrderBy(m => m.Order)
                .SelectMany(m => m.Lessons.OrderBy(l => l.Order))
                .ToList();
            var lockStates = await ProgressGateHelper.GetLessonLockStatesAsync(
                _context,
                _currentUserService,
                e.CourseId,
                cancellationToken);
            var totalLessons = orderedLessons.Count;
            var completedIds = progress
                .Where(p => p.IsCompleted && orderedLessons.Any(l => l.Id == p.LessonId))
                .Select(p => p.LessonId)
                .ToHashSet();
            var completedLessons = completedIds.Count;
            var firstLessonId = orderedLessons.FirstOrDefault()?.Id;
            var continueLessonId = orderedLessons
                .FirstOrDefault(lesson => !completedIds.Contains(lesson.Id) && !lockStates.GetValueOrDefault(lesson.Id).IsLocked)?.Id
                ?? orderedLessons.FirstOrDefault(lesson => !lockStates.GetValueOrDefault(lesson.Id).IsLocked)?.Id
                ?? firstLessonId;
            result.Add(new EnrollmentDto(
                e.Id,
                e.CourseId,
                e.Course.Title,
                await _fileStorage.GetThumbnailUrlOrNullAsync(e.Course.ThumbnailObjectKey, cancellationToken),
                e.Course.Level,
                e.EnrolledAt,
                completedLessons,
                totalLessons,
                totalLessons > 0 ? (double)completedLessons / totalLessons * 100 : 0,
                firstLessonId,
                continueLessonId));
        }

        return result;
    }
}

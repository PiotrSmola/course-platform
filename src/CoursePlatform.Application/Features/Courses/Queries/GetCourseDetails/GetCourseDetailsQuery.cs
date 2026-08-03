using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;

namespace CoursePlatform.Application.Features.Courses.Queries.GetCourseDetails;

public record GetCourseDetailsQuery(Guid Id) : IRequest<CourseDetailsDto>;

public class GetCourseDetailsQueryHandler : IRequestHandler<GetCourseDetailsQuery, CourseDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public GetCourseDetailsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<CourseDetailsDto> Handle(GetCourseDetailsQuery request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .Include(c => c.Instructor)
            .Include(c => c.Categories)
            .Include(c => c.Technologies)
            .Include(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .Include(c => c.Reviews)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (course == null)
        {
            throw new NotFoundException($"Course {request.Id} not found.");
        }

        var canManage = _currentUserService.UserId.HasValue &&
            (_currentUserService.UserId.Value == course.InstructorId || _currentUserService.IsAdmin);

        var isWaitlistTeaser = course.Status is Domain.Enums.CourseStatus.Draft or Domain.Enums.CourseStatus.Hidden
            && !canManage;

        if (course.Status != Domain.Enums.CourseStatus.Published && !canManage && !isWaitlistTeaser)
        {
            throw new NotFoundException($"Course {request.Id} not found.");
        }

        var isEnrolled = false;
        var canAccessContent = false;
        var contentAccess = new CourseContentAccess(CourseContentAccessLevel.None);
        var hasSubscriptionAccess = false;
        var hasUserReviewed = false;
        Guid? userReviewId = null;
        var isOnWishlist = false;
        var isOnWaitlist = false;
        HashSet<Guid> completedLessonIds = new();

        if (_currentUserService.UserId.HasValue && !isWaitlistTeaser)
        {
            var userId = _currentUserService.UserId.Value;
            isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == request.Id, cancellationToken);
            hasSubscriptionAccess = await CourseAccessHelper.HasActiveSubscriptionAsync(
                _context,
                userId,
                cancellationToken);

            contentAccess = await CourseContentAccessHelper.GetAsync(
                _context, _currentUserService, request.Id, cancellationToken);
            var userReview = await _context.Reviews
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == request.Id, cancellationToken);
            hasUserReviewed = userReview != null;
            userReviewId = userReview?.Id;

            canAccessContent = contentAccess.CanViewLessons;

            if (contentAccess.HasFullAccess)
            {
                var lessonIds = course.Modules.SelectMany(m => m.Lessons).Select(l => l.Id).ToList();
                completedLessonIds = (await _context.LessonProgresses
                    .Where(lp => lp.UserId == userId && lessonIds.Contains(lp.LessonId) && lp.IsCompleted)
                    .Select(lp => lp.LessonId)
                    .ToListAsync(cancellationToken))
                    .ToHashSet();
            }

            if (course.Status == Domain.Enums.CourseStatus.Published)
            {
                isOnWishlist = await _context.WishlistItems
                    .AnyAsync(w => w.UserId == userId && w.CourseId == request.Id, cancellationToken);
            }
        }

        if (_currentUserService.UserId.HasValue
            && course.Status is Domain.Enums.CourseStatus.Draft or Domain.Enums.CourseStatus.Hidden)
        {
            isOnWaitlist = await _context.CourseWaitlistEntries
                .AnyAsync(
                    e => e.UserId == _currentUserService.UserId.Value && e.CourseId == request.Id,
                    cancellationToken);
        }

        canAccessContent = canAccessContent || canManage;

        var lockStates = contentAccess.IsTrial
            ? await CourseContentAccessHelper.GetTrialLockStatesAsync(
                _context, course.Id, cancellationToken)
            : canAccessContent
                ? await ProgressGateHelper.GetLessonLockStatesAsync(
                    _context, _currentUserService, course.Id, cancellationToken)
                : new Dictionary<Guid, (bool IsLocked, string? LockReason)>();

        var modules = course.Modules.OrderBy(m => m.Order).Select(m => new ModuleDto(
            m.Id,
            m.Title,
            m.Order,
            m.Lessons.OrderBy(l => l.Order).Select(l =>
            {
                var lockState = lockStates.GetValueOrDefault(l.Id);
                return new LessonListDto(
                    l.Id,
                    l.Title,
                    isWaitlistTeaser ? null : l.Description,
                    l.Duration,
                    l.Order,
                    completedLessonIds.Contains(l.Id),
                    canManage ? l.VideoObjectKey : null,
                    lockState.IsLocked,
                    lockState.LockReason);
            }).ToList())).ToList();

        var reviews = isWaitlistTeaser
            ? new List<ReviewDto>()
            : course.Reviews.OrderByDescending(r => r.CreatedAt).Select(r => new ReviewDto(
                r.Id,
                r.Rating,
                r.Comment,
                $"{r.User.FirstName} {r.User.LastName}",
                r.CreatedAt)).ToList();

        var canReview = (isEnrolled || hasSubscriptionAccess) && !hasUserReviewed;

        var canJoinWaitlist = _currentUserService.UserId.HasValue &&
            course.Status is Domain.Enums.CourseStatus.Draft or Domain.Enums.CourseStatus.Hidden &&
            _currentUserService.UserId.Value != course.InstructorId &&
            !isOnWaitlist;

        return new CourseDetailsDto(
            course.Id,
            course.Title,
            isWaitlistTeaser ? course.ShortDescription : course.Description,
            course.ShortDescription,
            course.Price,
            course.Level,
            course.Status,
            await _fileStorage.GetThumbnailUrlOrNullAsync(course.ThumbnailObjectKey, cancellationToken),
            course.Language,
            course.InstructorId,
            $"{course.Instructor.FirstName} {course.Instructor.LastName}",
            course.CreatedAt,
            course.Categories.Select(c => c.Name).ToList(),
            course.Technologies.Select(t => t.Name).ToList(),
            modules,
            isWaitlistTeaser ? 0 : (course.Reviews.Any() ? course.Reviews.Average(r => r.Rating) : 0),
            isWaitlistTeaser ? 0 : course.Reviews.Count,
            reviews,
            isEnrolled,
            canAccessContent,
            hasSubscriptionAccess,
            hasUserReviewed,
            canReview,
            userReviewId,
            isOnWishlist,
            canJoinWaitlist,
            isOnWaitlist);
    }
}

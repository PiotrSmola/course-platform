using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Courses.Queries.GetCourseDetails;

public record CourseDetailsDto(
    Guid Id,
    string Title,
    string Description,
    string ShortDescription,
    decimal Price,
    CourseLevel Level,
    CourseStatus Status,
    string? ThumbnailUrl,
    string Language,
    Guid InstructorId,
    string InstructorName,
    DateTime CreatedAt,
    List<string> CategoryNames,
    List<string> TechnologyNames,
    List<ModuleDto> Modules,
    double AverageRating,
    int ReviewCount,
    List<ReviewDto> Reviews,
    bool IsEnrolled,
    bool CanAccessContent,
    bool HasSubscriptionAccess,
    bool HasUserReviewed,
    bool CanReview,
    Guid? UserReviewId,
    bool IsOnWishlist,
    bool CanJoinWaitlist,
    bool IsOnWaitlist);

public record ModuleDto(Guid Id, string Title, int Order, List<LessonListDto> Lessons);
public record LessonListDto(
    Guid Id,
    string Title,
    string? Description,
    int Duration,
    int Order,
    bool IsCompleted = false,
    string? VideoObjectKey = null,
    bool IsLocked = false,
    string? LockReason = null);
public record ReviewDto(Guid Id, int Rating, string Comment, string AuthorName, DateTime CreatedAt);

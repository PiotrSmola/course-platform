using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Coupons;

public record CouponListItemDto(
    Guid Id,
    string Code,
    DiscountType DiscountType,
    decimal Value,
    DateTime StartsAt,
    DateTime? ExpiresAt,
    int? MaxRedemptions,
    int RedeemedCount,
    bool IsActive,
    IReadOnlyList<Guid> CourseIds);

public record CreateCouponRequest(
    string Code,
    DiscountType DiscountType,
    decimal Value,
    DateTime StartsAt,
    DateTime? ExpiresAt,
    int? MaxRedemptions,
    bool IsActive,
    IReadOnlyList<Guid>? CourseIds);

public record UpdateCouponRequest(
    DiscountType DiscountType,
    decimal Value,
    DateTime StartsAt,
    DateTime? ExpiresAt,
    int? MaxRedemptions,
    bool IsActive,
    IReadOnlyList<Guid>? CourseIds);

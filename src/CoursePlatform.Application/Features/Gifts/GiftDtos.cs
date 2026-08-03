using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Gifts;

public record GiftPurchaseListItemDto(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    string RecipientEmail,
    GiftStatus Status,
    decimal Amount,
    string Currency,
    DateTime? CompletedAt,
    DateTime? RedeemedAt,
    string? Code);

public record GiftRedemptionDto(Guid CourseId, string CourseTitle);

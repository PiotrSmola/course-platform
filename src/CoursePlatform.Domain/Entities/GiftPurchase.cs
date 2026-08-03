using CoursePlatform.Domain.Common;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Domain.Entities;

public class GiftPurchase : BaseEntity
{
    public Guid BuyerUserId { get; set; }
    public ApplicationUser BuyerUser { get; set; } = null!;

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public string RecipientEmail { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public string ProtectedCode { get; set; } = string.Empty;
    public GiftStatus Status { get; set; } = GiftStatus.Pending;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? StripeSessionId { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Guid? RedeemedByUserId { get; set; }
    public ApplicationUser? RedeemedByUser { get; set; }
    public DateTime? RedeemedAt { get; set; }
}

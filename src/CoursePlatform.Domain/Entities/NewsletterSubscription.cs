using CoursePlatform.Domain.Common;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Domain.Entities;

public class NewsletterSubscription : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public NewsletterSubscriptionStatus Status { get; set; } = NewsletterSubscriptionStatus.Pending;
    public string? ConfirmationTokenHash { get; set; }
    public string UnsubscribeTokenHash { get; set; } = string.Empty;
    public DateTime? ConfirmedAt { get; set; }
}

using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class SubscriptionInvoice : BaseEntity
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
    public string StripeInvoiceId { get; set; } = string.Empty;

    public Guid SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;
}

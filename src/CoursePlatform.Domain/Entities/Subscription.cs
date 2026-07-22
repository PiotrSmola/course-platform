using CoursePlatform.Domain.Common;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string StripeCustomerId { get; set; } = string.Empty;
    public string StripeSubscriptionId { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Incomplete;
    public DateTime CurrentPeriodEnd { get; set; }

    public ICollection<SubscriptionInvoice> Invoices { get; set; } = new List<SubscriptionInvoice>();
}

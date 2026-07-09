using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class ProcessedStripeEvent : BaseEntity
{
    public string StripeEventId { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

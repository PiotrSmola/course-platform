using CoursePlatform.Domain.Common;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Domain.Entities;

public class BusinessPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string? Currency { get; set; } = "PLN";
    public BillingPeriod? BillingPeriod { get; set; }
    public string? PriceLabel { get; set; }
    public string CallToActionText { get; set; } = string.Empty;
    public string CallToActionUrl { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;

    public ICollection<BusinessPlanFeature> Features { get; set; } = new List<BusinessPlanFeature>();
}
using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class BusinessPlanFeature : BaseEntity
{
    public Guid BusinessPlanId { get; set; }
    public BusinessPlan BusinessPlan { get; set; } = null!;

    public string Text { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
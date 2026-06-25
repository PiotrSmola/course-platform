using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.BusinessPlans.Queries.GetBusinessPlans;

public record BusinessPlanFeatureDto(Guid Id, string Text, int DisplayOrder);

public record BusinessPlanDto(
    Guid Id,
    string Name,
    string Slug,
    string ShortDescription,
    decimal? Price,
    string? Currency,
    BillingPeriod? BillingPeriod,
    string? PriceLabel,
    string CallToActionText,
    string CallToActionUrl,
    bool IsFeatured,
    int DisplayOrder,
    IReadOnlyList<BusinessPlanFeatureDto> Features);

public record BusinessPlansVm(IReadOnlyList<BusinessPlanDto> Items);
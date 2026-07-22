namespace CoursePlatform.Application.Common.Options;

public sealed class SubscriptionOptions
{
    public const string SectionName = "Subscription";

    public decimal MonthlyPricePln { get; init; } = 399m;
}

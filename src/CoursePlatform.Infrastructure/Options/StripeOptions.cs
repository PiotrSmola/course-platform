namespace CoursePlatform.Infrastructure.Options;

public sealed class StripeOptions
{
    public string SecretKey { get; init; } = string.Empty;
    public string WebhookSecret { get; init; } = string.Empty;
    public string Currency { get; init; } = "pln";
    public string SuccessUrl { get; init; } = "http://localhost:5173/payment/success";
    public string CancelUrl { get; init; } = "http://localhost:5173/payment/cancel";
}

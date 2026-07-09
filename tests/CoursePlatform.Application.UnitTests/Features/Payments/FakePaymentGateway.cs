using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.UnitTests.Features.Payments;

public sealed class FakePaymentGateway : IPaymentGateway
{
    public bool IsConfigured { get; set; } = true;
    public string DefaultCurrency { get; set; } = "pln";
    public PaymentGatewayEvent? EventToReturn { get; set; }
    public string SessionIdToReturn { get; set; } = "cs_test_123";
    public Action? OnCreateCheckoutSession { get; set; }

    public Task<CheckoutSession> CreateCheckoutSessionAsync(
        Guid paymentId,
        Guid courseId,
        string courseTitle,
        decimal amount,
        string customerEmail,
        CancellationToken cancellationToken)
    {
        OnCreateCheckoutSession?.Invoke();
        return Task.FromResult(new CheckoutSession(
            SessionIdToReturn,
            $"https://checkout.stripe.test/{SessionIdToReturn}",
            DefaultCurrency));
    }

    public PaymentGatewayEvent ParseWebhookEvent(string payload, string signature)
    {
        return EventToReturn ?? new PaymentGatewayEvent(PaymentGatewayEventType.Ignored, string.Empty, null, null, null, null);
    }
}

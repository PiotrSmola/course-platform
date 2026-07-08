using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.UnitTests.Features.Payments;

public sealed class FakePaymentGateway : IPaymentGateway
{
    public bool IsConfigured { get; set; } = true;
    public PaymentGatewayEvent? EventToReturn { get; set; }
    public string SessionIdToReturn { get; set; } = "cs_test_123";

    public Task<CheckoutSession> CreateCheckoutSessionAsync(
        Guid paymentId,
        Guid courseId,
        string courseTitle,
        decimal amount,
        string customerEmail,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new CheckoutSession(
            SessionIdToReturn,
            $"https://checkout.stripe.test/{SessionIdToReturn}",
            "pln"));
    }

    public PaymentGatewayEvent ParseWebhookEvent(string payload, string signature)
    {
        return EventToReturn ?? new PaymentGatewayEvent(PaymentGatewayEventType.Ignored, null, null);
    }
}

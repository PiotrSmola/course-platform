using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.UnitTests.Features.Payments;

public sealed class FakePaymentGateway : IPaymentGateway
{
    public bool IsConfigured { get; set; } = true;
    public string DefaultCurrency { get; set; } = "pln";
    public PaymentGatewayEvent? EventToReturn { get; set; }
    public string SessionIdToReturn { get; set; } = "cs_test_123";
    public string BillingPortalUrlToReturn { get; set; } = "https://billing.stripe.test/session";
    public Action? OnCreateCheckoutSession { get; set; }
    public Action? OnCreateSubscriptionCheckoutSession { get; set; }
    public Action? OnCreateBillingPortalSession { get; set; }
    public SubscriptionGatewayState? SubscriptionStateToReturn { get; set; }

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

    public Task<CheckoutSession> CreateSubscriptionCheckoutSessionAsync(
        Guid userId,
        string customerEmail,
        decimal amountPln,
        string? existingStripeCustomerId,
        CancellationToken cancellationToken)
    {
        OnCreateSubscriptionCheckoutSession?.Invoke();
        return Task.FromResult(new CheckoutSession(
            SessionIdToReturn,
            $"https://checkout.stripe.test/{SessionIdToReturn}",
            DefaultCurrency));
    }

    public Task<BillingPortalSession> CreateBillingPortalSessionAsync(
        string stripeCustomerId,
        string returnUrl,
        CancellationToken cancellationToken)
    {
        OnCreateBillingPortalSession?.Invoke();
        return Task.FromResult(new BillingPortalSession(BillingPortalUrlToReturn));
    }

    public Task<SubscriptionGatewayState?> GetSubscriptionStateAsync(
        string stripeSubscriptionId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(SubscriptionStateToReturn);
    }

    public Task<PaymentGatewayEvent> ParseWebhookEventAsync(string payload, string signature, CancellationToken cancellationToken)
    {
        return Task.FromResult(EventToReturn ?? new PaymentGatewayEvent(PaymentGatewayEventType.Ignored, string.Empty, null, null, null, null));
    }
}

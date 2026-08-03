namespace CoursePlatform.Application.Common.Interfaces;

public record CheckoutSession(string SessionId, string RedirectUrl, string Currency);
public record BillingPortalSession(string RedirectUrl);

public enum PaymentGatewayEventType
{
    Ignored = 0,
    CheckoutCompleted = 1,
    CheckoutExpired = 2,
    PaymentRefunded = 3,
    Chargeback = 4,
    SubscriptionCheckoutCompleted = 5,
    SubscriptionUpdated = 6,
    SubscriptionDeleted = 7,
    InvoicePaid = 8,
    GiftCheckoutCompleted = 9,
    GiftCheckoutExpired = 10,
    GiftRefunded = 11,
    GiftChargeback = 12
}

public record PaymentGatewayEvent(
    PaymentGatewayEventType Type,
    string EventId,
    string? SessionId,
    long? AmountTotalMinorUnits,
    string? Currency,
    string? PaymentId,
    string? UserId = null,
    string? CustomerId = null,
    string? CustomerEmail = null,
    string? SubscriptionId = null,
    string? InvoiceId = null,
    CoursePlatform.Domain.Enums.SubscriptionStatus? SubscriptionStatus = null,
    DateTime? CurrentPeriodEnd = null,
    DateTime? PaidAt = null,
    string? GiftId = null);

public record SubscriptionGatewayState(
    string SubscriptionId,
    string CustomerId,
    CoursePlatform.Domain.Enums.SubscriptionStatus Status,
    DateTime CurrentPeriodEnd);

public interface IPaymentGateway
{
    bool IsConfigured { get; }
    string DefaultCurrency { get; }

    Task<CheckoutSession> CreateCheckoutSessionAsync(
        Guid paymentId,
        Guid courseId,
        string courseTitle,
        decimal amount,
        string customerEmail,
        CancellationToken cancellationToken);

    Task<CheckoutSession> CreateGiftCheckoutSessionAsync(
        Guid giftId,
        Guid courseId,
        string courseTitle,
        decimal amount,
        string customerEmail,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException("Gift checkout is not supported by this payment gateway.");

    Task<CheckoutSession> CreateSubscriptionCheckoutSessionAsync(
        Guid userId,
        string customerEmail,
        decimal amountPln,
        string? existingStripeCustomerId,
        CancellationToken cancellationToken);

    Task<BillingPortalSession> CreateBillingPortalSessionAsync(
        string stripeCustomerId,
        string returnUrl,
        CancellationToken cancellationToken);

    Task<SubscriptionGatewayState?> GetSubscriptionStateAsync(
        string stripeSubscriptionId,
        CancellationToken cancellationToken);

    Task<PaymentGatewayEvent> ParseWebhookEventAsync(string payload, string signature, CancellationToken cancellationToken);
}

namespace CoursePlatform.Application.Common.Interfaces;

public record CheckoutSession(string SessionId, string RedirectUrl, string Currency);

public enum PaymentGatewayEventType
{
    Ignored = 0,
    CheckoutCompleted = 1,
    CheckoutExpired = 2,
    PaymentRefunded = 3,
    Chargeback = 4
}

public record PaymentGatewayEvent(
    PaymentGatewayEventType Type,
    string EventId,
    string? SessionId,
    long? AmountTotalMinorUnits,
    string? Currency,
    string? PaymentId);

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

    PaymentGatewayEvent ParseWebhookEvent(string payload, string signature);
}

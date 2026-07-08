namespace CoursePlatform.Application.Common.Interfaces;

public record CheckoutSession(string SessionId, string RedirectUrl, string Currency);

public enum PaymentGatewayEventType
{
    Ignored = 0,
    CheckoutCompleted = 1,
    CheckoutExpired = 2
}

public record PaymentGatewayEvent(
    PaymentGatewayEventType Type,
    string? SessionId,
    long? AmountTotalMinorUnits);

public interface IPaymentGateway
{
    bool IsConfigured { get; }

    Task<CheckoutSession> CreateCheckoutSessionAsync(
        Guid paymentId,
        Guid courseId,
        string courseTitle,
        decimal amount,
        string customerEmail,
        CancellationToken cancellationToken);

    PaymentGatewayEvent ParseWebhookEvent(string payload, string signature);
}

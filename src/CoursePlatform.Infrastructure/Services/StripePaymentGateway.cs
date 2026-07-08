using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace CoursePlatform.Infrastructure.Services;

internal sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly StripeOptions _options;
    private readonly StripeClient? _client;

    public StripePaymentGateway(IOptions<StripeOptions> options)
    {
        _options = options.Value;
        _client = string.IsNullOrWhiteSpace(_options.SecretKey)
            ? null
            : new StripeClient(_options.SecretKey);
    }

    public bool IsConfigured =>
        _client != null && !string.IsNullOrWhiteSpace(_options.WebhookSecret);

    public async Task<CheckoutSession> CreateCheckoutSessionAsync(
        Guid paymentId,
        Guid courseId,
        string courseTitle,
        decimal amount,
        string customerEmail,
        CancellationToken cancellationToken)
    {
        if (_client == null)
        {
            throw new InvalidOperationException("Stripe is not configured.");
        }

        var sessionOptions = new SessionCreateOptions
        {
            Mode = "payment",
            ClientReferenceId = paymentId.ToString(),
            CustomerEmail = string.IsNullOrWhiteSpace(customerEmail) ? null : customerEmail,
            SuccessUrl = $"{_options.SuccessUrl}?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{_options.CancelUrl}?courseId={courseId}",
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = _options.Currency,
                        UnitAmount = (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = courseTitle
                        }
                    }
                }
            },
            Metadata = new Dictionary<string, string>
            {
                ["paymentId"] = paymentId.ToString(),
                ["courseId"] = courseId.ToString()
            }
        };

        var service = new SessionService(_client);
        var session = await service.CreateAsync(sessionOptions, cancellationToken: cancellationToken);

        return new CheckoutSession(session.Id, session.Url, _options.Currency);
    }

    public PaymentGatewayEvent ParseWebhookEvent(string payload, string signature)
    {
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                payload,
                signature,
                _options.WebhookSecret,
                throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex)
        {
            throw new FluentValidation.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Signature", $"Invalid webhook signature: {ex.Message}")
            });
        }

        return stripeEvent.Type switch
        {
            "checkout.session.completed" => ToEvent(PaymentGatewayEventType.CheckoutCompleted, stripeEvent),
            "checkout.session.expired" => ToEvent(PaymentGatewayEventType.CheckoutExpired, stripeEvent),
            _ => new PaymentGatewayEvent(PaymentGatewayEventType.Ignored, null, null)
        };
    }

    private static PaymentGatewayEvent ToEvent(PaymentGatewayEventType type, Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session;
        return new PaymentGatewayEvent(type, session?.Id, session?.AmountTotal);
    }
}

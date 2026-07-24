using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Enums;
using CoursePlatform.Infrastructure.Options;
using CoursePlatform.Infrastructure.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;
using Stripe;
using Stripe.Checkout;

namespace CoursePlatform.Infrastructure.Services;

internal sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly StripeOptions _options;
    private readonly StripeClient? _client;
    private readonly ResiliencePipeline _pipeline;

    public StripePaymentGateway(
        IOptions<StripeOptions> options,
        ResiliencePipelineProvider<string> pipelineProvider)
    {
        _options = options.Value;
        _client = string.IsNullOrWhiteSpace(_options.SecretKey)
            ? null
            : new StripeClient(_options.SecretKey);
        _pipeline = pipelineProvider.GetPipeline(ResiliencePipelineNames.Outbound);
    }

    public bool IsConfigured =>
        _client != null && !string.IsNullOrWhiteSpace(_options.WebhookSecret);

    public string DefaultCurrency => _options.Currency;

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
            },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["paymentId"] = paymentId.ToString(),
                    ["courseId"] = courseId.ToString()
                }
            }
        };

        var service = new SessionService(_client);
        var session = await _pipeline.ExecuteAsync(async ct =>
            await service.CreateAsync(sessionOptions, cancellationToken: ct), cancellationToken);

        return new CheckoutSession(session.Id, session.Url, _options.Currency);
    }

    public async Task<CheckoutSession> CreateSubscriptionCheckoutSessionAsync(
        Guid userId,
        string customerEmail,
        decimal amountPln,
        string? existingStripeCustomerId,
        CancellationToken cancellationToken)
    {
        if (_client == null)
        {
            throw new InvalidOperationException("Stripe is not configured.");
        }

        var sessionOptions = new SessionCreateOptions
        {
            Mode = "subscription",
            ClientReferenceId = userId.ToString(),
            Customer = string.IsNullOrWhiteSpace(existingStripeCustomerId) ? null : existingStripeCustomerId,
            CustomerEmail = string.IsNullOrWhiteSpace(existingStripeCustomerId) && !string.IsNullOrWhiteSpace(customerEmail)
                ? customerEmail
                : null,
            SuccessUrl = $"{_options.SuccessUrl}?type=subscription&session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{_options.CancelUrl}?type=subscription",
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = _options.Currency,
                        UnitAmount = (long)Math.Round(amountPln * 100, MidpointRounding.AwayFromZero),
                        Recurring = new SessionLineItemPriceDataRecurringOptions
                        {
                            Interval = "month"
                        },
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Course Platform All-access"
                        }
                    }
                }
            },
            Metadata = new Dictionary<string, string>
            {
                ["userId"] = userId.ToString()
            },
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["userId"] = userId.ToString()
                }
            }
        };

        var service = new SessionService(_client);
        var session = await _pipeline.ExecuteAsync(async ct =>
            await service.CreateAsync(sessionOptions, cancellationToken: ct), cancellationToken);

        return new CheckoutSession(session.Id, session.Url, _options.Currency);
    }

    public async Task<BillingPortalSession> CreateBillingPortalSessionAsync(
        string stripeCustomerId,
        string returnUrl,
        CancellationToken cancellationToken)
    {
        if (_client == null)
        {
            throw new InvalidOperationException("Stripe is not configured.");
        }

        var service = new Stripe.BillingPortal.SessionService(_client);
        var session = await _pipeline.ExecuteAsync(async ct =>
            await service.CreateAsync(
                new Stripe.BillingPortal.SessionCreateOptions
                {
                    Customer = stripeCustomerId,
                    ReturnUrl = returnUrl
                },
                cancellationToken: ct),
            cancellationToken);

        return new BillingPortalSession(session.Url);
    }

    public async Task<SubscriptionGatewayState?> GetSubscriptionStateAsync(
        string stripeSubscriptionId,
        CancellationToken cancellationToken)
    {
        if (_client == null || string.IsNullOrWhiteSpace(stripeSubscriptionId))
        {
            return null;
        }

        var service = new SubscriptionService(_client);
        var subscription = await _pipeline.ExecuteAsync(async ct =>
            await service.GetAsync(stripeSubscriptionId, cancellationToken: ct), cancellationToken);

        var status = MapSubscriptionStatus(subscription.Status);
        var periodEnd = GetCurrentPeriodEnd(subscription);

        if (status == null || periodEnd == null || string.IsNullOrWhiteSpace(subscription.CustomerId))
        {
            return null;
        }

        return new SubscriptionGatewayState(
            subscription.Id,
            subscription.CustomerId,
            status.Value,
            periodEnd.Value);
    }

    public PaymentGatewayEvent ParseWebhookEvent(string payload, string signature)
    {
        if (!IsConfigured)
        {
            throw new InvalidWebhookSignatureException("Stripe webhook is not configured.");
        }

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
            throw new InvalidWebhookSignatureException($"Invalid webhook signature: {ex.Message}");
        }

        return stripeEvent.Type switch
        {
            "checkout.session.completed" => ToSessionEvent(PaymentGatewayEventType.CheckoutCompleted, stripeEvent),
            "checkout.session.expired" => ToSessionEvent(PaymentGatewayEventType.CheckoutExpired, stripeEvent),
            "charge.refunded" => ToChargeEvent(PaymentGatewayEventType.PaymentRefunded, stripeEvent),
            "charge.dispute.created" => ToChargeEvent(PaymentGatewayEventType.Chargeback, stripeEvent),
            "customer.subscription.updated" => ToSubscriptionEvent(PaymentGatewayEventType.SubscriptionUpdated, stripeEvent),
            "customer.subscription.deleted" => ToSubscriptionEvent(PaymentGatewayEventType.SubscriptionDeleted, stripeEvent),
            "invoice.paid" => ToInvoiceEvent(stripeEvent),
            _ => new PaymentGatewayEvent(PaymentGatewayEventType.Ignored, stripeEvent.Id, null, null, null, null)
        };
    }

    private static PaymentGatewayEvent ToSessionEvent(PaymentGatewayEventType type, Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session;
        if (string.Equals(session?.Mode, "subscription", StringComparison.OrdinalIgnoreCase))
        {
            var userId = GetMetadataValue(session?.Metadata, "userId") ?? session?.ClientReferenceId;
            return new PaymentGatewayEvent(
                PaymentGatewayEventType.SubscriptionCheckoutCompleted,
                stripeEvent.Id,
                session?.Id,
                session?.AmountTotal,
                session?.Currency,
                null,
                userId,
                session?.CustomerId,
                session?.CustomerDetails?.Email,
                session?.SubscriptionId,
                null,
                SubscriptionStatus.Active,
                null);
        }

        var paymentId = GetMetadataValue(session?.Metadata, "paymentId") ?? session?.ClientReferenceId;
        return new PaymentGatewayEvent(type, stripeEvent.Id, session?.Id, session?.AmountTotal, session?.Currency, paymentId);
    }

    private static PaymentGatewayEvent ToChargeEvent(PaymentGatewayEventType type, Event stripeEvent)
    {
        var charge = stripeEvent.Data.Object as Charge;
        var paymentId = GetMetadataValue(charge?.Metadata, "paymentId");
        return new PaymentGatewayEvent(
            type,
            stripeEvent.Id,
            null,
            charge?.AmountRefunded,
            charge?.Currency,
            paymentId);
    }

    private static PaymentGatewayEvent ToSubscriptionEvent(PaymentGatewayEventType type, Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        return new PaymentGatewayEvent(
            type,
            stripeEvent.Id,
            null,
            null,
            subscription?.Currency,
            null,
            GetMetadataValue(subscription?.Metadata, "userId"),
            subscription?.CustomerId,
            null,
            subscription?.Id,
            null,
            MapSubscriptionStatus(subscription?.Status),
            GetCurrentPeriodEnd(subscription));
    }

    private static PaymentGatewayEvent ToInvoiceEvent(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        return new PaymentGatewayEvent(
            PaymentGatewayEventType.InvoicePaid,
            stripeEvent.Id,
            null,
            invoice?.AmountPaid,
            invoice?.Currency,
            null,
            GetMetadataValue(invoice?.Parent?.SubscriptionDetails?.Metadata, "userId"),
            invoice?.CustomerId,
            invoice?.CustomerEmail,
            invoice?.Parent?.Type == "subscription_details" ? invoice.Parent.SubscriptionDetails?.SubscriptionId : null,
            invoice?.Id,
            null,
            null,
            invoice?.StatusTransitions?.PaidAt ?? invoice?.Created);
    }

    private static DateTime? GetCurrentPeriodEnd(Stripe.Subscription? subscription)
    {
        if (subscription?.Items?.Data == null || subscription.Items.Data.Count == 0)
        {
            return null;
        }

        return subscription.Items.Data.Max(item => item.CurrentPeriodEnd);
    }

    private static SubscriptionStatus? MapSubscriptionStatus(string? stripeStatus)
    {
        return stripeStatus?.ToLowerInvariant() switch
        {
            "active" => SubscriptionStatus.Active,
            "trialing" => SubscriptionStatus.Active,
            "past_due" => SubscriptionStatus.PastDue,
            "unpaid" => SubscriptionStatus.PastDue,
            "canceled" => SubscriptionStatus.Canceled,
            "incomplete" => SubscriptionStatus.Incomplete,
            "incomplete_expired" => SubscriptionStatus.Incomplete,
            _ => null
        };
    }

    private static string? GetMetadataValue(IDictionary<string, string>? metadata, string key) =>
        metadata != null && metadata.TryGetValue(key, out var value) ? value : null;
}

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Options;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Payments.Commands.ProcessPaymentWebhook;

public record ProcessPaymentWebhookCommand(string Payload, string Signature) : IRequest;

public class ProcessPaymentWebhookCommandValidator : AbstractValidator<ProcessPaymentWebhookCommand>
{
    public ProcessPaymentWebhookCommandValidator()
    {
        RuleFor(x => x.Payload).NotEmpty();
        RuleFor(x => x.Signature).NotEmpty().MaximumLength(512);
    }
}

public class ProcessPaymentWebhookCommandHandler : IRequestHandler<ProcessPaymentWebhookCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentGateway _paymentGateway;
    private readonly INotificationService _notifications;
    private readonly IEmailQueue _emailQueue;
    private readonly ILogger<ProcessPaymentWebhookCommandHandler> _logger;
    private readonly IGiftCodeProtector? _giftCodeProtector;
    private readonly FrontendOptions? _frontendOptions;

    public ProcessPaymentWebhookCommandHandler(
        IApplicationDbContext context,
        IPaymentGateway paymentGateway,
        INotificationService notifications,
        IEmailQueue emailQueue,
        ILogger<ProcessPaymentWebhookCommandHandler> logger,
        IGiftCodeProtector? giftCodeProtector = null,
        IOptions<FrontendOptions>? frontendOptions = null)
    {
        _context = context;
        _paymentGateway = paymentGateway;
        _notifications = notifications;
        _emailQueue = emailQueue;
        _logger = logger;
        _giftCodeProtector = giftCodeProtector;
        _frontendOptions = frontendOptions?.Value;
    }

    public async Task Handle(ProcessPaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        var gatewayEvent = await _paymentGateway.ParseWebhookEventAsync(request.Payload, request.Signature, cancellationToken);

        if (gatewayEvent.Type == PaymentGatewayEventType.Ignored || string.IsNullOrWhiteSpace(gatewayEvent.EventId))
        {
            return;
        }

        var alreadyProcessed = await _context.ProcessedStripeEvents
            .AnyAsync(e => e.StripeEventId == gatewayEvent.EventId, cancellationToken);

        if (alreadyProcessed)
        {
            return;
        }

        _context.ProcessedStripeEvents.Add(new ProcessedStripeEvent
        {
            StripeEventId = gatewayEvent.EventId,
            ProcessedAt = DateTime.UtcNow
        });

        switch (gatewayEvent.Type)
        {
            case PaymentGatewayEventType.CheckoutCompleted:
                await HandleCompletedAsync(gatewayEvent, cancellationToken);
                break;
            case PaymentGatewayEventType.CheckoutExpired:
                await HandleExpiredAsync(gatewayEvent, cancellationToken);
                break;
            case PaymentGatewayEventType.PaymentRefunded:
                await HandleRefundedAsync(gatewayEvent, PaymentStatus.Refunded, cancellationToken);
                break;
            case PaymentGatewayEventType.Chargeback:
                await HandleRefundedAsync(gatewayEvent, PaymentStatus.Chargeback, cancellationToken);
                break;
            case PaymentGatewayEventType.GiftCheckoutCompleted:
                await HandleGiftCompletedAsync(gatewayEvent, cancellationToken);
                break;
            case PaymentGatewayEventType.GiftCheckoutExpired:
                await HandleGiftExpiredAsync(gatewayEvent, cancellationToken);
                break;
            case PaymentGatewayEventType.GiftRefunded:
                await HandleGiftReversedAsync(gatewayEvent, GiftStatus.Refunded, cancellationToken);
                break;
            case PaymentGatewayEventType.GiftChargeback:
                await HandleGiftReversedAsync(gatewayEvent, GiftStatus.Chargeback, cancellationToken);
                break;
            case PaymentGatewayEventType.SubscriptionCheckoutCompleted:
                await HandleSubscriptionCheckoutCompletedAsync(gatewayEvent, cancellationToken);
                break;
            case PaymentGatewayEventType.SubscriptionUpdated:
            case PaymentGatewayEventType.SubscriptionDeleted:
                await HandleSubscriptionUpdatedAsync(gatewayEvent, cancellationToken);
                break;
            case PaymentGatewayEventType.InvoicePaid:
                await HandleSubscriptionInvoicePaidAsync(gatewayEvent, cancellationToken);
                break;
        }
    }

    private async Task HandleCompletedAsync(PaymentGatewayEvent gatewayEvent, CancellationToken cancellationToken)
    {
        var payment = await FindPaymentAsync(gatewayEvent, cancellationToken);
        if (payment == null) return;

        if (payment.Status == PaymentStatus.Completed) return;

        if (!string.Equals(gatewayEvent.Currency, payment.Currency, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(
                "Payment {PaymentId} currency mismatch: expected {Expected}, webhook reported {Actual}. Marking as failed.",
                payment.Id, payment.Currency, gatewayEvent.Currency);

            payment.Status = PaymentStatus.Failed;
            payment.MarkUpdated();
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        var expectedMinorUnits = ToMinorUnits(payment.Amount);
        if (gatewayEvent.AmountTotalMinorUnits != expectedMinorUnits)
        {
            _logger.LogError(
                "Payment {PaymentId} amount mismatch: expected {Expected} minor units, webhook reported {Actual}. Marking as failed.",
                payment.Id, expectedMinorUnits, gatewayEvent.AmountTotalMinorUnits);

            payment.Status = PaymentStatus.Failed;
            payment.MarkUpdated();
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        payment.Status = PaymentStatus.Completed;
        payment.CompletedAt = DateTime.UtcNow;
        payment.MarkUpdated();

        if (payment.CouponId.HasValue)
        {
            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Id == payment.CouponId.Value, cancellationToken);

            if (coupon != null)
            {
                coupon.RedeemedCount += 1;
                coupon.MarkUpdated();
            }
        }

        var enrollment = new Enrollment
        {
            UserId = payment.UserId,
            CourseId = payment.CourseId,
            EnrolledAt = DateTime.UtcNow
        };
        _context.Enrollments.Add(enrollment);

        DbUpdateException? enrollmentConflict = null;

        await using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                enrollmentConflict = ex;
            }
        }

        if (enrollmentConflict != null)
        {
            var enrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == payment.UserId && e.CourseId == payment.CourseId, cancellationToken);

            if (!enrolled)
            {
                throw new InvalidOperationException(
                    $"Failed to persist enrollment for payment {payment.Id}.", enrollmentConflict);
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Payment {PaymentId} completed, user {UserId} enrolled in course {CourseId}.",
            payment.Id, payment.UserId, payment.CourseId);

        try
        {
            var courseTitle = await _context.Courses
                .Where(c => c.Id == payment.CourseId)
                .Select(c => c.Title)
                .FirstOrDefaultAsync(cancellationToken) ?? "kurs";

            var buyer = await _context.Users
                .Where(u => u.Id == payment.UserId)
                .Select(u => new { u.Email, u.FirstName })
                .FirstOrDefaultAsync(cancellationToken);

            if (buyer?.Email != null)
            {
                var (subject, html) = EmailTemplates.PurchaseConfirmed(
                    buyer.FirstName, courseTitle, payment.Amount, payment.Currency);
                _emailQueue.Enqueue(new EmailMessage(buyer.Email, subject, html));
            }

            await _notifications.NotifyUserAsync(
                payment.UserId,
                new UserNotification(
                    "purchase-completed",
                    "Zakup zakończony",
                    $"Masz już dostęp do kursu „{courseTitle}”.",
                    $"/courses/{payment.CourseId}"),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send purchase notification for payment {PaymentId}.", payment.Id);
        }
    }

    private async Task HandleExpiredAsync(PaymentGatewayEvent gatewayEvent, CancellationToken cancellationToken)
    {
        var payment = await FindPaymentAsync(gatewayEvent, cancellationToken);
        if (payment == null) return;

        if (payment.Status != PaymentStatus.Pending) return;

        payment.Status = PaymentStatus.Expired;
        payment.MarkUpdated();
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleRefundedAsync(PaymentGatewayEvent gatewayEvent, PaymentStatus status, CancellationToken cancellationToken)
    {
        var payment = await FindPaymentAsync(gatewayEvent, cancellationToken);
        if (payment == null) return;

        if (status == PaymentStatus.Refunded && gatewayEvent.AmountTotalMinorUnits != ToMinorUnits(payment.Amount))
        {
            _logger.LogInformation(
                "Payment {PaymentId} partially refunded ({Refunded} of {Expected} minor units), keeping enrollment.",
                payment.Id, gatewayEvent.AmountTotalMinorUnits, ToMinorUnits(payment.Amount));

            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        payment.Status = status;
        payment.MarkUpdated();

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == payment.UserId && e.CourseId == payment.CourseId, cancellationToken);

        if (enrollment != null)
        {
            _context.Enrollments.Remove(enrollment);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment {PaymentId} marked as {Status}, enrollment for user {UserId} in course {CourseId} removed.",
            payment.Id, status, payment.UserId, payment.CourseId);
    }

    private async Task HandleGiftCompletedAsync(
        PaymentGatewayEvent gatewayEvent,
        CancellationToken cancellationToken)
    {
        var gift = await FindGiftAsync(gatewayEvent, cancellationToken);
        if (gift == null)
        {
            return;
        }

        if (gift.Status is GiftStatus.Active or GiftStatus.Redeemed)
        {
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        if (gift.Status != GiftStatus.Pending)
        {
            _logger.LogWarning(
                "Gift {GiftId} cannot be completed from status {Status}.",
                gift.Id,
                gift.Status);
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        if (!string.Equals(gatewayEvent.Currency, gift.Currency, StringComparison.OrdinalIgnoreCase)
            || gatewayEvent.AmountTotalMinorUnits != ToMinorUnits(gift.Amount))
        {
            _logger.LogError(
                "Gift {GiftId} payment mismatch. Expected {ExpectedAmount} {ExpectedCurrency}, received {ActualAmount} {ActualCurrency}.",
                gift.Id,
                ToMinorUnits(gift.Amount),
                gift.Currency,
                gatewayEvent.AmountTotalMinorUnits,
                gatewayEvent.Currency);

            gift.Status = GiftStatus.Expired;
            gift.MarkUpdated();
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        gift.Status = GiftStatus.Active;
        gift.CompletedAt = DateTime.UtcNow;
        gift.MarkUpdated();
        await _context.SaveChangesAsync(cancellationToken);

        await SendGiftEmailAsync(gift, cancellationToken);
    }

    private async Task HandleGiftExpiredAsync(
        PaymentGatewayEvent gatewayEvent,
        CancellationToken cancellationToken)
    {
        var gift = await FindGiftAsync(gatewayEvent, cancellationToken);
        if (gift == null)
        {
            return;
        }

        if (gift.Status == GiftStatus.Pending)
        {
            gift.Status = GiftStatus.Expired;
            gift.MarkUpdated();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleGiftReversedAsync(
        PaymentGatewayEvent gatewayEvent,
        GiftStatus status,
        CancellationToken cancellationToken)
    {
        var gift = await FindGiftAsync(gatewayEvent, cancellationToken);
        if (gift == null)
        {
            return;
        }

        if (status == GiftStatus.Refunded
            && gatewayEvent.AmountTotalMinorUnits != ToMinorUnits(gift.Amount))
        {
            _logger.LogInformation(
                "Gift {GiftId} was partially refunded ({Refunded} of {Expected} minor units); keeping it active.",
                gift.Id,
                gatewayEvent.AmountTotalMinorUnits,
                ToMinorUnits(gift.Amount));

            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        if (gift.Status == status)
        {
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        gift.Status = status;
        gift.MarkUpdated();

        if (gift.RedeemedByUserId.HasValue)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(
                    item => item.UserId == gift.RedeemedByUserId.Value && item.CourseId == gift.CourseId,
                    cancellationToken);

            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Gift {GiftId} marked as {Status}.",
            gift.Id,
            status);
    }

    private async Task SendGiftEmailAsync(GiftPurchase gift, CancellationToken cancellationToken)
    {
        if (_giftCodeProtector == null || _frontendOptions == null)
        {
            _logger.LogWarning("Gift {GiftId} was activated but email dependencies are unavailable.", gift.Id);
            return;
        }

        try
        {
            var courseTitle = await _context.Courses
                .Where(course => course.Id == gift.CourseId)
                .Select(course => course.Title)
                .FirstOrDefaultAsync(cancellationToken) ?? "kurs";
            var code = _giftCodeProtector.Unprotect(gift.ProtectedCode);
            var redeemUrl = $"{_frontendOptions.BaseUrl.TrimEnd('/')}/gifts/redeem";
            var (subject, html) = GiftEmailTemplates.GiftReceived(courseTitle, code, redeemUrl);

            _emailQueue.Enqueue(new EmailMessage(gift.RecipientEmail, subject, html));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to queue gift email for gift {GiftId}.", gift.Id);
        }
    }

    private static long ToMinorUnits(decimal amount) =>
        (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);

    private async Task HandleSubscriptionCheckoutCompletedAsync(
        PaymentGatewayEvent gatewayEvent,
        CancellationToken cancellationToken)
    {
        gatewayEvent = await EnrichSubscriptionCheckoutEventAsync(gatewayEvent, cancellationToken);

        var subscription = await GetOrCreateSubscriptionAsync(gatewayEvent, cancellationToken);
        if (subscription == null)
        {
            return;
        }

        ApplySubscriptionChanges(subscription, gatewayEvent);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<PaymentGatewayEvent> EnrichSubscriptionCheckoutEventAsync(
        PaymentGatewayEvent gatewayEvent,
        CancellationToken cancellationToken)
    {
        var status = gatewayEvent.SubscriptionStatus ?? SubscriptionStatus.Active;
        var periodEnd = gatewayEvent.CurrentPeriodEnd;
        var customerId = gatewayEvent.CustomerId;
        var subscriptionId = gatewayEvent.SubscriptionId;

        if ((!periodEnd.HasValue || string.IsNullOrWhiteSpace(customerId))
            && !string.IsNullOrWhiteSpace(subscriptionId))
        {
            var state = await _paymentGateway.GetSubscriptionStateAsync(subscriptionId, cancellationToken);
            if (state != null)
            {
                status = state.Status;
                periodEnd = state.CurrentPeriodEnd;
                customerId ??= state.CustomerId;
                subscriptionId = state.SubscriptionId;
            }
        }

        periodEnd ??= DateTime.UtcNow.AddMonths(1);

        return gatewayEvent with
        {
            CustomerId = customerId,
            SubscriptionId = subscriptionId,
            SubscriptionStatus = status,
            CurrentPeriodEnd = periodEnd
        };
    }

    private async Task HandleSubscriptionUpdatedAsync(
        PaymentGatewayEvent gatewayEvent,
        CancellationToken cancellationToken)
    {
        var subscription = await GetOrCreateSubscriptionAsync(gatewayEvent, cancellationToken);
        if (subscription == null)
        {
            return;
        }

        ApplySubscriptionChanges(subscription, gatewayEvent);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleSubscriptionInvoicePaidAsync(
        PaymentGatewayEvent gatewayEvent,
        CancellationToken cancellationToken)
    {
        var subscription = await GetOrCreateSubscriptionAsync(gatewayEvent, cancellationToken);
        if (subscription == null)
        {
            return;
        }

        ApplySubscriptionChanges(subscription, gatewayEvent);

        if (string.IsNullOrWhiteSpace(gatewayEvent.InvoiceId)
            || gatewayEvent.AmountTotalMinorUnits == null
            || string.IsNullOrWhiteSpace(gatewayEvent.Currency))
        {
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        var invoiceExists = await _context.SubscriptionInvoices
            .AnyAsync(i => i.StripeInvoiceId == gatewayEvent.InvoiceId, cancellationToken);

        if (!invoiceExists)
        {
            _context.SubscriptionInvoices.Add(new SubscriptionInvoice
            {
                SubscriptionId = subscription.Id,
                Amount = gatewayEvent.AmountTotalMinorUnits.Value / 100m,
                Currency = gatewayEvent.Currency.ToUpperInvariant(),
                PaidAt = gatewayEvent.PaidAt ?? DateTime.UtcNow,
                StripeInvoiceId = gatewayEvent.InvoiceId
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Payment?> FindPaymentAsync(PaymentGatewayEvent gatewayEvent, CancellationToken cancellationToken)
    {
        Payment? payment = null;

        if (!string.IsNullOrWhiteSpace(gatewayEvent.SessionId))
        {
            payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripeSessionId == gatewayEvent.SessionId, cancellationToken);
        }

        if (payment == null && !string.IsNullOrWhiteSpace(gatewayEvent.PaymentId) && Guid.TryParse(gatewayEvent.PaymentId, out var paymentGuid))
        {
            payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentGuid, cancellationToken);
        }

        if (payment == null)
        {
            _logger.LogWarning(
                "Payment webhook for unknown reference session {SessionId}, payment {PaymentId}.",
                gatewayEvent.SessionId, gatewayEvent.PaymentId);
        }

        return payment;
    }

    private async Task<GiftPurchase?> FindGiftAsync(
        PaymentGatewayEvent gatewayEvent,
        CancellationToken cancellationToken)
    {
        GiftPurchase? gift = null;

        if (!string.IsNullOrWhiteSpace(gatewayEvent.SessionId))
        {
            gift = await _context.GiftPurchases
                .FirstOrDefaultAsync(item => item.StripeSessionId == gatewayEvent.SessionId, cancellationToken);
        }

        if (gift == null
            && !string.IsNullOrWhiteSpace(gatewayEvent.GiftId)
            && Guid.TryParse(gatewayEvent.GiftId, out var giftId))
        {
            gift = await _context.GiftPurchases
                .FirstOrDefaultAsync(item => item.Id == giftId, cancellationToken);
        }

        if (gift == null)
        {
            _logger.LogWarning(
                "Gift webhook for unknown reference session {SessionId}, gift {GiftId}.",
                gatewayEvent.SessionId,
                gatewayEvent.GiftId);
        }

        return gift;
    }

    private async Task<Subscription?> GetOrCreateSubscriptionAsync(
        PaymentGatewayEvent gatewayEvent,
        CancellationToken cancellationToken)
    {
        var subscription = await FindSubscriptionAsync(gatewayEvent, cancellationToken);
        if (subscription != null)
        {
            return subscription;
        }

        if (string.IsNullOrWhiteSpace(gatewayEvent.CustomerId) || string.IsNullOrWhiteSpace(gatewayEvent.SubscriptionId))
        {
            _logger.LogWarning(
                "Subscription webhook missing Stripe identifiers. Customer {CustomerId}, subscription {SubscriptionId}.",
                gatewayEvent.CustomerId,
                gatewayEvent.SubscriptionId);
            return null;
        }

        var userId = await ResolveSubscriptionUserIdAsync(gatewayEvent, cancellationToken);
        if (userId == null)
        {
            _logger.LogWarning(
                "Subscription webhook for unknown user. Customer {CustomerId}, subscription {SubscriptionId}.",
                gatewayEvent.CustomerId,
                gatewayEvent.SubscriptionId);
            return null;
        }

        subscription = new Subscription
        {
            UserId = userId.Value,
            StripeCustomerId = gatewayEvent.CustomerId,
            StripeSubscriptionId = gatewayEvent.SubscriptionId,
            Status = gatewayEvent.SubscriptionStatus ?? SubscriptionStatus.Incomplete,
            CurrentPeriodEnd = gatewayEvent.CurrentPeriodEnd ?? DateTime.UtcNow
        };

        _context.Subscriptions.Add(subscription);
        return subscription;
    }

    private async Task<Subscription?> FindSubscriptionAsync(
        PaymentGatewayEvent gatewayEvent,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(gatewayEvent.SubscriptionId))
        {
            var byStripeSubscriptionId = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == gatewayEvent.SubscriptionId, cancellationToken);

            if (byStripeSubscriptionId != null)
            {
                return byStripeSubscriptionId;
            }
        }

        if (!string.IsNullOrWhiteSpace(gatewayEvent.CustomerId))
        {
            var byStripeCustomerId = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.StripeCustomerId == gatewayEvent.CustomerId, cancellationToken);

            if (byStripeCustomerId != null)
            {
                return byStripeCustomerId;
            }
        }

        if (!string.IsNullOrWhiteSpace(gatewayEvent.UserId) && Guid.TryParse(gatewayEvent.UserId, out var userId))
        {
            return await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        }

        return null;
    }

    private async Task<Guid?> ResolveSubscriptionUserIdAsync(
        PaymentGatewayEvent gatewayEvent,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(gatewayEvent.UserId) && Guid.TryParse(gatewayEvent.UserId, out var userId))
        {
            return userId;
        }

        if (!string.IsNullOrWhiteSpace(gatewayEvent.CustomerEmail))
        {
            return await _context.Users
                .Where(u => u.Email == gatewayEvent.CustomerEmail)
                .Select(u => (Guid?)u.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return null;
    }

    private static void ApplySubscriptionChanges(Subscription subscription, PaymentGatewayEvent gatewayEvent)
    {
        var changed = false;

        if (!string.IsNullOrWhiteSpace(gatewayEvent.CustomerId) && subscription.StripeCustomerId != gatewayEvent.CustomerId)
        {
            subscription.StripeCustomerId = gatewayEvent.CustomerId;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(gatewayEvent.SubscriptionId) && subscription.StripeSubscriptionId != gatewayEvent.SubscriptionId)
        {
            subscription.StripeSubscriptionId = gatewayEvent.SubscriptionId;
            changed = true;
        }

        if (gatewayEvent.SubscriptionStatus.HasValue && subscription.Status != gatewayEvent.SubscriptionStatus.Value)
        {
            subscription.Status = gatewayEvent.SubscriptionStatus.Value;
            changed = true;
        }

        if (gatewayEvent.CurrentPeriodEnd.HasValue && subscription.CurrentPeriodEnd != gatewayEvent.CurrentPeriodEnd.Value)
        {
            subscription.CurrentPeriodEnd = gatewayEvent.CurrentPeriodEnd.Value;
            changed = true;
        }

        if (changed)
        {
            subscription.MarkUpdated();
        }
    }
}

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
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

    public ProcessPaymentWebhookCommandHandler(
        IApplicationDbContext context,
        IPaymentGateway paymentGateway,
        INotificationService notifications,
        IEmailQueue emailQueue,
        ILogger<ProcessPaymentWebhookCommandHandler> logger)
    {
        _context = context;
        _paymentGateway = paymentGateway;
        _notifications = notifications;
        _emailQueue = emailQueue;
        _logger = logger;
    }

    public async Task Handle(ProcessPaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        var gatewayEvent = _paymentGateway.ParseWebhookEvent(request.Payload, request.Signature);

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

    private static long ToMinorUnits(decimal amount) =>
        (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);

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
}

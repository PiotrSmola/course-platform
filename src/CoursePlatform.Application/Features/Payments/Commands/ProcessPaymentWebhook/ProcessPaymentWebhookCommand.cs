using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ProcessPaymentWebhookCommandHandler> _logger;

    public ProcessPaymentWebhookCommandHandler(
        IApplicationDbContext context,
        IPaymentGateway paymentGateway,
        ILogger<ProcessPaymentWebhookCommandHandler> logger)
    {
        _context = context;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public async Task Handle(ProcessPaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        var gatewayEvent = _paymentGateway.ParseWebhookEvent(request.Payload, request.Signature);

        if (await TryMarkEventProcessedAsync(gatewayEvent.EventId, cancellationToken))
        {
            return;
        }

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

    private async Task<bool> TryMarkEventProcessedAsync(string eventId, CancellationToken cancellationToken)
    {
        var alreadyProcessed = await _context.ProcessedStripeEvents
            .AnyAsync(e => e.StripeEventId == eventId, cancellationToken);

        if (alreadyProcessed)
        {
            return true;
        }

        _context.ProcessedStripeEvents.Add(new ProcessedStripeEvent
        {
            StripeEventId = eventId,
            ProcessedAt = DateTime.UtcNow
        });

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }
        catch (DbUpdateException)
        {
            return true;
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

        var expectedMinorUnits = (long)Math.Round(payment.Amount * 100, MidpointRounding.AwayFromZero);
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

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            _context.Enrollments.Add(new Enrollment
            {
                UserId = payment.UserId,
                CourseId = payment.CourseId,
                EnrolledAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);

            var enrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == payment.UserId && e.CourseId == payment.CourseId, cancellationToken);

            if (!enrolled)
            {
                throw;
            }
        }

        _logger.LogInformation("Payment {PaymentId} completed, user {UserId} enrolled in course {CourseId}.",
            payment.Id, payment.UserId, payment.CourseId);
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

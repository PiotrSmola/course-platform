using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Payments.Commands.ProcessPaymentWebhook;

public record ProcessPaymentWebhookCommand(string Payload, string Signature) : IRequest;

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

        switch (gatewayEvent.Type)
        {
            case PaymentGatewayEventType.CheckoutCompleted:
                await HandleCompletedAsync(gatewayEvent, cancellationToken);
                break;
            case PaymentGatewayEventType.CheckoutExpired:
                await HandleExpiredAsync(gatewayEvent, cancellationToken);
                break;
        }
    }

    private async Task HandleCompletedAsync(PaymentGatewayEvent gatewayEvent, CancellationToken cancellationToken)
    {
        var payment = await FindPaymentAsync(gatewayEvent.SessionId, cancellationToken);
        if (payment == null) return;

        if (payment.Status == PaymentStatus.Completed) return;

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

        var alreadyEnrolled = await _context.Enrollments
            .AnyAsync(e => e.UserId == payment.UserId && e.CourseId == payment.CourseId, cancellationToken);

        if (!alreadyEnrolled)
        {
            _context.Enrollments.Add(new Enrollment
            {
                UserId = payment.UserId,
                CourseId = payment.CourseId,
                EnrolledAt = DateTime.UtcNow
            });
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
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
        var payment = await FindPaymentAsync(gatewayEvent.SessionId, cancellationToken);
        if (payment == null) return;

        if (payment.Status != PaymentStatus.Pending) return;

        payment.Status = PaymentStatus.Expired;
        payment.MarkUpdated();
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Payment?> FindPaymentAsync(string? sessionId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            _logger.LogWarning("Payment webhook event without session id.");
            return null;
        }

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.StripeSessionId == sessionId, cancellationToken);

        if (payment == null)
        {
            _logger.LogWarning("Payment webhook for unknown session {SessionId}.", sessionId);
        }

        return payment;
    }
}

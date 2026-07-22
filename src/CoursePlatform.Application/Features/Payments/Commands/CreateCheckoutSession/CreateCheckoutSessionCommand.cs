using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Coupons;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Payments.Commands.CreateCheckoutSession;

public record CheckoutSessionDto(string? RedirectUrl, bool Enrolled);

public record CreateCheckoutSessionCommand(Guid CourseId, string? CouponCode = null) : IRequest<CheckoutSessionDto>;

public class CreateCheckoutSessionCommandValidator : AbstractValidator<CreateCheckoutSessionCommand>
{
    public CreateCheckoutSessionCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.CouponCode)
            .MaximumLength(64)
            .When(x => !string.IsNullOrWhiteSpace(x.CouponCode));
    }
}

public class CreateCheckoutSessionCommandHandler : IRequestHandler<CreateCheckoutSessionCommand, CheckoutSessionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<CreateCheckoutSessionCommandHandler> _logger;

    public CreateCheckoutSessionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPaymentGateway paymentGateway,
        ILogger<CreateCheckoutSessionCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public async Task<CheckoutSessionDto> Handle(CreateCheckoutSessionCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

        if (course == null)
        {
            throw new NotFoundException($"Course {request.CourseId} not found.");
        }

        if (course.Status != CourseStatus.Published)
        {
            throw new ForbiddenAccessException("Course is not published.");
        }

        var alreadyEnrolled = await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == request.CourseId, cancellationToken);

        if (alreadyEnrolled)
        {
            throw new ValidationException(new[] { new ValidationFailure("CourseId", "You are already enrolled in this course.") });
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new ForbiddenAccessException("User not found.");
        }

        var pricing = await CouponPricing.ResolveAsync(
            _context,
            course.Id,
            course.Price,
            request.CouponCode,
            cancellationToken);

        if (pricing.FinalAmount <= 0)
        {
            return await EnrollForFreeAsync(userId, course, pricing, cancellationToken);
        }

        if (!_paymentGateway.IsConfigured)
        {
            throw new ValidationException(new[] { new ValidationFailure("CourseId", "Payments are temporarily unavailable.") });
        }

        var payment = new Payment
        {
            UserId = userId,
            CourseId = course.Id,
            Amount = pricing.FinalAmount,
            OriginalAmount = pricing.OriginalAmount,
            DiscountAmount = pricing.DiscountAmount,
            CouponId = pricing.Coupon?.Id,
            Currency = string.Empty,
            Status = PaymentStatus.Pending,
            StripeSessionId = null
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        CheckoutSession session;
        try
        {
            session = await _paymentGateway.CreateCheckoutSessionAsync(
                payment.Id,
                course.Id,
                course.Title,
                pricing.FinalAmount,
                user.Email ?? string.Empty,
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to create Stripe checkout session for payment {PaymentId}.", payment.Id);

            payment.Status = PaymentStatus.Failed;
            payment.MarkUpdated();
            await _context.SaveChangesAsync(cancellationToken);

            throw new ValidationException(new[] { new ValidationFailure("CourseId", "Could not start the payment. Please try again.") });
        }

        payment.StripeSessionId = session.SessionId;
        payment.Currency = session.Currency;
        payment.MarkUpdated();
        await _context.SaveChangesAsync(cancellationToken);

        return new CheckoutSessionDto(session.RedirectUrl, false);
    }

    private async Task<CheckoutSessionDto> EnrollForFreeAsync(
        Guid userId,
        Course course,
        CouponPricingResult pricing,
        CancellationToken cancellationToken)
    {
        var payment = new Payment
        {
            UserId = userId,
            CourseId = course.Id,
            Amount = 0,
            OriginalAmount = pricing.OriginalAmount,
            DiscountAmount = pricing.DiscountAmount,
            CouponId = pricing.Coupon?.Id,
            Currency = _paymentGateway.DefaultCurrency,
            Status = PaymentStatus.Completed,
            CompletedAt = DateTime.UtcNow,
            StripeSessionId = null
        };

        _context.Payments.Add(payment);

        if (pricing.Coupon != null)
        {
            var coupon = await _context.Coupons
                .FirstAsync(c => c.Id == pricing.Coupon.Id, cancellationToken);
            coupon.RedeemedCount += 1;
            coupon.MarkUpdated();
        }

        _context.Enrollments.Add(new Enrollment
        {
            UserId = userId,
            CourseId = course.Id,
            EnrolledAt = DateTime.UtcNow
        });

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == course.Id, cancellationToken);

            if (!alreadyEnrolled)
            {
                throw;
            }
        }

        return new CheckoutSessionDto(null, true);
    }
}

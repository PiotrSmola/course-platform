using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Options;

namespace CoursePlatform.Application.Features.Subscriptions.Commands.CreateSubscriptionCheckout;

public record SubscriptionCheckoutSessionDto(string RedirectUrl);

public record CreateSubscriptionCheckoutCommand : IRequest<SubscriptionCheckoutSessionDto>;

public class CreateSubscriptionCheckoutCommandHandler : IRequestHandler<CreateSubscriptionCheckoutCommand, SubscriptionCheckoutSessionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly SubscriptionOptions _subscriptionOptions;
    private readonly ILogger<CreateSubscriptionCheckoutCommandHandler> _logger;

    public CreateSubscriptionCheckoutCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPaymentGateway paymentGateway,
        IOptions<SubscriptionOptions> subscriptionOptions,
        ILogger<CreateSubscriptionCheckoutCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _paymentGateway = paymentGateway;
        _subscriptionOptions = subscriptionOptions.Value;
        _logger = logger;
    }

    public async Task<SubscriptionCheckoutSessionDto> Handle(
        CreateSubscriptionCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        if (!_paymentGateway.IsConfigured)
        {
            throw new ValidationException(new[] { new ValidationFailure(string.Empty, "Payments are temporarily unavailable.") });
        }

        var userId = _currentUserService.UserId.Value;
        if (await CourseAccessHelper.HasActiveSubscriptionAsync(_context, userId, cancellationToken))
        {
            throw new ValidationException(new[] { new ValidationFailure(string.Empty, "You already have an active subscription.") });
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new ForbiddenAccessException("User not found.");
        }

        var existingStripeCustomerId = await _context.Subscriptions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => s.StripeCustomerId)
            .FirstOrDefaultAsync(cancellationToken);

        try
        {
            var session = await _paymentGateway.CreateSubscriptionCheckoutSessionAsync(
                userId,
                user.Email ?? string.Empty,
                _subscriptionOptions.MonthlyPricePln,
                string.IsNullOrWhiteSpace(existingStripeCustomerId) ? null : existingStripeCustomerId,
                cancellationToken);

            return new SubscriptionCheckoutSessionDto(session.RedirectUrl);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to create subscription checkout session for user {UserId}.", userId);
            throw new ValidationException(new[] { new ValidationFailure(string.Empty, "Could not start the subscription checkout. Please try again.") });
        }
    }
}

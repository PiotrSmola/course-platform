using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Options;

namespace CoursePlatform.Application.Features.Subscriptions.Commands.CreateBillingPortalSession;

public record BillingPortalSessionDto(string RedirectUrl);

public record CreateBillingPortalSessionCommand : IRequest<BillingPortalSessionDto>;

public class CreateBillingPortalSessionCommandHandler : IRequestHandler<CreateBillingPortalSessionCommand, BillingPortalSessionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly FrontendOptions _frontendOptions;

    public CreateBillingPortalSessionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPaymentGateway paymentGateway,
        IOptions<FrontendOptions> frontendOptions)
    {
        _context = context;
        _currentUserService = currentUserService;
        _paymentGateway = paymentGateway;
        _frontendOptions = frontendOptions.Value;
    }

    public async Task<BillingPortalSessionDto> Handle(
        CreateBillingPortalSessionCommand request,
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

        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == _currentUserService.UserId.Value, cancellationToken);

        if (subscription == null || string.IsNullOrWhiteSpace(subscription.StripeCustomerId))
        {
            throw new ValidationException(new[] { new ValidationFailure(string.Empty, "You do not have a billing profile yet.") });
        }

        var portalSession = await _paymentGateway.CreateBillingPortalSessionAsync(
            subscription.StripeCustomerId,
            $"{_frontendOptions.BaseUrl.TrimEnd('/')}/courses",
            cancellationToken);

        return new BillingPortalSessionDto(portalSession.RedirectUrl);
    }
}

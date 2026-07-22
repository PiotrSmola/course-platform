using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Subscriptions.Queries.GetMySubscription;

public record MySubscriptionDto(
    bool HasSubscription,
    bool HasActiveAccess,
    SubscriptionStatus? Status,
    DateTime? CurrentPeriodEnd,
    bool CanManageInPortal,
    DateTime? LatestInvoicePaidAt,
    decimal? LatestInvoiceAmount,
    string? LatestInvoiceCurrency);

public record GetMySubscriptionQuery : IRequest<MySubscriptionDto>;

public class GetMySubscriptionQueryHandler : IRequestHandler<GetMySubscriptionQuery, MySubscriptionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMySubscriptionQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<MySubscriptionDto> Handle(GetMySubscriptionQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Invoices)
            .FirstOrDefaultAsync(s => s.UserId == _currentUserService.UserId.Value, cancellationToken);

        if (subscription == null)
        {
            return new MySubscriptionDto(false, false, null, null, false, null, null, null);
        }

        var latestInvoice = subscription.Invoices
            .OrderByDescending(i => i.PaidAt)
            .FirstOrDefault();

        return new MySubscriptionDto(
            true,
            HasActiveAccess(subscription),
            subscription.Status,
            subscription.CurrentPeriodEnd,
            !string.IsNullOrWhiteSpace(subscription.StripeCustomerId),
            latestInvoice?.PaidAt,
            latestInvoice?.Amount,
            latestInvoice?.Currency);
    }

    private static bool HasActiveAccess(Subscription subscription)
    {
        var now = DateTime.UtcNow;
        return subscription.CurrentPeriodEnd > now
            && (subscription.Status == SubscriptionStatus.Active || subscription.Status == SubscriptionStatus.PastDue);
    }
}

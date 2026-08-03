using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Application.Features.Payments.Queries.GetPaymentHistory;

public record PurchaseHistoryItemDto(
    string Kind,
    Guid Id,
    Guid? CourseId,
    string? CourseTitle,
    string? RecipientEmail,
    decimal Amount,
    string Currency,
    DateTime CompletedAt,
    string? GiftCode);

public record GetPaymentHistoryQuery(int Limit = 100) : IRequest<IReadOnlyList<PurchaseHistoryItemDto>>;

public class GetPaymentHistoryQueryValidator : AbstractValidator<GetPaymentHistoryQuery>
{
    public GetPaymentHistoryQueryValidator()
    {
        RuleFor(query => query.Limit).InclusiveBetween(1, 100);
    }
}

public class GetPaymentHistoryQueryHandler
    : IRequestHandler<GetPaymentHistoryQuery, IReadOnlyList<PurchaseHistoryItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGiftCodeProtector? _giftCodeProtector;

    public GetPaymentHistoryQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IGiftCodeProtector? giftCodeProtector = null)
    {
        _context = context;
        _currentUserService = currentUserService;
        _giftCodeProtector = giftCodeProtector;
    }

    public async Task<IReadOnlyList<PurchaseHistoryItemDto>> Handle(
        GetPaymentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;
        var limit = Math.Clamp(request.Limit, 1, 100);

        var coursePurchases = await _context.Payments
            .AsNoTracking()
            .Where(payment => payment.UserId == userId
                && payment.Status == PaymentStatus.Completed
                && payment.CompletedAt != null)
            .Select(payment => new PurchaseHistoryItemDto(
                "course",
                payment.Id,
                payment.CourseId,
                payment.Course.Title,
                null,
                payment.Amount,
                payment.Currency,
                payment.CompletedAt!.Value,
                null))
            .ToListAsync(cancellationToken);

        var subscriptionInvoices = await _context.SubscriptionInvoices
            .AsNoTracking()
            .Where(invoice => invoice.Subscription.UserId == userId)
            .Select(invoice => new PurchaseHistoryItemDto(
                "subscription",
                invoice.Id,
                null,
                "All-access",
                null,
                invoice.Amount,
                invoice.Currency,
                invoice.PaidAt,
                null))
            .ToListAsync(cancellationToken);

        var gifts = await _context.GiftPurchases
            .AsNoTracking()
            .Where(gift => gift.BuyerUserId == userId
                && gift.CompletedAt != null
                && (gift.Status == GiftStatus.Active || gift.Status == GiftStatus.Redeemed))
            .Select(gift => new
            {
                gift.Id,
                gift.CourseId,
                CourseTitle = gift.Course.Title,
                gift.RecipientEmail,
                gift.Amount,
                gift.Currency,
                CompletedAt = gift.CompletedAt!.Value,
                gift.ProtectedCode
            })
            .ToListAsync(cancellationToken);

        var giftPurchases = gifts.Select(gift => new PurchaseHistoryItemDto(
            "gift",
            gift.Id,
            gift.CourseId,
            gift.CourseTitle,
            gift.RecipientEmail,
            gift.Amount,
            gift.Currency,
            gift.CompletedAt,
            UnprotectGiftCode(gift.ProtectedCode)));

        return coursePurchases
            .Concat(subscriptionInvoices)
            .Concat(giftPurchases)
            .OrderByDescending(item => item.CompletedAt)
            .Take(limit)
            .ToList();
    }

    private string? UnprotectGiftCode(string protectedCode)
    {
        if (_giftCodeProtector == null)
        {
            return null;
        }

        try
        {
            return _giftCodeProtector.Unprotect(protectedCode);
        }
        catch
        {
            return null;
        }
    }
}

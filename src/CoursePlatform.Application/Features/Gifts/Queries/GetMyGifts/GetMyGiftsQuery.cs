using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Application.Features.Gifts.Queries.GetMyGifts;

public record GetMyGiftsQuery : IRequest<IReadOnlyList<GiftPurchaseListItemDto>>;

public class GetMyGiftsQueryHandler : IRequestHandler<GetMyGiftsQuery, IReadOnlyList<GiftPurchaseListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGiftCodeProtector _codeProtector;

    public GetMyGiftsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IGiftCodeProtector codeProtector)
    {
        _context = context;
        _currentUserService = currentUserService;
        _codeProtector = codeProtector;
    }

    public async Task<IReadOnlyList<GiftPurchaseListItemDto>> Handle(
        GetMyGiftsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var gifts = await _context.GiftPurchases
            .AsNoTracking()
            .Include(gift => gift.Course)
            .Where(gift => gift.BuyerUserId == _currentUserService.UserId.Value)
            .OrderByDescending(gift => gift.CreatedAt)
            .ToListAsync(cancellationToken);

        return gifts.Select(gift => new GiftPurchaseListItemDto(
            gift.Id,
            gift.CourseId,
            gift.Course.Title,
            gift.RecipientEmail,
            gift.Status,
            gift.Amount,
            gift.Currency,
            gift.CompletedAt,
            gift.RedeemedAt,
            TryUnprotect(_codeProtector, gift.ProtectedCode)))
            .ToList();
    }

    private static string? TryUnprotect(IGiftCodeProtector protector, string protectedCode)
    {
        try
        {
            return protector.Unprotect(protectedCode);
        }
        catch
        {
            return null;
        }
    }
}

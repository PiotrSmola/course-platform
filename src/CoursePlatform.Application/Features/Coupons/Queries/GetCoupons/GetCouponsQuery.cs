using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Coupons.Queries.GetCoupons;

public record GetCouponsQuery : IRequest<IReadOnlyList<CouponListItemDto>>;

public class GetCouponsQueryHandler : IRequestHandler<GetCouponsQuery, IReadOnlyList<CouponListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCouponsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CouponListItemDto>> Handle(
        GetCouponsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Coupons
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CouponListItemDto(
                c.Id,
                c.Code,
                c.DiscountType,
                c.Value,
                c.StartsAt,
                c.ExpiresAt,
                c.MaxRedemptions,
                c.RedeemedCount,
                c.IsActive,
                c.CouponCourses.Select(cc => cc.CourseId).ToList()
            ))
            .ToListAsync(cancellationToken);
    }
}

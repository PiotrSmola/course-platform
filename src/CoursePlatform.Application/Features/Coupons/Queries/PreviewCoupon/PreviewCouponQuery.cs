using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Coupons;

namespace CoursePlatform.Application.Features.Coupons.Queries.PreviewCoupon;

public record CouponPreviewDto(
    decimal OriginalAmount,
    decimal DiscountAmount,
    decimal FinalAmount,
    string Code);

public record PreviewCouponQuery(Guid CourseId, string CouponCode) : IRequest<CouponPreviewDto>;

public class PreviewCouponQueryHandler : IRequestHandler<PreviewCouponQuery, CouponPreviewDto>
{
    private readonly IApplicationDbContext _context;

    public PreviewCouponQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CouponPreviewDto> Handle(PreviewCouponQuery request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken)
            ?? throw new Common.Exceptions.NotFoundException($"Course {request.CourseId} not found.");

        var pricing = await CouponPricing.ResolveAsync(
            _context,
            course.Id,
            course.Price,
            request.CouponCode,
            cancellationToken);

        return new CouponPreviewDto(
            pricing.OriginalAmount,
            pricing.DiscountAmount,
            pricing.FinalAmount,
            pricing.Coupon!.Code);
    }
}

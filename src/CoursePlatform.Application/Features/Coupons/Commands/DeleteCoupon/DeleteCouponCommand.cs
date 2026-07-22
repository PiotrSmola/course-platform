using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Coupons.Commands.DeleteCoupon;

public record DeleteCouponCommand(Guid CouponId) : IRequest;

public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCouponCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == request.CouponId, cancellationToken);

        if (coupon == null)
        {
            throw new NotFoundException($"Coupon {request.CouponId} not found.");
        }

        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

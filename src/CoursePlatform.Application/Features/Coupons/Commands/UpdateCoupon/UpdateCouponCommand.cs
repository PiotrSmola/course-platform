using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Coupons.Commands.UpdateCoupon;

public record UpdateCouponCommand(
    Guid CouponId,
    DiscountType DiscountType,
    decimal Value,
    DateTime StartsAt,
    DateTime? ExpiresAt,
    int? MaxRedemptions,
    bool IsActive,
    IReadOnlyList<Guid>? CourseIds) : IRequest;

public class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCouponCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _context.Coupons
            .Include(c => c.CouponCourses)
            .FirstOrDefaultAsync(c => c.Id == request.CouponId, cancellationToken);

        if (coupon == null)
        {
            throw new NotFoundException($"Coupon {request.CouponId} not found.");
        }

        var courseIds = request.CourseIds?.Distinct().ToList() ?? new List<Guid>();
        if (courseIds.Count > 0)
        {
            var validCount = await _context.Courses
                .CountAsync(c => courseIds.Contains(c.Id), cancellationToken);

            if (validCount != courseIds.Count)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("CourseIds", "Jeden lub więcej kursów nie istnieje.")
                });
            }
        }

        coupon.DiscountType = request.DiscountType;
        coupon.Value = request.Value;
        coupon.StartsAt = request.StartsAt;
        coupon.ExpiresAt = request.ExpiresAt;
        coupon.MaxRedemptions = request.MaxRedemptions;
        coupon.IsActive = request.IsActive;
        coupon.MarkUpdated();

        _context.CouponCourses.RemoveRange(coupon.CouponCourses);
        coupon.CouponCourses = courseIds
            .Select(id => new CouponCourse { CouponId = coupon.Id, CourseId = id })
            .ToList();

        await _context.SaveChangesAsync(cancellationToken);
    }
}

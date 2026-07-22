using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Coupons;

public record CouponPricingResult(
    Coupon? Coupon,
    decimal OriginalAmount,
    decimal DiscountAmount,
    decimal FinalAmount);

public static class CouponPricing
{
    public static async Task<CouponPricingResult> ResolveAsync(
        IApplicationDbContext context,
        Guid courseId,
        decimal coursePrice,
        string? couponCode,
        CancellationToken cancellationToken)
    {
        var original = coursePrice;

        if (string.IsNullOrWhiteSpace(couponCode))
        {
            return new CouponPricingResult(null, original, 0, original);
        }

        var normalized = couponCode.Trim().ToUpperInvariant();
        var coupon = await context.Coupons
            .Include(c => c.CouponCourses)
            .FirstOrDefaultAsync(c => c.Code == normalized, cancellationToken);

        if (coupon == null)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("CouponCode", "Nieprawidłowy kod rabatowy.")
            });
        }

        ValidateCoupon(coupon, courseId);

        var discount = CalculateDiscount(coupon, original);
        var finalAmount = Math.Max(0, original - discount);

        return new CouponPricingResult(coupon, original, discount, finalAmount);
    }

    public static void ValidateCoupon(Coupon coupon, Guid courseId, DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;

        if (!coupon.IsActive)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("CouponCode", "Kod rabatowy jest nieaktywny.")
            });
        }

        if (coupon.StartsAt > now)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("CouponCode", "Kod rabatowy nie jest jeszcze aktywny.")
            });
        }

        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt.Value < now)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("CouponCode", "Kod rabatowy wygasł.")
            });
        }

        if (coupon.MaxRedemptions.HasValue && coupon.RedeemedCount >= coupon.MaxRedemptions.Value)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("CouponCode", "Kod rabatowy wyczerpał limit użyć.")
            });
        }

        if (coupon.CouponCourses.Count > 0
            && coupon.CouponCourses.All(cc => cc.CourseId != courseId))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("CouponCode", "Kod rabatowy nie dotyczy tego kursu.")
            });
        }
    }

    public static decimal CalculateDiscount(Coupon coupon, decimal originalAmount)
    {
        if (originalAmount <= 0)
        {
            return 0;
        }

        decimal discount = coupon.DiscountType switch
        {
            DiscountType.Percentage => Math.Round(originalAmount * coupon.Value / 100m, 2, MidpointRounding.AwayFromZero),
            DiscountType.FixedAmount => coupon.Value,
            _ => 0
        };

        return Math.Min(discount, originalAmount);
    }
}

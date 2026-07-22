using FluentAssertions;
using FluentValidation;
using CoursePlatform.Application.Features.Coupons;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.Coupons;

public class CouponPricingTests
{
    [Fact]
    public void CalculateDiscount_Percentage_RoundsAwayFromZero()
    {
        var coupon = new Coupon
        {
            DiscountType = DiscountType.Percentage,
            Value = 20
        };

        CouponPricing.CalculateDiscount(coupon, 99).Should().Be(19.80m);
    }

    [Fact]
    public void CalculateDiscount_FixedAmount_ClampsToPrice()
    {
        var coupon = new Coupon
        {
            DiscountType = DiscountType.FixedAmount,
            Value = 150
        };

        CouponPricing.CalculateDiscount(coupon, 99).Should().Be(99);
    }

    [Fact]
    public void ValidateCoupon_Expired_Throws()
    {
        var coupon = new Coupon
        {
            IsActive = true,
            StartsAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            Code = "OLD"
        };

        var act = () => CouponPricing.ValidateCoupon(coupon, Guid.NewGuid());

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void ValidateCoupon_WrongCourse_Throws()
    {
        var courseId = Guid.NewGuid();
        var coupon = new Coupon
        {
            IsActive = true,
            StartsAt = DateTime.UtcNow.AddDays(-1),
            Code = "SCOPED",
            CouponCourses =
            {
                new CouponCourse { CourseId = Guid.NewGuid() }
            }
        };

        var act = () => CouponPricing.ValidateCoupon(coupon, courseId);

        act.Should().Throw<ValidationException>();
    }
}

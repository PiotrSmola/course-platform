using CoursePlatform.Domain.Common;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Domain.Entities;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal Value { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxRedemptions { get; set; }
    public int RedeemedCount { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CouponCourse> CouponCourses { get; set; } = new List<CouponCourse>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

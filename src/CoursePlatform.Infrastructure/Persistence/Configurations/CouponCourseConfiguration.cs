using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class CouponCourseConfiguration : IEntityTypeConfiguration<CouponCourse>
{
    public void Configure(EntityTypeBuilder<CouponCourse> builder)
    {
        builder.HasKey(cc => new { cc.CouponId, cc.CourseId });

        builder.HasOne(cc => cc.Coupon)
            .WithMany(c => c.CouponCourses)
            .HasForeignKey(cc => cc.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cc => cc.Course)
            .WithMany()
            .HasForeignKey(cc => cc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

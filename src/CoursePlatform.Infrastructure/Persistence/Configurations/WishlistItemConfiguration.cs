using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.HasKey(w => w.Id);

        builder.HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Course)
            .WithMany()
            .HasForeignKey(w => w.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => new { w.UserId, w.CourseId })
            .IsUnique();
    }
}

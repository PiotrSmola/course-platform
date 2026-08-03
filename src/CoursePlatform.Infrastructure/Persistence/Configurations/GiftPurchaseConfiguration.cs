using CoursePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class GiftPurchaseConfiguration : IEntityTypeConfiguration<GiftPurchase>
{
    public void Configure(EntityTypeBuilder<GiftPurchase> builder)
    {
        builder.HasKey(gift => gift.Id);

        builder.Property(gift => gift.RecipientEmail)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(gift => gift.CodeHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(gift => gift.ProtectedCode)
            .IsRequired();

        builder.Property(gift => gift.Amount)
            .HasPrecision(18, 2);

        builder.Property(gift => gift.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(gift => gift.StripeSessionId)
            .HasMaxLength(255);

        builder.HasIndex(gift => gift.CodeHash)
            .IsUnique();

        builder.HasIndex(gift => gift.StripeSessionId)
            .IsUnique();

        builder.HasIndex(gift => new { gift.BuyerUserId, gift.CreatedAt });

        builder.HasOne(gift => gift.BuyerUser)
            .WithMany()
            .HasForeignKey(gift => gift.BuyerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(gift => gift.Course)
            .WithMany()
            .HasForeignKey(gift => gift.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(gift => gift.RedeemedByUser)
            .WithMany()
            .HasForeignKey(gift => gift.RedeemedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

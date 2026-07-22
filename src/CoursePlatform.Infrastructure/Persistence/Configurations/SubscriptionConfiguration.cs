using CoursePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.StripeCustomerId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.StripeSubscriptionId)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(s => s.UserId)
            .IsUnique();

        builder.HasIndex(s => s.StripeCustomerId)
            .IsUnique();

        builder.HasIndex(s => s.StripeSubscriptionId)
            .IsUnique();

        builder.HasOne(s => s.User)
            .WithOne(u => u.Subscription)
            .HasForeignKey<Subscription>(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

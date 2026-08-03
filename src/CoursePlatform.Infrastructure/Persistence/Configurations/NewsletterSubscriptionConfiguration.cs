using CoursePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class NewsletterSubscriptionConfiguration : IEntityTypeConfiguration<NewsletterSubscription>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscription> builder)
    {
        builder.HasKey(subscription => subscription.Id);

        builder.Property(subscription => subscription.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(subscription => subscription.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(subscription => subscription.ConfirmationTokenHash)
            .HasMaxLength(128);

        builder.Property(subscription => subscription.UnsubscribeTokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(subscription => subscription.NormalizedEmail)
            .IsUnique();

        builder.HasIndex(subscription => subscription.UnsubscribeTokenHash)
            .IsUnique();
    }
}

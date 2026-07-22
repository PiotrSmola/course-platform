using CoursePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class SubscriptionInvoiceConfiguration : IEntityTypeConfiguration<SubscriptionInvoice>
{
    public void Configure(EntityTypeBuilder<SubscriptionInvoice> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Amount)
            .HasPrecision(18, 2);

        builder.Property(i => i.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(i => i.StripeInvoiceId)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(i => i.StripeInvoiceId)
            .IsUnique();

        builder.HasOne(i => i.Subscription)
            .WithMany(s => s.Invoices)
            .HasForeignKey(i => i.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

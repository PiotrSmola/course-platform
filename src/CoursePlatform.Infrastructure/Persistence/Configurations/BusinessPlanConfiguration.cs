using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class BusinessPlanConfiguration : IEntityTypeConfiguration<BusinessPlan>
{
    public void Configure(EntityTypeBuilder<BusinessPlan> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.ShortDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Currency)
            .HasMaxLength(10);

        builder.Property(p => p.PriceLabel)
            .HasMaxLength(100);

        builder.Property(p => p.CallToActionText)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.CallToActionUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.BillingPeriod)
            .HasConversion<int?>();

        builder.HasIndex(p => p.Slug).IsUnique();
    }
}
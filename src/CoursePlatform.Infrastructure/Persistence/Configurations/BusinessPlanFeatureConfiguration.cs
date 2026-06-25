using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class BusinessPlanFeatureConfiguration : IEntityTypeConfiguration<BusinessPlanFeature>
{
    public void Configure(EntityTypeBuilder<BusinessPlanFeature> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Text)
            .IsRequired()
            .HasMaxLength(300);

        builder.HasOne(f => f.BusinessPlan)
            .WithMany(p => p.Features)
            .HasForeignKey(f => f.BusinessPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => new { f.BusinessPlanId, f.DisplayOrder });
    }
}
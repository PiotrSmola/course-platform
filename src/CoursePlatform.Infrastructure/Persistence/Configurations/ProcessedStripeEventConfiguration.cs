using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class ProcessedStripeEventConfiguration : IEntityTypeConfiguration<ProcessedStripeEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedStripeEvent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.StripeEventId)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(e => e.StripeEventId)
            .IsUnique();

        builder.Property(e => e.ProcessedAt)
            .IsRequired();
    }
}

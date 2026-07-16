using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(al => al.Id);

        builder.Property(al => al.Action)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(al => al.EntityType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(al => al.EntityId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(al => al.Details)
            .IsRequired()
            .HasMaxLength(2048);

        builder.HasIndex(al => al.CreatedAt);
        builder.HasIndex(al => new { al.EntityType, al.EntityId });

        builder.HasOne(al => al.User)
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

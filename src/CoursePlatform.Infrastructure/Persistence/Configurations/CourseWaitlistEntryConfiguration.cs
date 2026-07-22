using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class CourseWaitlistEntryConfiguration : IEntityTypeConfiguration<CourseWaitlistEntry>
{
    public void Configure(EntityTypeBuilder<CourseWaitlistEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserId, e.CourseId })
            .IsUnique();
    }
}

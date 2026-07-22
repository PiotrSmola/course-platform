using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class LessonResourceConfiguration : IEntityTypeConfiguration<LessonResource>
{
    public void Configure(EntityTypeBuilder<LessonResource> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.ObjectKey)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(r => r.ContentType)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasOne(r => r.Lesson)
            .WithMany(l => l.Resources)
            .HasForeignKey(r => r.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .IsRequired();

        builder.Property(c => c.ShortDescription)
            .HasMaxLength(500);

        builder.Property(c => c.Price)
            .HasPrecision(18, 2);

        builder.Property(c => c.ThumbnailObjectKey)
            .HasMaxLength(1024);

        builder.Property(c => c.Language)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(c => c.Instructor)
            .WithMany(u => u.Courses)
            .HasForeignKey(c => c.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Categories)
            .WithMany(cat => cat.Courses);

        builder.HasMany(c => c.Technologies)
            .WithMany(tech => tech.Courses);

        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.Language);
    }
}

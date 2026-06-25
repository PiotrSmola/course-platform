using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class LearningPathCourseConfiguration : IEntityTypeConfiguration<LearningPathCourse>
{
    public void Configure(EntityTypeBuilder<LearningPathCourse> builder)
    {
        builder.HasKey(pc => pc.Id);

        builder.HasOne(pc => pc.LearningPath)
            .WithMany(p => p.PathCourses)
            .HasForeignKey(pc => pc.LearningPathId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pc => pc.Course)
            .WithMany()
            .HasForeignKey(pc => pc.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pc => new { pc.LearningPathId, pc.CourseId }).IsUnique();
    }
}
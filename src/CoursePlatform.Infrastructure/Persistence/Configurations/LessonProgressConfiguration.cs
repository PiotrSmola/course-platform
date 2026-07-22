using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class LessonProgressConfiguration : IEntityTypeConfiguration<LessonProgress>
{
    public void Configure(EntityTypeBuilder<LessonProgress> builder)
    {
        builder.HasKey(lp => new { lp.UserId, lp.LessonId });

        builder.Property(lp => lp.LastPositionSeconds)
            .HasDefaultValue(0);

        builder.HasOne(lp => lp.User)
            .WithMany(u => u.LessonProgresses)
            .HasForeignKey(lp => lp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lp => lp.Lesson)
            .WithMany(l => l.LessonProgresses)
            .HasForeignKey(lp => lp.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

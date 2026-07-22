using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class LessonQuestionConfiguration : IEntityTypeConfiguration<LessonQuestion>
{
    public void Configure(EntityTypeBuilder<LessonQuestion> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Body)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(q => q.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(q => q.Lesson)
            .WithMany(l => l.Questions)
            .HasForeignKey(q => q.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(q => q.Author)
            .WithMany(u => u.LessonQuestions)
            .HasForeignKey(q => q.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(q => new { q.LessonId, q.CreatedAt });
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class LessonAnswerConfiguration : IEntityTypeConfiguration<LessonAnswer>
{
    public void Configure(EntityTypeBuilder<LessonAnswer> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Body)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(a => a.IsInstructorAnswer)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Author)
            .WithMany(u => u.LessonAnswers)
            .HasForeignKey(a => a.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.QuestionId, a.CreatedAt });
    }
}

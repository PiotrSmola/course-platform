using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Prompt)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(q => q.Quiz)
            .WithMany(z => z.Questions)
            .HasForeignKey(q => q.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(q => new { q.QuizId, q.Order });
    }
}

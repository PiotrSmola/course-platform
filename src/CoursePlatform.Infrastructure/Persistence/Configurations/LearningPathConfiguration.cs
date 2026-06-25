using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class LearningPathConfiguration : IEntityTypeConfiguration<LearningPath>
{
    public void Configure(EntityTypeBuilder<LearningPath> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.ShortDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Description)
            .IsRequired();

        builder.Property(p => p.ThumbnailUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.DifficultyLevel)
            .HasConversion<int>();

        builder.HasIndex(p => p.Slug).IsUnique();
    }
}
using CoursePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class TrialAccessConfiguration : IEntityTypeConfiguration<TrialAccess>
{
    public void Configure(EntityTypeBuilder<TrialAccess> builder)
    {
        builder.HasKey(access => access.Id);

        builder.HasIndex(access => access.UserId)
            .IsUnique();

        builder.HasIndex(access => new { access.CourseId, access.ActivatedAt });

        builder.HasOne(access => access.User)
            .WithMany()
            .HasForeignKey(access => access.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(access => access.Course)
            .WithMany()
            .HasForeignKey(access => access.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations;

public class UserStatisticsConfiguration : IEntityTypeConfiguration<UserStatistics>
{
    public void Configure(EntityTypeBuilder<UserStatistics> builder)
    {
        builder.HasKey(us => us.UserId);

        builder.Property(us => us.AverageProgressPercentage)
            .HasPrecision(5, 2);

        builder.HasOne(us => us.User)
            .WithOne(u => u.Statistics)
            .HasForeignKey<UserStatistics>(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

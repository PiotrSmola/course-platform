using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.UnitTests.Common;

public class TestDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Technology> Technologies => Set<Technology>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<LearningPath> LearningPaths => Set<LearningPath>();
    public DbSet<LearningPathCourse> LearningPathCourses => Set<LearningPathCourse>();
    public DbSet<BusinessPlan> BusinessPlans => Set<BusinessPlan>();
    public DbSet<BusinessPlanFeature> BusinessPlanFeatures => Set<BusinessPlanFeature>();
    public DbSet<UserStatistics> UserStatistics => Set<UserStatistics>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<LessonProgress>().HasKey(lp => new { lp.UserId, lp.LessonId });
        builder.Entity<UserStatistics>().HasKey(us => us.UserId);
        builder.Entity<ApplicationUser>().HasKey(u => u.Id);
        builder.Entity<LearningPathCourse>().HasKey(lpc => lpc.Id);
        builder.Entity<BusinessPlanFeature>().HasKey(bf => bf.Id);
        builder.Entity<BusinessPlan>().HasKey(bp => bp.Id);
        builder.Entity<LearningPath>().HasKey(lp => lp.Id);
        builder.Entity<Review>().HasKey(r => r.Id);
        builder.Entity<Enrollment>().HasKey(e => e.Id);
        builder.Entity<Lesson>().HasKey(l => l.Id);
        builder.Entity<Module>().HasKey(m => m.Id);
        builder.Entity<Course>().HasKey(c => c.Id);
        builder.Entity<Category>().HasKey(c => c.Id);
        builder.Entity<Technology>().HasKey(t => t.Id);
    }
}

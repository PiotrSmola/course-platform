using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Application.Common.Interfaces;
using System.Reflection;

namespace CoursePlatform.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Technology> Technologies => Set<Technology>();
    public DbSet<Domain.Entities.Module> Modules => Set<Domain.Entities.Module>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<LearningPath> LearningPaths => Set<LearningPath>();
    public DbSet<LearningPathCourse> LearningPathCourses => Set<LearningPathCourse>();
    public DbSet<BusinessPlan> BusinessPlans => Set<BusinessPlan>();
    public DbSet<BusinessPlanFeature> BusinessPlanFeatures => Set<BusinessPlanFeature>();
    public DbSet<UserStatistics> UserStatistics => Set<UserStatistics>();
    public new DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

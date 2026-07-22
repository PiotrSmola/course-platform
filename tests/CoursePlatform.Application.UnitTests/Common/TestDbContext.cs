using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Infrastructure.Persistence.Configurations;

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
    public DbSet<LessonQuestion> LessonQuestions => Set<LessonQuestion>();
    public DbSet<LessonAnswer> LessonAnswers => Set<LessonAnswer>();
    public DbSet<LessonResource> LessonResources => Set<LessonResource>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizOption> QuizOptions => Set<QuizOption>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers => Set<QuizAttemptAnswer>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponCourse> CouponCourses => Set<CouponCourse>();
    public DbSet<LearningPath> LearningPaths => Set<LearningPath>();


    public DbSet<LearningPathCourse> LearningPathCourses => Set<LearningPathCourse>();
    public DbSet<BusinessPlan> BusinessPlans => Set<BusinessPlan>();
    public DbSet<BusinessPlanFeature> BusinessPlanFeatures => Set<BusinessPlanFeature>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionInvoice> SubscriptionInvoices => Set<SubscriptionInvoice>();
    public DbSet<ProcessedStripeEvent> ProcessedStripeEvents => Set<ProcessedStripeEvent>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<CourseWaitlistEntry> CourseWaitlistEntries => Set<CourseWaitlistEntry>();

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>().HasKey(u => u.Id);
        builder.ApplyConfigurationsFromAssembly(typeof(CourseConfiguration).Assembly);
    }
}

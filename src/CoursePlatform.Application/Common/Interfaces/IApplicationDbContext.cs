using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Course> Courses { get; }
    DbSet<Category> Categories { get; }
    DbSet<Technology> Technologies { get; }
    DbSet<Module> Modules { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<Enrollment> Enrollments { get; }
    DbSet<LessonProgress> LessonProgresses { get; }
    DbSet<Review> Reviews { get; }
    DbSet<LessonQuestion> LessonQuestions { get; }
    DbSet<LessonAnswer> LessonAnswers { get; }
    DbSet<LessonResource> LessonResources { get; }
    DbSet<Quiz> Quizzes { get; }
    DbSet<QuizQuestion> QuizQuestions { get; }
    DbSet<QuizOption> QuizOptions { get; }
    DbSet<QuizAttempt> QuizAttempts { get; }
    DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<CouponCourse> CouponCourses { get; }
    DbSet<LearningPath> LearningPaths { get; }

    DbSet<LearningPathCourse> LearningPathCourses { get; }
    DbSet<BusinessPlan> BusinessPlans { get; }
    DbSet<BusinessPlanFeature> BusinessPlanFeatures { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<SubscriptionInvoice> SubscriptionInvoices { get; }
    DbSet<ProcessedStripeEvent> ProcessedStripeEvents { get; }
    DbSet<Certificate> Certificates { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<ApplicationUser> Users { get; }
    DbSet<WishlistItem> WishlistItems { get; }
    DbSet<CourseWaitlistEntry> CourseWaitlistEntries { get; }
    DbSet<TrialAccess> TrialAccesses { get; }
    DbSet<GiftPurchase> GiftPurchases { get; }
    DbSet<NewsletterSubscription> NewsletterSubscriptions { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
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
    DbSet<LearningPath> LearningPaths { get; }
    DbSet<LearningPathCourse> LearningPathCourses { get; }
    DbSet<BusinessPlan> BusinessPlans { get; }
    DbSet<BusinessPlanFeature> BusinessPlanFeatures { get; }
    DbSet<UserStatistics> UserStatistics { get; }
    DbSet<ApplicationUser> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

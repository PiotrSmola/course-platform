namespace CoursePlatform.Domain.Entities;

public class UserStatistics
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public int TotalEnrollments { get; set; }
    public int CompletedCourses { get; set; }
    public int TotalLessonsCompleted { get; set; }
    public int TotalLessonsAvailable { get; set; }
    public double AverageProgressPercentage { get; set; }
    public int TotalLearningTimeSeconds { get; set; }
    public int CertificatesEarned { get; set; }
    public DateTime? LastActivityAt { get; set; }
}

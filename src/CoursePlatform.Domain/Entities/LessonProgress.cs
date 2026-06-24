namespace CoursePlatform.Domain.Entities;

public class LessonProgress
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

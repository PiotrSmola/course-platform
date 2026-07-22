using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class Quiz : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public int PassThresholdPercent { get; set; } = 70;

    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}

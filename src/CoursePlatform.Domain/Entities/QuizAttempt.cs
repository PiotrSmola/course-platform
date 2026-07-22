using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class QuizAttempt : BaseEntity
{
    public int ScorePercent { get; set; }
    public bool Passed { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public Guid QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public ICollection<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
}

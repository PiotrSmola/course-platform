using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class QuizAttemptAnswer : BaseEntity
{
    public Guid AttemptId { get; set; }
    public QuizAttempt Attempt { get; set; } = null!;

    public Guid QuestionId { get; set; }
    public QuizQuestion Question { get; set; } = null!;

    public Guid SelectedOptionId { get; set; }
    public QuizOption SelectedOption { get; set; } = null!;
}

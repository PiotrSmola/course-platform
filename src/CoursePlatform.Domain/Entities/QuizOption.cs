using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class QuizOption : BaseEntity
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Order { get; set; }

    public Guid QuestionId { get; set; }
    public QuizQuestion Question { get; set; } = null!;
}

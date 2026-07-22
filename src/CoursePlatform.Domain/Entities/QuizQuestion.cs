using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class QuizQuestion : BaseEntity
{
    public string Prompt { get; set; } = string.Empty;
    public int Order { get; set; }

    public Guid QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();
}

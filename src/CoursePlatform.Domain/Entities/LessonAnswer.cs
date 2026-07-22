using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class LessonAnswer : BaseEntity
{
    public string Body { get; set; } = string.Empty;
    public bool IsInstructorAnswer { get; set; }
    public bool IsDeleted { get; set; }

    public Guid QuestionId { get; set; }
    public LessonQuestion Question { get; set; } = null!;

    public Guid AuthorId { get; set; }
    public ApplicationUser Author { get; set; } = null!;
}

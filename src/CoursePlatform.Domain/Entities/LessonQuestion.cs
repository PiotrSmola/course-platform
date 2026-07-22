using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class LessonQuestion : BaseEntity
{
    public string Body { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public Guid AuthorId { get; set; }
    public ApplicationUser Author { get; set; } = null!;

    public ICollection<LessonAnswer> Answers { get; set; } = new List<LessonAnswer>();
}

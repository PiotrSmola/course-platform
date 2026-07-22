using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class Lesson : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string VideoObjectKey { get; set; } = string.Empty;
    public int Duration { get; set; }
    public int Order { get; set; }

    public Guid ModuleId { get; set; }
    public Module Module { get; set; } = null!;

    public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    public ICollection<LessonQuestion> Questions { get; set; } = new List<LessonQuestion>();
    public ICollection<LessonResource> Resources { get; set; } = new List<LessonResource>();
    public Quiz? Quiz { get; set; }
}



using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class Review : BaseEntity
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;
}

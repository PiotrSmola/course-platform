using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class Technology : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}

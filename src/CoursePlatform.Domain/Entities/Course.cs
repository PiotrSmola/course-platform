using CoursePlatform.Domain.Common;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Domain.Entities;

public class Course : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public CourseLevel Level { get; set; }
    public CourseStatus Status { get; set; } = CourseStatus.Draft;
    public string ThumbnailUrl { get; set; } = string.Empty;

    public Guid InstructorId { get; set; }
    public ApplicationUser Instructor { get; set; } = null!;

    public ICollection<Module> Modules { get; set; } = new List<Module>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}

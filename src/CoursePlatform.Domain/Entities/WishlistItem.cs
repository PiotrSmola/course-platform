using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class WishlistItem : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;
}

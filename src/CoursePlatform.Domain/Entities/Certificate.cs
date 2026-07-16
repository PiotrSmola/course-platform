using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class Certificate : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public string Number { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public string? PdfObjectKey { get; set; }
}

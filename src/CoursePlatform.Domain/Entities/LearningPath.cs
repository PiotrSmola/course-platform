using CoursePlatform.Domain.Common;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Domain.Entities;

public class LearningPath : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PathDifficultyLevel DifficultyLevel { get; set; }
    public int EstimatedHours { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;

    public ICollection<LearningPathCourse> PathCourses { get; set; } = new List<LearningPathCourse>();
}
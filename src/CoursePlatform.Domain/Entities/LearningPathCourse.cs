using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities;

public class LearningPathCourse : BaseEntity
{
    public Guid LearningPathId { get; set; }
    public LearningPath LearningPath { get; set; } = null!;

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int Order { get; set; }
    public bool IsOptional { get; set; }
}
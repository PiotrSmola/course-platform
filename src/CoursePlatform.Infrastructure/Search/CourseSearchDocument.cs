namespace CoursePlatform.Infrastructure.Search;

public sealed class CourseSearchDocument
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public long PriceMinorUnits { get; init; }
    public int Level { get; init; }
    public int Status { get; init; }
    public string Language { get; init; } = string.Empty;
    public string InstructorName { get; init; } = string.Empty;
    public string? ThumbnailObjectKey { get; init; }
    public IReadOnlyCollection<Guid> CategoryIds { get; init; } = Array.Empty<Guid>();
    public IReadOnlyCollection<Guid> TechnologyIds { get; init; } = Array.Empty<Guid>();
    public IReadOnlyCollection<string> CategoryNames { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> TechnologyNames { get; init; } = Array.Empty<string>();
    public int ModuleCount { get; init; }
    public int LessonCount { get; init; }
    public double AverageRating { get; init; }
    public int ReviewCount { get; init; }
    public DateTime CreatedAt { get; init; }
}


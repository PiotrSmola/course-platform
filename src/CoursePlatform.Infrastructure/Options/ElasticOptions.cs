namespace CoursePlatform.Infrastructure.Options;

public sealed class ElasticOptions
{
    public bool Enabled { get; init; } = false;
    public string Uri { get; init; } = "http://localhost:9200";
    public string CourseIndexName { get; init; } = "courseplatform-courses";
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? ApiKey { get; init; }
}


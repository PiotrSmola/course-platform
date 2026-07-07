namespace CoursePlatform.Infrastructure.Options;

public sealed class ElasticOptions
{
    public bool Enabled { get; init; } = false;
    public string Uri { get; init; } = "http://localhost:9200";
    public string CourseIndexName { get; init; } = "courseplatform-courses";
}


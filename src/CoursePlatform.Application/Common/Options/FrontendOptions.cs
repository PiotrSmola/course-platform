namespace CoursePlatform.Application.Common.Options;

public sealed class FrontendOptions
{
    public const string SectionName = "Frontend";

    public string BaseUrl { get; init; } = "http://localhost:5173";
}

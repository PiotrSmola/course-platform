namespace CoursePlatform.Application.Common.Options;

public sealed class SupportOptions
{
    public const string SectionName = "Support";

    public string InboxAddress { get; init; } = string.Empty;
}

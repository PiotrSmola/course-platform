namespace CoursePlatform.Infrastructure.Options;

public sealed class EmailOptions
{
    public bool Enabled { get; init; }
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 1025;
    public string FromAddress { get; init; } = "no-reply@courseplatform.local";
    public string FromName { get; init; } = "CoursePlatform";
}

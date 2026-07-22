namespace CoursePlatform.Application.Features.LessonResources;

public record LessonResourceDto(
    Guid Id,
    string Title,
    string ContentType,
    long SizeBytes,
    int Order);

public record LessonResourceDownloadDto(string Url);

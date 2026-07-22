namespace CoursePlatform.Application.Common.Helpers;

public static class UploadLimits
{
    public const long MaxThumbnailBytes = 5L * 1024 * 1024;
    public const long MaxVideoBytes = 200L * 1024 * 1024;
    public const int VideoPartSizeBytes = 15 * 1024 * 1024;
    public const int MaxVideoParts = (int)((MaxVideoBytes + VideoPartSizeBytes - 1) / VideoPartSizeBytes);

    public static readonly IReadOnlyCollection<string> AllowedThumbnailContentTypes =
        new[] { "image/jpeg", "image/png", "image/webp" };

    public static readonly IReadOnlyCollection<string> AllowedVideoContentTypes =
        new[] { "video/mp4", "video/webm", "video/quicktime" };

    public const long MaxLessonResourceBytes = 25L * 1024 * 1024;

    public static readonly IReadOnlyCollection<string> AllowedLessonResourceContentTypes =
        new[] { "application/pdf", "application/zip", "text/plain", "application/json" };
}

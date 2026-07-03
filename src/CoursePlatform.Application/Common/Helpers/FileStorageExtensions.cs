using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Common.Helpers;

public static class FileStorageExtensions
{
    private static readonly TimeSpan ThumbnailUrlExpiration = TimeSpan.FromMinutes(15);

    public static async Task<string?> GetThumbnailUrlOrNullAsync(
        this IFileStorageService fileStorage,
        string objectKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            return null;
        }

        return await fileStorage.GetPresignedDownloadUrlAsync(objectKey, ThumbnailUrlExpiration, cancellationToken);
    }
}

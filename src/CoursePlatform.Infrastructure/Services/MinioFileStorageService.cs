using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Infrastructure.Services;

public class MinioFileStorageService : IFileStorageService
{
    public Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"https://placeholder.local/{fileName}");
    }

    public Task<string> GetPresignedUrlAsync(string fileName, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"https://placeholder.local/{fileName}");
    }

    public Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

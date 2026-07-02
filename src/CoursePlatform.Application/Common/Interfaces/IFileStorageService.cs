namespace CoursePlatform.Application.Common.Interfaces;

using CoursePlatform.Application.Common.Models;

public interface IFileStorageService
{
    Task<string> GetPresignedDownloadUrlAsync(
        string objectKey,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);

    Task<string> GetPresignedPutUrlAsync(
        string objectKey,
        string contentType,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);

    Task<string> CreateMultipartUploadAsync(
        string objectKey,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<string> GetPresignedUploadPartUrlAsync(
        string objectKey,
        string uploadId,
        int partNumber,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);

    Task CompleteMultipartUploadAsync(
        string objectKey,
        string uploadId,
        IReadOnlyList<CompletedPart> parts,
        CancellationToken cancellationToken = default);

    Task AbortMultipartUploadAsync(
        string objectKey,
        string uploadId,
        CancellationToken cancellationToken = default);

    Task<StoredObjectStat?> StatObjectAsync(
        string objectKey,
        CancellationToken cancellationToken = default);
}

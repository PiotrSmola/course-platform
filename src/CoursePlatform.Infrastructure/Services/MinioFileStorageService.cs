using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Models;
using CoursePlatform.Infrastructure.Options;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Options;

namespace CoursePlatform.Infrastructure.Services;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly MinioOptions _options;
    private readonly IAmazonS3 _presignS3;
    private readonly Uri _publicEndpoint;

    public MinioFileStorageService(IAmazonS3 s3, IOptions<MinioOptions> options)
    {
        _s3 = s3;
        _options = options.Value;
        _publicEndpoint = NormalizePublicEndpoint(_options.PublicEndpoint);
        _presignS3 = CreatePresignClient(_options);
    }

    public Task<string> GetPresignedDownloadUrlAsync(
        string objectKey,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var url = _presignS3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiration)
        });
        return Task.FromResult(NormalizeScheme(url));
    }

    public Task<string> GetPresignedPutUrlAsync(
        string objectKey,
        string contentType,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var url = _presignS3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(expiration),
            ContentType = contentType
        });
        return Task.FromResult(NormalizeScheme(url));
    }

    public async Task<string> CreateMultipartUploadAsync(
        string objectKey,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var response = await _s3.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest
        {
            BucketName = _options.Bucket,
            Key = objectKey,
            ContentType = contentType
        }, cancellationToken);

        return response.UploadId;
    }

    public Task<string> GetPresignedUploadPartUrlAsync(
        string objectKey,
        string uploadId,
        int partNumber,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var url = _presignS3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(expiration),
            UploadId = uploadId,
            PartNumber = partNumber
        });

        return Task.FromResult(NormalizeScheme(url));
    }

    public async Task CompleteMultipartUploadAsync(
        string objectKey,
        string uploadId,
        IReadOnlyList<CompletedPart> parts,
        CancellationToken cancellationToken = default)
    {
        var partEtags = parts
            .OrderBy(p => p.PartNumber)
            .Select(p => new PartETag(p.PartNumber, p.ETag.Trim('"')))
            .ToList();

        await _s3.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest
        {
            BucketName = _options.Bucket,
            Key = objectKey,
            UploadId = uploadId,
            PartETags = partEtags
        }, cancellationToken);
    }

    public async Task AbortMultipartUploadAsync(
        string objectKey,
        string uploadId,
        CancellationToken cancellationToken = default)
    {
        await _s3.AbortMultipartUploadAsync(new AbortMultipartUploadRequest
        {
            BucketName = _options.Bucket,
            Key = objectKey,
            UploadId = uploadId
        }, cancellationToken);
    }

    public async Task<StoredObjectStat?> StatObjectAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var meta = await _s3.GetObjectMetadataAsync(new GetObjectMetadataRequest
            {
                BucketName = _options.Bucket,
                Key = objectKey
            }, cancellationToken);

            return new StoredObjectStat(meta.ContentLength, meta.Headers.ContentType);
        }
        catch (AmazonS3Exception e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    private static IAmazonS3 CreatePresignClient(MinioOptions options)
    {
        var publicEndpoint = NormalizePublicEndpoint(options.PublicEndpoint);

        var credentials = new BasicAWSCredentials(options.AccessKey, options.SecretKey);

        var config = new AmazonS3Config
        {
            ServiceURL = publicEndpoint.ToString().TrimEnd('/'),
            UseHttp = publicEndpoint.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase),
            ForcePathStyle = true,
            AuthenticationRegion = options.Region
        };

        return new AmazonS3Client(credentials, config);
    }

    private static Uri NormalizePublicEndpoint(string publicEndpoint)
    {
        var uri = new Uri(publicEndpoint);
        if (uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) && uri.Port == 9000)
        {
            return new UriBuilder(uri) { Scheme = Uri.UriSchemeHttp, Port = 9000 }.Uri;
        }

        return uri;
    }

    private string NormalizeScheme(string presignedUrl)
    {
        if (_publicEndpoint.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && presignedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return "http://" + presignedUrl["https://".Length..];
        }

        return presignedUrl;
    }
}

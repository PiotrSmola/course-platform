using Amazon.S3;
using CoursePlatform.Infrastructure.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace CoursePlatform.API.HealthChecks;

public class MinioHealthCheck : IHealthCheck
{
    private readonly IAmazonS3 _s3;
    private readonly MinioOptions _options;

    public MinioHealthCheck(IAmazonS3 s3, IOptions<MinioOptions> options)
    {
        _s3 = s3;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Cheap connectivity probe — no bucket enumeration, just a metadata round-trip.
            await _s3.GetBucketLocationAsync(_options.Bucket, cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("MinIO is unreachable.", ex);
        }
    }
}

using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CoursePlatform.API.HealthChecks;

public class ElasticsearchHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider;

    public ElasticsearchHealthCheck(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var client = _serviceProvider.GetService<ElasticsearchClient>();
        if (client == null)
        {
            return HealthCheckResult.Degraded("Elasticsearch is disabled.");
        }

        try
        {
            var response = await client.PingAsync(cancellationToken);
            return response.IsValidResponse
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Elasticsearch ping failed.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Elasticsearch is unreachable.", ex);
        }
    }
}

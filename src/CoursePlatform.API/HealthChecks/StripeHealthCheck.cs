using CoursePlatform.Application.Common.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CoursePlatform.API.HealthChecks;

public class StripeHealthCheck : IHealthCheck
{
    private readonly IPaymentGateway _paymentGateway;

    public StripeHealthCheck(IPaymentGateway paymentGateway)
    {
        _paymentGateway = paymentGateway;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_paymentGateway.IsConfigured
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Degraded("Stripe is not configured."));
    }
}

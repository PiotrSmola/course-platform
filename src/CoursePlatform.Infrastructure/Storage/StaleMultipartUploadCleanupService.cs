using CoursePlatform.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoursePlatform.Infrastructure.Storage;

public sealed class StaleMultipartUploadCleanupService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
    private static readonly TimeSpan OlderThan = TimeSpan.FromHours(24);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StaleMultipartUploadCleanupService> _logger;

    public StaleMultipartUploadCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<StaleMultipartUploadCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Stale multipart upload cleanup pass failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
        var aborted = await storage.AbortStaleMultipartUploadsAsync(OlderThan, cancellationToken);
        if (aborted > 0)
        {
            _logger.LogInformation("Aborted {Count} stale multipart upload(s) older than {OlderThan}.", aborted, OlderThan);
        }
    }
}

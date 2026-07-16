using CoursePlatform.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace CoursePlatform.Infrastructure.Caching;

internal sealed class HybridAppCache : IAppCache
{
    private readonly HybridCache _cache;
    private readonly ILogger<HybridAppCache> _logger;

    public HybridAppCache(HybridCache cache, ILogger<HybridAppCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async ValueTask<T> GetOrCreateAsync<T>(
        string key,
        TimeSpan ttl,
        Func<CancellationToken, ValueTask<T>> factory,
        IReadOnlyCollection<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _cache.GetOrCreateAsync(
                key,
                factory,
                new HybridCacheEntryOptions { Expiration = ttl },
                tags,
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Cache lookup for {Key} failed, falling back to source.", key);
            return await factory(cancellationToken);
        }
    }

    public async ValueTask InvalidateTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveByTagAsync(tag, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Cache tag invalidation for {Tag} failed.", tag);
        }
    }
}

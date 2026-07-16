namespace CoursePlatform.Application.Common.Interfaces;

public interface IAppCache
{
    ValueTask<T> GetOrCreateAsync<T>(
        string key,
        TimeSpan ttl,
        Func<CancellationToken, ValueTask<T>> factory,
        IReadOnlyCollection<string>? tags = null,
        CancellationToken cancellationToken = default);

    ValueTask InvalidateTagAsync(string tag, CancellationToken cancellationToken = default);
}

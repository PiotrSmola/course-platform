using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.UnitTests.Common;

public sealed class PassThroughAppCache : IAppCache
{
    public List<string> InvalidatedTags { get; } = new();

    public async ValueTask<T> GetOrCreateAsync<T>(
        string key,
        TimeSpan ttl,
        Func<CancellationToken, ValueTask<T>> factory,
        IReadOnlyCollection<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        return await factory(cancellationToken);
    }

    public ValueTask InvalidateTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        InvalidatedTags.Add(tag);
        return ValueTask.CompletedTask;
    }
}

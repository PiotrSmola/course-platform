using CoursePlatform.Application.Features.Admin.Search;

namespace CoursePlatform.Application.Common.Interfaces;

public interface IReindexJobService
{
    ReindexJobState? GetCurrent();

    Task<ReindexStartResult> StartAsync(CancellationToken cancellationToken);
}

public sealed record ReindexStartResult(ReindexJobState State, bool AlreadyRunning)
{
    public static ReindexStartResult Started(ReindexJobState state) => new(state, false);
    public static ReindexStartResult AlreadyInProgress(ReindexJobState state) => new(state, true);
}
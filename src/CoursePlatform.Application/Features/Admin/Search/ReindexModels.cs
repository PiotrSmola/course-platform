namespace CoursePlatform.Application.Features.Admin.Search;

public enum ReindexStatus
{
    Idle,
    Running,
    Succeeded,
    Failed,
    Cancelled
}

public enum ReindexPhase
{
    Counting,
    PreparingIndex,
    Indexing,
    SwitchingAlias,
    CleaningUp,
    Done
}

public enum ReindexLogLevel
{
    Info,
    Warning,
    Error
}

public sealed record ReindexLogEntry(
    DateTimeOffset Timestamp,
    ReindexLogLevel Level,
    string Message);

public sealed record ReindexProgress(
    int Total,
    int Processed,
    int Failed,
    int BatchSize,
    int BatchesCompleted,
    int BatchesTotal,
    double Percent,
    ReindexPhase Phase = ReindexPhase.Indexing);

public sealed record ReindexJobState(
    Guid JobId,
    ReindexStatus Status,
    ReindexPhase Phase,
    ReindexProgress Progress,
    IReadOnlyList<ReindexLogEntry> Logs,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    string? ErrorMessage);

public sealed record CourseIndexStats(
    bool Exists,
    bool Enabled,
    string IndexName,
    string AliasName,
    string? ConcreteIndexName,
    long DocumentCount,
    long? SizeBytes,
    string Health);
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Admin.Search;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Infrastructure.Options;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoursePlatform.Infrastructure.Search;

public sealed class ElasticCourseIndexingService : ICourseIndexingService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticOptions _options;
    private readonly IApplicationDbContext _db;
    private readonly ILogger<ElasticCourseIndexingService> _logger;

    public ElasticCourseIndexingService(
        ElasticsearchClient client,
        IOptions<ElasticOptions> options,
        IApplicationDbContext db,
        ILogger<ElasticCourseIndexingService> logger)
    {
        _client = client;
        _options = options.Value;
        _db = db;
        _logger = logger;
    }

    public async Task EnsureIndexAsync(CancellationToken cancellationToken)
    {
        var aliasResponse = await _client.Indices.GetAliasAsync(Indices.Index(_options.CourseIndexName), cancellationToken);
        if (aliasResponse.IsValidResponse && aliasResponse.Values?.Count > 0) return;

        var legacyIndexExists = await _client.Indices.ExistsAsync(_options.CourseIndexName, cancellationToken);
        if (legacyIndexExists.Exists)
        {
            _logger.LogWarning(
                "Found legacy index '{IndexName}' without an alias. Rebuilding it as an aliased index.",
                _options.CourseIndexName);
            await ReindexCoursesAsync(cancellationToken);
            return;
        }

        var indexName = $"{_options.CourseIndexName}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        await CreateIndexAsync(indexName, cancellationToken);

        var putAliasResponse = await _client.Indices.PutAliasAsync(indexName, _options.CourseIndexName, cancellationToken);
        if (!putAliasResponse.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to create alias '{_options.CourseIndexName}'.");
        }
    }

    public async Task IndexCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        try
        {
            var doc = await ProjectToDocuments(_db.Courses.Where(c => c.Id == courseId))
                .FirstOrDefaultAsync(cancellationToken);

            if (doc == null) return;

            var response = await _client.IndexAsync(doc, i => i
                .Index(_options.CourseIndexName)
                .Id(doc.Id.ToString()), cancellationToken);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Failed to index course {CourseId} in Elasticsearch: {Details}",
                    courseId, response.DebugInformation);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to index course {CourseId} in Elasticsearch.", courseId);
        }
    }

    public async Task DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.DeleteAsync(_options.CourseIndexName, courseId.ToString(), cancellationToken);

            if (!response.IsValidResponse && response.Result != Result.NotFound)
            {
                _logger.LogWarning("Failed to delete course {CourseId} from Elasticsearch: {Details}",
                    courseId, response.DebugInformation);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to delete course {CourseId} from Elasticsearch.", courseId);
        }
    }

    public async Task ReindexCoursesAsync(CancellationToken cancellationToken)
    {
        await ReindexCoursesChunkedAsync(progress: null, log: null, batchSize: 500, cancellationToken);
    }

    public async Task<CourseIndexStats> GetStatsAsync(CancellationToken cancellationToken)
    {
        var aliasExists = await _client.Indices.ExistsAsync(_options.CourseIndexName, cancellationToken);
        if (!aliasExists.Exists)
        {
            return new CourseIndexStats(
                Exists: false,
                Enabled: _options.Enabled,
                IndexName: _options.CourseIndexName,
                AliasName: _options.CourseIndexName,
                ConcreteIndexName: null,
                DocumentCount: 0,
                SizeBytes: null,
                Health: "unknown");
        }

        var aliasResponse = await _client.Indices.GetAliasAsync(Indices.Index(_options.CourseIndexName), cancellationToken);
        var concreteIndex = aliasResponse.IsValidResponse && aliasResponse.Values?.Count > 0
            ? aliasResponse.Values.Keys.First()
            : _options.CourseIndexName;

        var countResponse = await _client.CountAsync(c => c.Indices(_options.CourseIndexName), cancellationToken);

        long? sizeBytes = null;
        var statsResponse = await _client.Indices.StatsAsync(new IndicesStatsRequest
        {
            Indices = Indices.Index(_options.CourseIndexName)
        }, cancellationToken);

        if (statsResponse.IsValidResponse && statsResponse.Indices?.Count > 0)
        {
            var primary = statsResponse.Indices.FirstOrDefault();
            if (primary.Value?.Total?.Store?.SizeInBytes > 0)
            {
                sizeBytes = primary.Value.Total.Store.SizeInBytes;
            }
        }

        var health = "unknown";
        var healthResponse = await _client.Cluster.HealthAsync(
            new Elastic.Clients.Elasticsearch.Cluster.HealthRequest(Indices.Index(_options.CourseIndexName)),
            cancellationToken);
        if (healthResponse.IsValidResponse)
        {
            health = healthResponse.Status.ToString().ToLowerInvariant();
        }

        return new CourseIndexStats(
            Exists: true,
            Enabled: _options.Enabled,
            IndexName: _options.CourseIndexName,
            AliasName: _options.CourseIndexName,
            ConcreteIndexName: concreteIndex,
            DocumentCount: countResponse.IsValidResponse ? countResponse.Count : 0,
            SizeBytes: sizeBytes,
            Health: health);
    }

    public async Task ReindexCoursesChunkedAsync(
        IProgress<ReindexProgress>? progress,
        IProgress<ReindexLogEntry>? log,
        int batchSize,
        CancellationToken cancellationToken)
    {
        if (batchSize <= 0) batchSize = 500;

        var total = await _db.Courses.AsNoTracking().CountAsync(cancellationToken);
        var batchesTotal = total == 0 ? 0 : (total + batchSize - 1) / batchSize;

        progress?.Report(new ReindexProgress(total, 0, 0, batchSize, 0, batchesTotal, 0, ReindexPhase.PreparingIndex));
        log?.Report(new ReindexLogEntry(DateTimeOffset.UtcNow, ReindexLogLevel.Info,
            $"Reindexing {total} course(s) in {batchesTotal} batch(es) of up to {batchSize}."));

        var oldIndexName = await ResolveIndexNameAsync(cancellationToken);
        var newIndexName = $"{_options.CourseIndexName}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        log?.Report(new ReindexLogEntry(DateTimeOffset.UtcNow, ReindexLogLevel.Info,
            $"Creating new index '{newIndexName}'."));
        await CreateIndexAsync(newIndexName, cancellationToken);

        if (total == 0)
        {
            log?.Report(new ReindexLogEntry(DateTimeOffset.UtcNow, ReindexLogLevel.Info,
                "No courses to index — switching alias to empty index."));
            progress?.Report(new ReindexProgress(0, 0, 0, batchSize, 0, 0, 99, ReindexPhase.SwitchingAlias));
            await SwitchAliasSafelyAsync(oldIndexName, newIndexName, log, cancellationToken);
            progress?.Report(new ReindexProgress(0, 0, 0, batchSize, 0, 0, 100, ReindexPhase.Done));
            return;
        }

        var processed = 0;
        var failed = 0;

        for (var batchIndex = 0; batchIndex < batchesTotal; batchIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var skip = batchIndex * batchSize;
            var docs = await ProjectToDocuments(_db.Courses.AsNoTracking())
                .OrderBy(c => c.Id)
                .Skip(skip)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (docs.Count == 0) break;

            log?.Report(new ReindexLogEntry(DateTimeOffset.UtcNow, ReindexLogLevel.Info,
                $"Indexing batch {batchIndex + 1}/{batchesTotal} ({docs.Count} course(s))."));

            var response = await _client.BulkAsync(b => b
                .Index(newIndexName)
                .IndexMany(docs, (op, doc) => op.Id(doc.Id.ToString())), cancellationToken);

            var batchFailed = 0;
            if (!response.IsValidResponse || response.Errors)
            {
                batchFailed = CountFailedBulkItems(response);
                log?.Report(new ReindexLogEntry(DateTimeOffset.UtcNow, ReindexLogLevel.Error,
                    $"Batch {batchIndex + 1} reported errors: {batchFailed} item(s) failed. {response.DebugInformation}"));

                if (!response.IsValidResponse)
                {
                    await _client.Indices.DeleteAsync(newIndexName, cancellationToken);
                    throw new InvalidOperationException($"Elasticsearch bulk indexing failed: {response.DebugInformation}");
                }
            }

            processed += docs.Count - batchFailed;
            failed += batchFailed;
            var percent = total == 0 ? 100 : Math.Round((double)processed / total * 100, 2);

            progress?.Report(new ReindexProgress(
                Total: total,
                Processed: processed,
                Failed: failed,
                BatchSize: batchSize,
                BatchesCompleted: batchIndex + 1,
                BatchesTotal: batchesTotal,
                Percent: percent,
                Phase: ReindexPhase.Indexing));
        }

        log?.Report(new ReindexLogEntry(DateTimeOffset.UtcNow, ReindexLogLevel.Info,
            $"Refreshing index and switching alias."));
        progress?.Report(new ReindexProgress(total, processed, failed, batchSize, batchesTotal, batchesTotal, 99, ReindexPhase.SwitchingAlias));
        await _client.Indices.RefreshAsync(newIndexName, cancellationToken);
        await SwitchAliasSafelyAsync(oldIndexName, newIndexName, log, cancellationToken);

        log?.Report(new ReindexLogEntry(DateTimeOffset.UtcNow, ReindexLogLevel.Info,
            $"Reindex done. Indexed {processed}/{total}, {failed} failure(s)."));
        progress?.Report(new ReindexProgress(total, processed, failed, batchSize, batchesTotal, batchesTotal, 100, ReindexPhase.Done));
    }

    private static int CountFailedBulkItems(BulkResponse response)
    {
        if (response.Items == null) return 0;
        return response.Items.Count(item => item.Error != null);
    }

    private async Task<string?> ResolveIndexNameAsync(CancellationToken cancellationToken)
    {
        var aliasResponse = await _client.Indices.GetAliasAsync(Indices.Index(_options.CourseIndexName), cancellationToken);
        if (!aliasResponse.IsValidResponse)
        {
            var indexExists = await _client.Indices.ExistsAsync(_options.CourseIndexName, cancellationToken);
            return indexExists.Exists ? _options.CourseIndexName : null;
        }

        return aliasResponse.Values?.Keys.FirstOrDefault();
    }

    private async Task SwitchAliasSafelyAsync(
        string? oldIndexName,
        string newIndexName,
        IProgress<ReindexLogEntry>? log,
        CancellationToken cancellationToken)
    {
        try
        {
            await SwitchAliasAsync(oldIndexName, newIndexName, log, cancellationToken);
        }
        catch
        {
            log?.Report(new ReindexLogEntry(DateTimeOffset.UtcNow, ReindexLogLevel.Error,
                $"Alias switch failed — deleting orphaned index '{newIndexName}'."));
            await _client.Indices.DeleteAsync(newIndexName, CancellationToken.None);
            throw;
        }
    }

    private async Task SwitchAliasAsync(
        string? oldIndexName,
        string newIndexName,
        IProgress<ReindexLogEntry>? log,
        CancellationToken cancellationToken)
    {
        if (oldIndexName == _options.CourseIndexName)
        {
            log?.Report(new ReindexLogEntry(DateTimeOffset.UtcNow, ReindexLogLevel.Warning,
                $"Deleting legacy index '{oldIndexName}' to free the alias name."));

            var deleteLegacy = await _client.Indices.DeleteAsync(oldIndexName, cancellationToken);
            if (!deleteLegacy.IsValidResponse)
            {
                throw new InvalidOperationException(
                    $"Failed to delete legacy index '{oldIndexName}': {deleteLegacy.DebugInformation}");
            }

            oldIndexName = null;
        }

        var actions = new List<IndexUpdateAliasesAction>();

        if (!string.IsNullOrEmpty(oldIndexName))
        {
            actions.Add(new IndexUpdateAliasesAction { Remove = new RemoveAction { Index = oldIndexName, Alias = _options.CourseIndexName } });
        }

        actions.Add(new IndexUpdateAliasesAction { Add = new AddAction { Index = newIndexName, Alias = _options.CourseIndexName } });

        var response = await _client.Indices.UpdateAliasesAsync(new UpdateAliasesRequest { Actions = actions }, cancellationToken);
        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to switch alias '{_options.CourseIndexName}': {response.DebugInformation}");
        }

        if (!string.IsNullOrEmpty(oldIndexName))
        {
            log?.Report(new ReindexLogEntry(DateTimeOffset.UtcNow, ReindexLogLevel.Info,
                $"Deleting old index '{oldIndexName}'."));
            await _client.Indices.DeleteAsync(oldIndexName, cancellationToken);
        }
    }

    private async Task CreateIndexAsync(string indexName, CancellationToken cancellationToken)
    {
        var create = await _client.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(0)
                .Analysis(a => a
                    .Analyzers(an => an
                        .Custom("folding", ca => ca
                            .Tokenizer("standard")
                            .Filter(new[] { "lowercase", "asciifolding" }))
                        .Custom("polish_folding", ca => ca
                            .Tokenizer("standard")
                            .Filter(new[] { "lowercase", "asciifolding", "polish_stem" })))))
            .Mappings(m => m
                .Properties<CourseSearchDocument>(p => p
                    .Text(n => n.Title, t => t.Analyzer("polish_folding"))
                    .Text(n => n.ShortDescription, t => t.Analyzer("polish_folding"))
                    .Text(n => n.Description, t => t.Analyzer("polish_folding"))
                    .Text(n => n.InstructorName, t => t.Analyzer("polish_folding"))
                    .Text(n => n.CategoryNames, t => t.Analyzer("polish_folding"))
                    .Text(n => n.TechnologyNames, t => t.Analyzer("polish_folding"))
                    .IntegerNumber(n => n.Level)
                    .IntegerNumber(n => n.Status)
                    .IntegerNumber(n => n.ReviewCount)
                    .ScaledFloatNumber(n => n.PriceMinorUnits, num => num.ScalingFactor(100))
                    .FloatNumber(n => n.AverageRating)
                    .Date(n => n.CreatedAt)
                    .Keyword(n => n.Language)
                    .Keyword("categoryIds")
                    .Keyword("technologyIds"))), cancellationToken);

        if (!create.Acknowledged)
        {
            throw new InvalidOperationException($"Failed to create index '{indexName}'.");
        }
    }

    private static IQueryable<CourseSearchDocument> ProjectToDocuments(IQueryable<Course> courses)
    {
        return courses
            .AsNoTracking()
            .Select(c => new CourseSearchDocument
            {
                Id = c.Id,
                Title = c.Title,
                ShortDescription = c.ShortDescription,
                Description = c.Description,
                PriceMinorUnits = (long)(c.Price * 100),
                Level = (int)c.Level,
                Status = (int)c.Status,
                Language = c.Language.ToLower(),
                InstructorName = $"{c.Instructor.FirstName} {c.Instructor.LastName}",
                ThumbnailObjectKey = c.ThumbnailObjectKey,
                CategoryIds = c.Categories.Select(x => x.Id).ToList(),
                TechnologyIds = c.Technologies.Select(x => x.Id).ToList(),
                CategoryNames = c.Categories.Select(x => x.Name).ToList(),
                TechnologyNames = c.Technologies.Select(x => x.Name).ToList(),
                ModuleCount = c.Modules.Count,
                LessonCount = c.Modules.SelectMany(m => m.Lessons).Count(),
                AverageRating = c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = c.Reviews.Count,
                CreatedAt = c.CreatedAt
            });
    }
}
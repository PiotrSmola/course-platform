using CoursePlatform.Application.Common.Interfaces;
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
        var aliasExists = await _client.Indices.ExistsAsync(_options.CourseIndexName, cancellationToken);
        if (aliasExists.Exists) return;

        var indexName = $"{_options.CourseIndexName}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        await CreateIndexAsync(indexName, cancellationToken);

        var aliasResponse = await _client.Indices.PutAliasAsync(indexName, _options.CourseIndexName, cancellationToken);
        if (!aliasResponse.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to create alias '{_options.CourseIndexName}'.");
        }
    }

    public async Task IndexCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var doc = await ProjectToDocuments(_db.Courses.Where(c => c.Id == courseId))
            .FirstOrDefaultAsync(cancellationToken);

        if (doc == null) return;

        var response = await _client.IndexAsync(doc, i => i
            .Index(_options.CourseIndexName)
            .Id(doc.Id.ToString()), cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to index course {courseId} in Elasticsearch: {response.DebugInformation}");
        }
    }

    public async Task DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var response = await _client.DeleteAsync(_options.CourseIndexName, courseId.ToString(), cancellationToken);

        if (!response.IsValidResponse && response.Result != Result.NotFound)
        {
            throw new InvalidOperationException($"Failed to delete course {courseId} from Elasticsearch: {response.DebugInformation}");
        }
    }

    public async Task ReindexCoursesAsync(CancellationToken cancellationToken)
    {
        var docs = await ProjectToDocuments(_db.Courses).ToListAsync(cancellationToken);

        var oldIndexName = await ResolveIndexNameAsync(cancellationToken);
        var newIndexName = $"{_options.CourseIndexName}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        await CreateIndexAsync(newIndexName, cancellationToken);

        if (docs.Count == 0)
        {
            await SwitchAliasAsync(oldIndexName, newIndexName, cancellationToken);
            if (!string.IsNullOrEmpty(oldIndexName))
            {
                await _client.Indices.DeleteAsync(oldIndexName, cancellationToken);
            }
            return;
        }

        var response = await _client.BulkAsync(b => b
            .Index(newIndexName)
            .IndexMany(docs, (op, doc) => op.Id(doc.Id.ToString()))
            .Refresh(Refresh.WaitFor), cancellationToken);

        if (!response.IsValidResponse || response.Errors)
        {
            await _client.Indices.DeleteAsync(newIndexName, cancellationToken);
            throw new InvalidOperationException($"Elasticsearch bulk indexing failed: {response.DebugInformation}");
        }

        await SwitchAliasAsync(oldIndexName, newIndexName, cancellationToken);

        if (!string.IsNullOrEmpty(oldIndexName))
        {
            await _client.Indices.DeleteAsync(oldIndexName, cancellationToken);
        }
    }

    private async Task<string?> ResolveIndexNameAsync(CancellationToken cancellationToken)
    {
        var aliasResponse = await _client.Indices.GetAliasAsync(Indices.Index(_options.CourseIndexName), cancellationToken);
        if (!aliasResponse.IsValidResponse)
        {
            var indexExists = await _client.Indices.ExistsAsync(_options.CourseIndexName, cancellationToken);
            return indexExists.Exists ? _options.CourseIndexName : null;
        }

        return aliasResponse.Values.Keys.FirstOrDefault();
    }

    private async Task SwitchAliasAsync(string? oldIndexName, string newIndexName, CancellationToken cancellationToken)
    {
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

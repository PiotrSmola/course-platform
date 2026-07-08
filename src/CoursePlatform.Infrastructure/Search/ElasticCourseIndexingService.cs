using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Infrastructure.Options;
using CoursePlatform.Infrastructure.Persistence;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoursePlatform.Infrastructure.Search;

internal sealed class ElasticCourseIndexingService : ICourseIndexingService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticOptions _options;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ElasticCourseIndexingService> _logger;

    public ElasticCourseIndexingService(
        ElasticsearchClient client,
        IOptions<ElasticOptions> options,
        ApplicationDbContext db,
        ILogger<ElasticCourseIndexingService> logger)
    {
        _client = client;
        _options = options.Value;
        _db = db;
        _logger = logger;
    }

    public async Task EnsureIndexAsync(CancellationToken cancellationToken)
    {
        var exists = await _client.Indices.ExistsAsync(_options.CourseIndexName, cancellationToken);
        if (exists.Exists) return;

        await CreateIndexAsync(cancellationToken);
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to index course {CourseId} in Elasticsearch.", courseId);
        }
    }

    public async Task ReindexCoursesAsync(CancellationToken cancellationToken)
    {
        var exists = await _client.Indices.ExistsAsync(_options.CourseIndexName, cancellationToken);
        if (exists.Exists)
        {
            await _client.Indices.DeleteAsync(_options.CourseIndexName, cancellationToken);
        }

        await CreateIndexAsync(cancellationToken);

        var docs = await ProjectToDocuments(_db.Courses).ToListAsync(cancellationToken);
        if (docs.Count == 0) return;

        var response = await _client.BulkAsync(b => b
            .Index(_options.CourseIndexName)
            .IndexMany(docs, (op, doc) => op.Id(doc.Id.ToString()))
            .Refresh(Refresh.WaitFor), cancellationToken);

        if (!response.IsValidResponse || response.Errors)
        {
            throw new InvalidOperationException("Elasticsearch bulk indexing failed.");
        }
    }

    private async Task CreateIndexAsync(CancellationToken cancellationToken)
    {
        var create = await _client.Indices.CreateAsync(_options.CourseIndexName, c => c
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(0)
                .Analysis(a => a
                    .Analyzers(an => an
                        .Custom("folding", ca => ca
                            .Tokenizer("standard")
                            .Filter(new[] { "lowercase", "asciifolding" })))))
            .Mappings(m => m
                .Properties<CourseSearchDocument>(p => p
                    .Text(n => n.Title, t => t.Analyzer("folding"))
                    .Text(n => n.ShortDescription, t => t.Analyzer("folding"))
                    .Text(n => n.Description, t => t.Analyzer("folding"))
                    .Text(n => n.InstructorName, t => t.Analyzer("folding"))
                    .Text(n => n.CategoryNames, t => t.Analyzer("folding"))
                    .Text(n => n.TechnologyNames, t => t.Analyzer("folding"))
                    .IntegerNumber(n => n.Level)
                    .IntegerNumber(n => n.Status)
                    .IntegerNumber(n => n.ReviewCount)
                    .FloatNumber(n => n.Price)
                    .FloatNumber(n => n.AverageRating)
                    .Date(n => n.CreatedAt)
                    .Keyword(n => n.Language)
                    .Keyword(n => n.CategoryIds)
                    .Keyword(n => n.TechnologyIds))), cancellationToken);

        if (!create.Acknowledged)
        {
            throw new InvalidOperationException($"Failed to create index '{_options.CourseIndexName}'.");
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
                Price = c.Price,
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

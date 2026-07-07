using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Infrastructure.Options;
using CoursePlatform.Infrastructure.Persistence;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoursePlatform.Infrastructure.Search;

internal sealed class ElasticCourseIndexingService : ICourseIndexingService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticOptions _options;
    private readonly ApplicationDbContext _db;

    public ElasticCourseIndexingService(
        ElasticsearchClient client,
        IOptions<ElasticOptions> options,
        ApplicationDbContext db)
    {
        _client = client;
        _options = options.Value;
        _db = db;
    }

    public async Task EnsureIndexAsync(CancellationToken cancellationToken)
    {
        var exists = await _client.Indices.ExistsAsync(_options.CourseIndexName, cancellationToken);
        if (exists.Exists) return;

        var create = await _client.Indices.CreateAsync(_options.CourseIndexName, c => c
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(0))
            .Mappings(m => m
                .Properties<CourseSearchDocument>(p => p
                    .IntegerNumber(n => n.Level)
                    .IntegerNumber(n => n.Status)
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

    public async Task ReindexCoursesAsync(CancellationToken cancellationToken)
    {
        await EnsureIndexAsync(cancellationToken);

        await _client.DeleteByQueryAsync<CourseSearchDocument>(_options.CourseIndexName, d => d
            .Query(q => q.MatchAll()), cancellationToken);

        var docs = await _db.Courses
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
            })
            .ToListAsync(cancellationToken);

        if (docs.Count == 0) return;

        foreach (var doc in docs)
        {
            var indexResponse = await _client.IndexAsync(doc, i => i
                .Index(_options.CourseIndexName)
                .Id(doc.Id.ToString()), cancellationToken);

            if (!indexResponse.IsValidResponse)
            {
                throw new InvalidOperationException("Elasticsearch indexing failed.");
            }
        }
    }
}


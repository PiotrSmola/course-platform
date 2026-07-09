using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Courses.Queries.GetCourses;
using CoursePlatform.Domain.Enums;
using CoursePlatform.Infrastructure.Options;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoursePlatform.Infrastructure.Search;

public sealed class ElasticCourseSearchService : ICourseSearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticOptions _options;
    private readonly IApplicationDbContext _db;
    private readonly EfCourseSearchService _fallback;
    private readonly ILogger<ElasticCourseSearchService> _logger;

    public ElasticCourseSearchService(
        ElasticsearchClient client,
        IOptions<ElasticOptions> options,
        IApplicationDbContext db,
        EfCourseSearchService fallback,
        ILogger<ElasticCourseSearchService> logger)
    {
        _client = client;
        _options = options.Value;
        _db = db;
        _fallback = fallback;
        _logger = logger;
    }

    public async Task<CourseSearchPage> SearchAsync(CourseSearchCriteria criteria, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            return await _fallback.SearchAsync(criteria, cancellationToken);
        }

        try
        {
            return await SearchElasticAsync(criteria, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Elasticsearch search failed, falling back to database search.");
            return await _fallback.SearchAsync(criteria, cancellationToken);
        }
    }

    private async Task<CourseSearchPage> SearchElasticAsync(CourseSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var should = BuildShouldQueries(criteria.SearchTerm!.Trim());
        var filter = BuildFilterQueries(criteria);

        var from = (criteria.PageNumber - 1) * criteria.PageSize;

        var response = await _client.SearchAsync<CourseSearchDocument>(s => s
            .Indices(_options.CourseIndexName)
            .From(from)
            .Size(criteria.PageSize)
            .Source(new SourceConfig(false))
            .Query(qry => qry.Bool(b => b
                .Should(should)
                .MinimumShouldMatch(1)
                .Filter(filter)))
            .Sort(sort => ApplySort(sort, criteria.SortBy))
            .TrackTotalHits(true), cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogWarning("Elasticsearch search failed: {Details}", response.DebugInformation);
            return await _fallback.SearchAsync(criteria, cancellationToken);
        }

        var ids = new List<Guid>();
        var matchedById = new Dictionary<Guid, IReadOnlyCollection<string>>();

        foreach (var hit in response.Hits)
        {
            if (!Guid.TryParse(hit.Id, out var id)) continue;

            ids.Add(id);
            matchedById[id] = ReadMatchedQueries(hit.MatchedQueries, criteria.SearchTerm!.Trim(), hit.Source);
        }

        var total = response.Total > 0 ? response.Total : ids.Count;

        if (ids.Count == 0)
        {
            return new CourseSearchPage(Array.Empty<CourseListRow>(), total);
        }

        var dbQuery = _db.Courses
            .AsNoTracking()
            .Where(c => ids.Contains(c.Id));

        if (!criteria.IsAdmin)
        {
            dbQuery = dbQuery.Where(c => c.Status == CourseStatus.Published);
        }
        else if (criteria.Status.HasValue)
        {
            dbQuery = dbQuery.Where(c => c.Status == criteria.Status.Value);
        }

        var rows = await dbQuery
            .Select(c => new CourseListRow(
                c.Id,
                c.Title,
                c.ShortDescription,
                c.Price,
                c.Level,
                c.Status,
                c.ThumbnailObjectKey,
                $"{c.Instructor.FirstName} {c.Instructor.LastName}",
                c.Language,
                c.Categories.Select(cat => cat.Name).ToList(),
                c.Technologies.Select(tech => tech.Name).ToList(),
                c.Modules.Count,
                c.Modules.SelectMany(m => m.Lessons).Count(),
                c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0,
                c.Reviews.Count,
                Array.Empty<string>()))
            .ToListAsync(cancellationToken);

        var rowById = rows.ToDictionary(x => x.Id, x => x);
        var ordered = ids
            .Where(rowById.ContainsKey)
            .Select(id => rowById[id] with { MatchedBy = matchedById.GetValueOrDefault(id) ?? new[] { "database" } })
            .ToList();

        return new CourseSearchPage(ordered, total);
    }

    private static Query[] BuildShouldQueries(string term)
    {
        return new Query[]
        {
            new MultiMatchQuery
            {
                Query = term,
                Fields = new[] { "title^5" },
                Type = TextQueryType.BestFields,
                Operator = Operator.Or,
                Fuzziness = "AUTO",
                QueryName = "title:fuzzy"
            },
            new MultiMatchQuery
            {
                Query = term,
                Fields = new[] { "title" },
                Type = TextQueryType.PhrasePrefix,
                QueryName = "title:prefix"
            },
            new MultiMatchQuery
            {
                Query = term,
                Fields = new[] { "shortDescription", "description" },
                Type = TextQueryType.BestFields,
                Operator = Operator.Or,
                Fuzziness = "AUTO",
                QueryName = "description:fuzzy"
            },
            new MultiMatchQuery
            {
                Query = term,
                Fields = new[] { "categoryNames^2", "technologyNames^2" },
                Type = TextQueryType.BestFields,
                Operator = Operator.Or,
                Fuzziness = "AUTO",
                QueryName = "tags:fuzzy"
            },
            new MultiMatchQuery
            {
                Query = term,
                Fields = new[] { "categoryNames", "technologyNames" },
                Type = TextQueryType.PhrasePrefix,
                QueryName = "tags:prefix"
            },
            new MultiMatchQuery
            {
                Query = term,
                Fields = new[] { "instructorName" },
                Type = TextQueryType.BestFields,
                Operator = Operator.Or,
                Fuzziness = "AUTO",
                QueryName = "instructor:fuzzy"
            },
            new MultiMatchQuery
            {
                Query = term,
                Fields = new[] { "instructorName" },
                Type = TextQueryType.PhrasePrefix,
                QueryName = "instructor:prefix"
            }
        };
    }

    private static Query[] BuildFilterQueries(CourseSearchCriteria criteria)
    {
        var filter = new List<Query>();

        if (criteria.Level.HasValue)
        {
            filter.Add(new TermQuery
            {
                Field = "level",
                Value = (int)criteria.Level.Value
            });
        }

        if (criteria.IsAdmin)
        {
            if (criteria.Status.HasValue)
            {
                filter.Add(new TermQuery
                {
                    Field = "status",
                    Value = (int)criteria.Status.Value
                });
            }
        }
        else
        {
            filter.Add(new TermQuery
            {
                Field = "status",
                Value = (int)CourseStatus.Published
            });
        }

        if (criteria.MinPrice.HasValue || criteria.MaxPrice.HasValue)
        {
            filter.Add(new NumberRangeQuery("priceMinorUnits")
            {
                Gte = criteria.MinPrice.HasValue ? (double?)ToMinorUnits(criteria.MinPrice.Value) : null,
                Lte = criteria.MaxPrice.HasValue ? (double?)ToMinorUnits(criteria.MaxPrice.Value) : null
            });
        }

        if (!string.IsNullOrWhiteSpace(criteria.Language))
        {
            filter.Add(new TermQuery
            {
                Field = "language",
                Value = criteria.Language.Trim().ToLower()
            });
        }

        if (criteria.CategoryIds != null && criteria.CategoryIds.Count > 0)
        {
            filter.Add(new TermsQuery
            {
                Field = "categoryIds",
                Terms = new TermsQueryField(criteria.CategoryIds.Select(id => FieldValue.String(id.ToString())).ToList())
            });
        }

        if (criteria.TechnologyIds != null && criteria.TechnologyIds.Count > 0)
        {
            filter.Add(new TermsQuery
            {
                Field = "technologyIds",
                Terms = new TermsQueryField(criteria.TechnologyIds.Select(id => FieldValue.String(id.ToString())).ToList())
            });
        }

        if (criteria.MinRating.HasValue)
        {
            filter.Add(new NumberRangeQuery("averageRating")
            {
                Gte = criteria.MinRating
            });
        }

        return filter.ToArray();
    }

    private static SortOptionsDescriptor<CourseSearchDocument> ApplySort(
        SortOptionsDescriptor<CourseSearchDocument> sort,
        string? sortBy)
    {
        return sortBy switch
        {
            "price-asc" => sort.Field(f => f.PriceMinorUnits, o => o.Order(SortOrder.Asc)),
            "price-desc" => sort.Field(f => f.PriceMinorUnits, o => o.Order(SortOrder.Desc)),
            "rating" => sort.Field(f => f.AverageRating, o => o.Order(SortOrder.Desc)),
            "popular" => sort.Field(f => f.ReviewCount, o => o.Order(SortOrder.Desc)),
            "newest" => sort.Field(f => f.CreatedAt, o => o.Order(SortOrder.Desc)),
            _ => sort.Score(o => o.Order(SortOrder.Desc))
        };
    }

    private static IReadOnlyCollection<string> ReadMatchedQueries(
        Union<IReadOnlyCollection<string>, IReadOnlyDictionary<string, double>>? matchedQueries,
        string term,
        CourseSearchDocument? source)
    {
        var names = matchedQueries?.Match(
            plain => plain ?? (IEnumerable<string>)Array.Empty<string>(),
            scored => scored?.Keys ?? (IEnumerable<string>)Array.Empty<string>()) ?? Array.Empty<string>();

        var result = names
            .Select(n => n.Split(':')[0])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (result.Count > 0)
        {
            return result;
        }

        return ComputeMatchedByFallback(term, source);
    }

    private static IReadOnlyCollection<string> ComputeMatchedByFallback(string term, CourseSearchDocument? source)
    {
        if (source == null) return new[] { "database" };

        var normalizedTerm = term.ToLowerInvariant();
        var matched = new List<string>();

        if (ContainsNormalized(source.Title, normalizedTerm)) matched.Add("title");
        if (ContainsNormalized(source.ShortDescription, normalizedTerm) || ContainsNormalized(source.Description, normalizedTerm)) matched.Add("description");
        if (source.CategoryNames.Any(c => ContainsNormalized(c, normalizedTerm)) || source.TechnologyNames.Any(t => ContainsNormalized(t, normalizedTerm))) matched.Add("tags");
        if (ContainsNormalized(source.InstructorName, normalizedTerm)) matched.Add("instructor");

        return matched.Count > 0 ? matched : new[] { "database" };
    }

    private static bool ContainsNormalized(string? value, string term)
    {
        return value != null && value.ToLowerInvariant().Contains(term, StringComparison.InvariantCultureIgnoreCase);
    }

    private static long ToMinorUnits(decimal value) => (long)(value * 100);
}

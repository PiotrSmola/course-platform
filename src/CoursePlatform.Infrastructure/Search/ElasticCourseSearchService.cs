using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Courses.Queries.GetCourses;
using CoursePlatform.Domain.Enums;
using CoursePlatform.Infrastructure.Options;
using CoursePlatform.Infrastructure.Persistence;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoursePlatform.Infrastructure.Search;

internal sealed class ElasticCourseSearchService : ICourseSearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticOptions _options;
    private readonly ApplicationDbContext _db;

    public ElasticCourseSearchService(
        ElasticsearchClient client,
        IOptions<ElasticOptions> options,
        ApplicationDbContext db)
    {
        _client = client;
        _options = options.Value;
        _db = db;
    }

    public async Task<CourseSearchPage> SearchAsync(CourseSearchCriteria criteria, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            // Bez frazy lepiej nie robić round-trip do ES: logika browse to nadal DB (sort/paging/filtry).
            var ef = new EfCourseSearchService(_db);
            return await ef.SearchAsync(criteria, cancellationToken);
        }

        var should = new List<Query>();
        var filter = new List<Query>();

        var q = criteria.SearchTerm.Trim();
        should.Add(new MultiMatchQuery
        {
            Query = q,
            Fields = new[] { "title^5" },
            Type = TextQueryType.BestFields,
            Operator = Operator.Or,
            Fuzziness = "AUTO",
            QueryName = "title"
        });

        should.Add(new MultiMatchQuery
        {
            Query = q,
            Fields = new[] { "title" },
            Type = TextQueryType.PhrasePrefix,
            QueryName = "title"
        });

        should.Add(new MultiMatchQuery
        {
            Query = q,
            Fields = new[] { "categoryNames^2", "technologyNames^2" },
            Type = TextQueryType.BestFields,
            Operator = Operator.Or,
            Fuzziness = "AUTO",
            QueryName = "tags"
        });

        should.Add(new MultiMatchQuery
        {
            Query = q,
            Fields = new[] { "categoryNames", "technologyNames" },
            Type = TextQueryType.PhrasePrefix,
            QueryName = "tags"
        });

        should.Add(new MultiMatchQuery
        {
            Query = q,
            Fields = new[] { "instructorName" },
            Type = TextQueryType.BestFields,
            Operator = Operator.Or,
            Fuzziness = "AUTO",
            QueryName = "instructor"
        });

        should.Add(new MultiMatchQuery
        {
            Query = q,
            Fields = new[] { "instructorName" },
            Type = TextQueryType.PhrasePrefix,
            QueryName = "instructor"
        });

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
            filter.Add(new NumberRangeQuery("price")
            {
                Gte = criteria.MinPrice.HasValue ? (double)criteria.MinPrice.Value : null,
                Lte = criteria.MaxPrice.HasValue ? (double)criteria.MaxPrice.Value : null
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

        var from = (criteria.PageNumber - 1) * criteria.PageSize;

        var response = await _client.SearchAsync<CourseSearchDocument>(s => s
            .Indices(_options.CourseIndexName)
            .From(from)
            .Size(criteria.PageSize)
            .Query(qry => qry.Bool(b => b
                .Should(should.ToArray())
                .MinimumShouldMatch(1)
                .Filter(filter.ToArray())))
            .Sort(sort => ApplySort(sort, criteria.SortBy)), cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException("Elasticsearch search failed.");
        }

        var hits = response.Hits.Where(h => h.Source != null).ToList();

        var ids = hits.Select(h => h.Source!.Id).ToList();
        var matchedByById = hits.ToDictionary(
            h => h.Source!.Id,
            h => ReadMatchedQueries(h.MatchedQueries));

        var total = response.Total > 0 ? (int)response.Total : ids.Count;

        if (ids.Count == 0)
        {
            return new CourseSearchPage(Array.Empty<CourseListRow>(), total);
        }

        var dbQuery = _db.Courses
            .AsNoTracking()
            .Where(c => ids.Contains(c.Id));

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
                matchedByById.GetValueOrDefault(c.Id) ?? Array.Empty<string>()))
            .ToListAsync(cancellationToken);

        var rowById = rows.ToDictionary(x => x.Id, x => x);
        var ordered = ids.Where(rowById.ContainsKey).Select(id => rowById[id]).ToList();

        return new CourseSearchPage(ordered, total);
    }

    private static SortOptionsDescriptor<CourseSearchDocument> ApplySort(
        SortOptionsDescriptor<CourseSearchDocument> sort,
        string? sortBy)
    {
        return sortBy switch
        {
            "price-asc" => sort.Field(f => f.Price, o => o.Order(SortOrder.Asc)),
            "price-desc" => sort.Field(f => f.Price, o => o.Order(SortOrder.Desc)),
            "rating" => sort.Field(f => f.AverageRating, o => o.Order(SortOrder.Desc)),
            "popular" => sort.Field(f => f.ReviewCount, o => o.Order(SortOrder.Desc)),
            _ => sort.Field(f => f.CreatedAt, o => o.Order(SortOrder.Desc))
        };
    }

    private static IReadOnlyCollection<string> ReadMatchedQueries(
        Union<IReadOnlyCollection<string>, IReadOnlyDictionary<string, double>>? matchedQueries)
    {
        if (matchedQueries is null) return Array.Empty<string>();

        var type = matchedQueries.GetType();

        var property = type.GetProperty("Value1", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (property?.GetValue(matchedQueries) is IReadOnlyCollection<string> names)
        {
            return names.Distinct(StringComparer.OrdinalIgnoreCase).ToList().AsReadOnly();
        }

        property = type.GetProperty("Value2", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (property?.GetValue(matchedQueries) is IReadOnlyDictionary<string, double> scored)
        {
            return scored.Keys.Distinct(StringComparer.OrdinalIgnoreCase).ToList().AsReadOnly();
        }

        return Array.Empty<string>();
    }
}


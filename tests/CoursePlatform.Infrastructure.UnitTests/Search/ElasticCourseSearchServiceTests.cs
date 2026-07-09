using Elastic.Clients.Elasticsearch;
using FluentAssertions;
using CoursePlatform.Application.Features.Courses.Queries.GetCourses;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using CoursePlatform.Infrastructure.Options;
using CoursePlatform.Infrastructure.Search;
using CoursePlatform.Infrastructure.UnitTests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CoursePlatform.Infrastructure.UnitTests.Search;

[Collection("PostgreSql")]
public class ElasticCourseSearchServiceTests : IAsyncLifetime
{
    private readonly PostgreSqlTestFixture _fixture;
    private TestDbContext _context = null!;
    private ElasticsearchClient _client = null!;
    private ElasticCourseSearchService _service = null!;
    private ElasticCourseIndexingService _indexing = null!;
    private readonly string _indexName;

    public ElasticCourseSearchServiceTests(PostgreSqlTestFixture fixture)
    {
        _fixture = fixture;
        _indexName = $"courseplatform-tests-{Guid.NewGuid():N}";
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetAsync();
        _context = _fixture.CreateContext();

        var settings = new ElasticsearchClientSettings(new Uri("http://elasticsearch:9200"));
        _client = new ElasticsearchClient(settings);

        var options = Microsoft.Extensions.Options.Options.Create(new ElasticOptions
        {
            Enabled = true,
            Uri = "http://elasticsearch:9200",
            CourseIndexName = _indexName
        });

        await DeleteTestIndicesAsync();

        var fallback = new EfCourseSearchService(_context);
        _service = new ElasticCourseSearchService(_client, options, _context, fallback, NullLogger<ElasticCourseSearchService>.Instance);
        _indexing = new ElasticCourseIndexingService(_client, options, _context, NullLogger<ElasticCourseIndexingService>.Instance);
    }

    public async Task DisposeAsync()
    {
        await DeleteTestIndicesAsync();
        await _context.DisposeAsync();
    }

    private async Task DeleteTestIndicesAsync()
    {
        var aliasResponse = await _client.Indices.GetAliasAsync(Indices.Index(_indexName));
        if (aliasResponse.IsValidResponse && aliasResponse.Values != null)
        {
            foreach (var index in aliasResponse.Values.Keys)
            {
                await _client.Indices.DeleteAsync(index);
            }
        }

        await _client.Indices.DeleteAsync(_indexName);
    }

    private static CourseSearchCriteria Criteria(
        bool isAdmin = false,
        string? searchTerm = null,
        CourseStatus? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null) =>
        new(isAdmin, searchTerm, null, status, null, minPrice, maxPrice, null, null, null, null, 1, 10);

    private async Task<ApplicationUser> SeedInstructorAsync(string firstName = "A", string lastName = "B")
    {
        var instructor = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"inst-{Guid.NewGuid():N}",
            Email = $"{Guid.NewGuid():N}@t.com",
            FirstName = firstName,
            LastName = lastName
        };
        _context.Users.Add(instructor);
        await _context.SaveChangesAsync();
        return instructor;
    }

    private async Task<Course> SeedCourseAsync(ApplicationUser instructor, string title, CourseStatus status, decimal price = 10)
    {
        var course = new Course
        {
            Title = title,
            Description = "D",
            ShortDescription = "S",
            Price = price,
            Level = CourseLevel.Beginner,
            Status = status,
            ThumbnailObjectKey = "",
            Language = "pl",
            InstructorId = instructor.Id,
            Categories = new List<Category>(),
            Technologies = new List<Technology>(),
            Modules = new List<Module>(),
            Reviews = new List<Review>()
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    [Fact]
    public async Task Search_ByTitle_ReturnsMatchingCourse()
    {
        await _indexing.EnsureIndexAsync(CancellationToken.None);

        var instructor = await SeedInstructorAsync();
        var vue = await SeedCourseAsync(instructor, "Vue 3 Fundamentals", CourseStatus.Published);
        await SeedCourseAsync(instructor, "Advanced .NET", CourseStatus.Published);

        await _indexing.IndexCourseAsync(vue.Id, CancellationToken.None);
        await _client.Indices.RefreshAsync(_indexName, CancellationToken.None);

        var page = await _service.SearchAsync(Criteria(searchTerm: "Vue"), CancellationToken.None);

        page.Items.Should().ContainSingle(c => c.Title == "Vue 3 Fundamentals");
    }

    [Fact]
    public async Task Search_NonAdminWithDraftStatus_ReturnsOnlyPublished()
    {
        await _indexing.EnsureIndexAsync(CancellationToken.None);

        var instructor = await SeedInstructorAsync();
        var published = await SeedCourseAsync(instructor, "Published", CourseStatus.Published);
        var draft = await SeedCourseAsync(instructor, "Draft", CourseStatus.Draft);

        await _indexing.IndexCourseAsync(published.Id, CancellationToken.None);
        await _indexing.IndexCourseAsync(draft.Id, CancellationToken.None);
        await _client.Indices.RefreshAsync(_indexName, CancellationToken.None);

        var page = await _service.SearchAsync(Criteria(status: CourseStatus.Draft), CancellationToken.None);

        page.Items.Should().ContainSingle(c => c.Title == "Published");
        page.Items.Should().NotContain(c => c.Title == "Draft");
    }

    [Fact]
    public async Task Search_PriceRange_FiltersByPrice()
    {
        await _indexing.EnsureIndexAsync(CancellationToken.None);

        var instructor = await SeedInstructorAsync();
        var cheap = await SeedCourseAsync(instructor, "Cheap Course", CourseStatus.Published, 50);
        var mid = await SeedCourseAsync(instructor, "Mid Course", CourseStatus.Published, 150);
        var expensive = await SeedCourseAsync(instructor, "Expensive Course", CourseStatus.Published, 250);

        await _indexing.IndexCourseAsync(cheap.Id, CancellationToken.None);
        await _indexing.IndexCourseAsync(mid.Id, CancellationToken.None);
        await _indexing.IndexCourseAsync(expensive.Id, CancellationToken.None);
        await _client.Indices.RefreshAsync(_indexName, CancellationToken.None);

        var page = await _service.SearchAsync(Criteria(searchTerm: "Course", minPrice: 100, maxPrice: 200), CancellationToken.None);

        page.Items.Should().ContainSingle(c => c.Title == "Mid Course");
    }
}

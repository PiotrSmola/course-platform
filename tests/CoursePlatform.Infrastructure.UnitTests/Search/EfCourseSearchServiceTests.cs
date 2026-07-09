using FluentAssertions;
using CoursePlatform.Application.Features.Courses.Queries.GetCourses;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using CoursePlatform.Infrastructure.Search;
using CoursePlatform.Infrastructure.UnitTests.Common;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Infrastructure.UnitTests.Search;

[CollectionDefinition("PostgreSql")]
public class PostgreSqlCollection : ICollectionFixture<PostgreSqlTestFixture> { }

[Collection("PostgreSql")]
public class EfCourseSearchServiceTests : IAsyncLifetime
{
    private readonly PostgreSqlTestFixture _fixture;
    private TestDbContext _context = null!;
    private EfCourseSearchService _service = null!;

    public EfCourseSearchServiceTests(PostgreSqlTestFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetAsync();
        _context = _fixture.CreateContext();
        _service = new EfCourseSearchService(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
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

    private async Task SeedCourseAsync(ApplicationUser instructor, string title, CourseStatus status, decimal price = 10)
    {
        _context.Courses.Add(new Course
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
        });
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Search_NonAdminWithDraftStatus_ReturnsOnlyPublished()
    {
        var instructor = await SeedInstructorAsync();
        await SeedCourseAsync(instructor, "Published", CourseStatus.Published);
        await SeedCourseAsync(instructor, "Draft", CourseStatus.Draft);

        var page = await _service.SearchAsync(Criteria(status: CourseStatus.Draft), CancellationToken.None);

        page.Items.Should().ContainSingle(c => c.Title == "Published");
        page.Items.Should().NotContain(c => c.Title == "Draft");
    }

    [Fact]
    public async Task Search_AdminWithStatusFilter_ReturnsMatchingStatus()
    {
        var instructor = await SeedInstructorAsync();
        await SeedCourseAsync(instructor, "Published", CourseStatus.Published);
        await SeedCourseAsync(instructor, "Draft", CourseStatus.Draft);

        var page = await _service.SearchAsync(Criteria(isAdmin: true, status: CourseStatus.Draft), CancellationToken.None);

        page.Items.Should().ContainSingle(c => c.Title == "Draft");
    }

    [Fact]
    public async Task Search_SearchTerm_FiltersByTitle()
    {
        var instructor = await SeedInstructorAsync();
        await SeedCourseAsync(instructor, "Vue 3 Fundamentals", CourseStatus.Published);
        await SeedCourseAsync(instructor, "Advanced .NET", CourseStatus.Published);

        var page = await _service.SearchAsync(Criteria(searchTerm: "Vue"), CancellationToken.None);

        page.Items.Should().ContainSingle(c => c.Title == "Vue 3 Fundamentals");
        page.Items.Should().NotContain(c => c.Title == "Advanced .NET");
    }

    [Fact]
    public async Task Search_SearchTerm_FiltersByInstructorName()
    {
        var kowalski = await SeedInstructorAsync("Jan", "Kowalski");
        var nowak = await SeedInstructorAsync("Anna", "Nowak");
        await SeedCourseAsync(kowalski, "Course A", CourseStatus.Published);
        await SeedCourseAsync(nowak, "Course B", CourseStatus.Published);

        var page = await _service.SearchAsync(Criteria(searchTerm: "kowalski"), CancellationToken.None);

        page.Items.Should().ContainSingle(c => c.Title == "Course A");
    }

    [Fact]
    public async Task Search_PriceRange_FiltersByPrice()
    {
        var instructor = await SeedInstructorAsync();
        await SeedCourseAsync(instructor, "Cheap", CourseStatus.Published, 50);
        await SeedCourseAsync(instructor, "Mid", CourseStatus.Published, 150);
        await SeedCourseAsync(instructor, "Expensive", CourseStatus.Published, 250);

        var page = await _service.SearchAsync(Criteria(minPrice: 100, maxPrice: 200), CancellationToken.None);

        page.Items.Should().ContainSingle(c => c.Title == "Mid");
    }

    [Fact]
    public async Task Search_MatchedBy_ReturnsDatabase()
    {
        var instructor = await SeedInstructorAsync();
        await SeedCourseAsync(instructor, "Course A", CourseStatus.Published);

        var page = await _service.SearchAsync(Criteria(searchTerm: "Course"), CancellationToken.None);

        page.Items.Should().ContainSingle();
        page.Items.First().MatchedBy.Should().ContainSingle().Which.Should().Be("database");
    }
}

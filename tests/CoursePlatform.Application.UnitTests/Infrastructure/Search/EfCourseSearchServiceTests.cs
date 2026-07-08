using FluentAssertions;
using CoursePlatform.Application.Features.Courses.Queries.GetCourses;
using CoursePlatform.Application.UnitTests.Common;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using CoursePlatform.Infrastructure.Search;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Application.UnitTests.Infrastructure.Search;

public class EfCourseSearchServiceTests
{
    private readonly TestDbContext _context;
    private readonly EfCourseSearchService _service;

    public EfCourseSearchServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
        _service = new EfCourseSearchService(_context);
    }

    private static CourseSearchCriteria Criteria(
        bool isAdmin = false,
        string? searchTerm = null,
        CourseStatus? status = null) =>
        new(isAdmin, searchTerm, null, status, null, null, null, null, null, null, null, 1, 10);

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

    private async Task SeedCourseAsync(ApplicationUser instructor, string title, CourseStatus status)
    {
        _context.Courses.Add(new Course
        {
            Title = title,
            Description = "D",
            ShortDescription = "S",
            Price = 10,
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
}

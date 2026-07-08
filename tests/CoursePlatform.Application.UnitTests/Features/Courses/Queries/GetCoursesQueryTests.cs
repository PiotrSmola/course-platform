using FluentAssertions;
using Moq;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Courses.Queries.GetCourses;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.Courses.Queries;

public class GetCoursesQueryTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();
    private readonly StubCourseSearchService _searchService = new();

    private GetCoursesQueryHandler CreateHandler() =>
        new(_currentUserServiceMock.Object, _fileStorageMock.Object, _searchService);

    [Fact]
    public async Task Handle_PassesRequestAndAdminFlagToSearchCriteria()
    {
        _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(true);

        var handler = CreateHandler();
        await handler.Handle(
            new GetCoursesQuery("vue", CourseLevel.Beginner, CourseStatus.Draft, "rating", 10, 100, "pl", null, null, 4, 2, 5),
            CancellationToken.None);

        _searchService.LastCriteria.Should().NotBeNull();
        _searchService.LastCriteria!.IsAdmin.Should().BeTrue();
        _searchService.LastCriteria.SearchTerm.Should().Be("vue");
        _searchService.LastCriteria.Level.Should().Be(CourseLevel.Beginner);
        _searchService.LastCriteria.Status.Should().Be(CourseStatus.Draft);
        _searchService.LastCriteria.SortBy.Should().Be("rating");
        _searchService.LastCriteria.MinPrice.Should().Be(10);
        _searchService.LastCriteria.MaxPrice.Should().Be(100);
        _searchService.LastCriteria.Language.Should().Be("pl");
        _searchService.LastCriteria.MinRating.Should().Be(4);
        _searchService.LastCriteria.PageNumber.Should().Be(2);
        _searchService.LastCriteria.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task Handle_MapsRowsToDtosWithMatchedBy()
    {
        var courseId = Guid.NewGuid();
        _searchService.Page = new CourseSearchPage(
            new[]
            {
                new CourseListRow(
                    courseId, "Vue 3", "Short", 49, CourseLevel.Beginner, CourseStatus.Published,
                    null, "Jan Kowalski", "pl",
                    new[] { "Frontend" }, new[] { "Vue" },
                    2, 10, 4.5, 3,
                    new[] { "title", "tags" })
            },
            42);

        var handler = CreateHandler();
        var result = await handler.Handle(
            new GetCoursesQuery(null, null, null, null, null, null, null, null, null, null),
            CancellationToken.None);

        result.TotalCount.Should().Be(42);
        var dto = result.Items.Should().ContainSingle().Subject;
        dto.Id.Should().Be(courseId);
        dto.Title.Should().Be("Vue 3");
        dto.InstructorName.Should().Be("Jan Kowalski");
        dto.MatchedBy.Should().BeEquivalentTo("title", "tags");
    }

    private sealed class StubCourseSearchService : ICourseSearchService
    {
        public CourseSearchCriteria? LastCriteria { get; private set; }
        public CourseSearchPage Page { get; set; } = new(Array.Empty<CourseListRow>(), 0);

        public Task<CourseSearchPage> SearchAsync(CourseSearchCriteria criteria, CancellationToken cancellationToken)
        {
            LastCriteria = criteria;
            return Task.FromResult(Page);
        }
    }
}

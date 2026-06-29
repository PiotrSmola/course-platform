using FluentAssertions;
using Moq;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Courses.Queries.GetCourses;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.UnitTests.Common;

namespace CoursePlatform.Application.UnitTests.Features.Courses.Queries;

public class GetCoursesQueryTests
{
    private readonly TestDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public GetCoursesQueryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_AnonymousWithDraftStatus_ReturnsOnlyPublished()
    {
        var instructor = new ApplicationUser { Id = Guid.NewGuid(), UserName = "inst", Email = "i@t.com", FirstName = "A", LastName = "B" };
        _context.Users.Add(instructor);
        await _context.SaveChangesAsync();

        var published = new Course
        {
            Title = "Published",
            Description = "D",
            ShortDescription = "S",
            Price = 10,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            ThumbnailUrl = "",
            Language = "pl",
            InstructorId = instructor.Id,
            Categories = new List<Category>(),
            Technologies = new List<Technology>(),
            Modules = new List<Module>(),
            Reviews = new List<Review>()
        };
        var draft = new Course
        {
            Title = "Draft",
            Description = "D",
            ShortDescription = "S",
            Price = 10,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Draft,
            ThumbnailUrl = "",
            Language = "pl",
            InstructorId = instructor.Id,
            Categories = new List<Category>(),
            Technologies = new List<Technology>(),
            Modules = new List<Module>(),
            Reviews = new List<Review>()
        };
        _context.Courses.Add(published);
        _context.Courses.Add(draft);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);
        _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(false);

        var handler = new GetCoursesQueryHandler(_context, _currentUserServiceMock.Object);
        var result = await handler.Handle(new GetCoursesQuery(null, null, CourseStatus.Draft, null, null, null, null, null, null, null), CancellationToken.None);

        result.Items.Should().ContainSingle(c => c.Title == "Published");
        result.Items.Should().NotContain(c => c.Title == "Draft");
    }

    [Fact]
    public async Task Handle_SearchTerm_FiltersPublishedCourses()
    {
        var instructor = new ApplicationUser { Id = Guid.NewGuid(), UserName = "inst", Email = "i@t.com", FirstName = "A", LastName = "B" };
        _context.Users.Add(instructor);
        await _context.SaveChangesAsync();

        var vueCourse = new Course
        {
            Title = "Vue 3 Fundamentals",
            Description = "Vue course",
            ShortDescription = "Learn Vue",
            Price = 10,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            ThumbnailUrl = "",
            Language = "Polski",
            InstructorId = instructor.Id,
            Categories = new List<Category>(),
            Technologies = new List<Technology>(),
            Modules = new List<Module>(),
            Reviews = new List<Review>()
        };
        var dotnetCourse = new Course
        {
            Title = "Advanced .NET",
            Description = "Dotnet course",
            ShortDescription = "Learn .NET",
            Price = 20,
            Level = CourseLevel.Advanced,
            Status = CourseStatus.Published,
            ThumbnailUrl = "",
            Language = "Polski",
            InstructorId = instructor.Id,
            Categories = new List<Category>(),
            Technologies = new List<Technology>(),
            Modules = new List<Module>(),
            Reviews = new List<Review>()
        };
        _context.Courses.Add(vueCourse);
        _context.Courses.Add(dotnetCourse);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);
        _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(false);

        var handler = new GetCoursesQueryHandler(_context, _currentUserServiceMock.Object);
        var result = await handler.Handle(
            new GetCoursesQuery("Vue", null, null, null, null, null, null, null, null, null),
            CancellationToken.None);

        result.Items.Should().ContainSingle(c => c.Title == "Vue 3 Fundamentals");
        result.Items.Should().NotContain(c => c.Title == "Advanced .NET");
    }
}

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
        var published = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Published",
            Description = "D",
            ShortDescription = "S",
            Price = 10,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            ThumbnailUrl = "",
            Language = "pl",
            InstructorId = instructor.Id,
            CreatedAt = DateTime.UtcNow,
            Categories = new List<Category>(),
            Technologies = new List<Technology>(),
            Modules = new List<Module>(),
            Reviews = new List<Review>()
        };
        var draft = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Draft",
            Description = "D",
            ShortDescription = "S",
            Price = 10,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Draft,
            ThumbnailUrl = "",
            Language = "pl",
            InstructorId = instructor.Id,
            CreatedAt = DateTime.UtcNow,
            Categories = new List<Category>(),
            Technologies = new List<Technology>(),
            Modules = new List<Module>(),
            Reviews = new List<Review>()
        };
        _context.Users.Add(instructor);
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
}

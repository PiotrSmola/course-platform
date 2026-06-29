using FluentAssertions;
using Moq;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Lessons.Queries.GetLesson;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.UnitTests.Common;

namespace CoursePlatform.Application.UnitTests.Features.Lessons.Queries;

public class GetLessonQueryTests
{
    private readonly TestDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public GetLessonQueryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_NoEnrollment_ThrowsForbidden()
    {
        var instructor = new ApplicationUser { Id = Guid.NewGuid(), UserName = "inst", Email = "i@t.com", FirstName = "A", LastName = "B" };
        _context.Users.Add(instructor);
        await _context.SaveChangesAsync();

        var course = new Course
        {
            Title = "C",
            Description = "D",
            ShortDescription = "S",
            Price = 0,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            ThumbnailUrl = "",
            Language = "pl",
            InstructorId = instructor.Id,
            Categories = new List<Category>(),
            Technologies = new List<Technology>(),
            Modules = new List<Module>
            {
                new()
                {
                    Title = "M1",
                    Order = 1,
                    Lessons = new List<Lesson>
                    {
                        new()
                        {
                            Title = "L1",
                            Description = "Desc",
                            Duration = 10,
                            Order = 1,
                            VideoUrl = "https://video.com/v.mp4"
                        }
                    }
                }
            },
            Reviews = new List<Review>()
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var handler = new GetLessonQueryHandler(_context, _currentUserServiceMock.Object);
        var lessonId = course.Modules.First().Lessons.First().Id;
        var act = async () => await handler.Handle(new GetLessonQuery(course.Id, lessonId), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}

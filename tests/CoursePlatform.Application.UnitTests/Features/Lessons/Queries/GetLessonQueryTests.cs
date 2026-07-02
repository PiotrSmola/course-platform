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

    private async Task<Course> SeedCourseAsync(Guid instructorId)
    {
        var instructor = new ApplicationUser { Id = instructorId, UserName = "inst", Email = "i@t.com", FirstName = "A", LastName = "B" };
        _context.Users.Add(instructor);

        var course = new Course
        {
            Title = "C",
            Description = "D",
            ShortDescription = "S",
            Price = 0,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            ThumbnailObjectKey = "",
            Language = "pl",
            InstructorId = instructorId,
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
                            VideoObjectKey = "videos/v.mp4"
                        }
                    }
                }
            },
            Reviews = new List<Review>()
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    [Fact]
    public async Task Handle_NoEnrollment_ThrowsForbidden()
    {
        var course = await SeedCourseAsync(Guid.NewGuid());

        _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

        var handler = new GetLessonQueryHandler(_context, _currentUserServiceMock.Object);
        var lessonId = course.Modules.First().Lessons.First().Id;
        var act = async () => await handler.Handle(new GetLessonQuery(course.Id, lessonId), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_CourseInstructor_ReturnsLessonWithoutEnrollment()
    {
        var instructorId = Guid.NewGuid();
        var course = await SeedCourseAsync(instructorId);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(instructorId);

        var handler = new GetLessonQueryHandler(_context, _currentUserServiceMock.Object);
        var lessonId = course.Modules.First().Lessons.First().Id;

        var result = await handler.Handle(new GetLessonQuery(course.Id, lessonId), CancellationToken.None);

        result.Id.Should().Be(lessonId);
    }

    [Fact]
    public async Task Handle_Admin_ReturnsLessonWithoutEnrollment()
    {
        var course = await SeedCourseAsync(Guid.NewGuid());

        _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(true);

        var handler = new GetLessonQueryHandler(_context, _currentUserServiceMock.Object);
        var lessonId = course.Modules.First().Lessons.First().Id;

        var result = await handler.Handle(new GetLessonQuery(course.Id, lessonId), CancellationToken.None);

        result.Id.Should().Be(lessonId);
    }
}

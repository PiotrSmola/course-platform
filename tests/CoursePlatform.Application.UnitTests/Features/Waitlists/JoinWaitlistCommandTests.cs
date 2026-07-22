using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Options;
using CoursePlatform.Application.Features.Courses.Commands.UpdateCourseStatus;
using CoursePlatform.Application.UnitTests.Common;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.Waitlists;

public class JoinWaitlistCommandTests
{
    private readonly TestDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    public JoinWaitlistCommandTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_DraftCourse_JoinsWaitlist()
    {
        var instructor = new ApplicationUser { Id = Guid.NewGuid(), UserName = "inst", Email = "i@t.com", FirstName = "A", LastName = "B" };
        var student = new ApplicationUser { Id = Guid.NewGuid(), UserName = "stud", Email = "s@t.com", FirstName = "C", LastName = "D" };
        _context.Users.AddRange(instructor, student);

        var course = new Course
        {
            Title = "Course",
            Description = "D",
            ShortDescription = "S",
            Price = 10,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Draft,
            ThumbnailObjectKey = "",
            Language = "pl",
            InstructorId = instructor.Id
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(student.Id);
        var handler = new CoursePlatform.Application.Features.Waitlists.Commands.JoinWaitlist.JoinWaitlistCommandHandler(
            _context, _currentUserServiceMock.Object);

        await handler.Handle(new CoursePlatform.Application.Features.Waitlists.Commands.JoinWaitlist.JoinWaitlistCommand(course.Id), CancellationToken.None);

        _context.CourseWaitlistEntries.Should().ContainSingle(e => e.UserId == student.Id && e.CourseId == course.Id);
    }
}

public class WaitlistNotificationTests
{
    [Fact]
    public async Task PublishCourse_NotifiesWaitlistAndSetsNotifiedAt()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new TestDbContext(options);

        var instructor = new ApplicationUser { Id = Guid.NewGuid(), UserName = "inst", Email = "i@t.com", FirstName = "A", LastName = "B" };
        var student = new ApplicationUser { Id = Guid.NewGuid(), UserName = "stud", Email = "s@t.com", FirstName = "C", LastName = "D" };
        context.Users.AddRange(instructor, student);

        var course = new Course
        {
            Title = "Upcoming",
            Description = "D",
            ShortDescription = "S",
            Price = 0,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Draft,
            ThumbnailObjectKey = "",
            Language = "pl",
            InstructorId = instructor.Id
        };
        context.Courses.Add(course);
        context.CourseWaitlistEntries.Add(new CourseWaitlistEntry
        {
            UserId = student.Id,
            CourseId = course.Id
        });
        await context.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(instructor.Id);
        currentUserMock.Setup(x => x.IsAdmin).Returns(false);

        var indexingMock = new Mock<ICourseIndexingService>();
        indexingMock.Setup(x => x.IndexCourseAsync(course.Id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var cacheMock = new Mock<IAppCache>();
        cacheMock.Setup(x => x.InvalidateTagAsync("courses", It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);

        var emailQueue = new FakeEmailQueue();
        var frontendOptions = Options.Create(new FrontendOptions { BaseUrl = "http://localhost:5173" });

        var handler = new UpdateOwnCourseStatusCommandHandler(
            context,
            currentUserMock.Object,
            indexingMock.Object,
            cacheMock.Object,
            emailQueue,
            frontendOptions);

        await handler.Handle(new UpdateOwnCourseStatusCommand(course.Id, CourseStatus.Published), CancellationToken.None);

        var entry = await context.CourseWaitlistEntries.FirstAsync();
        entry.NotifiedAt.Should().NotBeNull();
        emailQueue.Sent.Should().ContainSingle(m => m.To == student.Email);
    }
}

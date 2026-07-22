using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Analytics.Queries.GetInstructorAnalytics;
using CoursePlatform.Application.UnitTests.Common;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.Analytics;

public class GetInstructorAnalyticsQueryTests
{
    private readonly TestDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUser = new();

    public GetInstructorAnalyticsQueryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_ComputesDropOffPerLesson()
    {
        var instructorId = Guid.NewGuid();
        var studentA = Guid.NewGuid();
        var studentB = Guid.NewGuid();

        _context.Users.AddRange(
            new ApplicationUser { Id = instructorId, UserName = "i", Email = "i@t.com", FirstName = "I", LastName = "N" },
            new ApplicationUser { Id = studentA, UserName = "a", Email = "a@t.com", FirstName = "A", LastName = "A" },
            new ApplicationUser { Id = studentB, UserName = "b", Email = "b@t.com", FirstName = "B", LastName = "B" });

        var course = new Course
        {
            Title = "C",
            Description = "D",
            ShortDescription = "S",
            Price = 100,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            Language = "pl",
            InstructorId = instructorId
        };
        _context.Courses.Add(course);

        var module = new Module { Title = "M", Order = 1, CourseId = course.Id, Course = course };
        _context.Modules.Add(module);

        var lesson1 = new Lesson { Title = "L1", Order = 1, Duration = 5, ModuleId = module.Id, Module = module, VideoObjectKey = "" };
        var lesson2 = new Lesson { Title = "L2", Order = 2, Duration = 5, ModuleId = module.Id, Module = module, VideoObjectKey = "" };
        _context.Lessons.AddRange(lesson1, lesson2);

        _context.Enrollments.AddRange(
            new Enrollment { UserId = studentA, CourseId = course.Id, EnrolledAt = DateTime.UtcNow },
            new Enrollment { UserId = studentB, CourseId = course.Id, EnrolledAt = DateTime.UtcNow });

        _context.LessonProgresses.AddRange(
            new LessonProgress { UserId = studentA, LessonId = lesson1.Id, IsCompleted = true, CompletedAt = DateTime.UtcNow },
            new LessonProgress { UserId = studentB, LessonId = lesson1.Id, IsCompleted = true, CompletedAt = DateTime.UtcNow },
            new LessonProgress { UserId = studentA, LessonId = lesson2.Id, IsCompleted = true, CompletedAt = DateTime.UtcNow });

        _context.Payments.Add(new Payment
        {
            UserId = studentA,
            CourseId = course.Id,
            Amount = 100,
            OriginalAmount = 100,
            Currency = "pln",
            Status = PaymentStatus.Completed,
            StripeSessionId = "cs_1"
        });

        await _context.SaveChangesAsync();
        _currentUser.Setup(x => x.UserId).Returns(instructorId);

        var result = await new GetInstructorAnalyticsQueryHandler(_context, _currentUser.Object)
            .Handle(new GetInstructorAnalyticsQuery(), CancellationToken.None);

        var courseStats = result.Courses.Should().ContainSingle().Subject;
        courseStats.EnrollmentCount.Should().Be(2);
        courseStats.Revenue.Should().Be(100);
        courseStats.CompletionRate.Should().Be(50);
        courseStats.DropOff.Should().HaveCount(2);
        courseStats.DropOff[0].ReachedCount.Should().Be(2);
        courseStats.DropOff[0].CompletedCount.Should().Be(2);
        courseStats.DropOff[0].DropOffPercent.Should().Be(0);
        courseStats.DropOff[1].ReachedCount.Should().Be(2);
        courseStats.DropOff[1].CompletedCount.Should().Be(1);
        courseStats.DropOff[1].DropOffPercent.Should().Be(50);
    }
}

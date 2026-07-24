using FluentAssertions;
using Moq;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Courses.Queries.GetCourseDetails;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.UnitTests.Common;

namespace CoursePlatform.Application.UnitTests.Features.Courses.Queries;

public class GetCourseDetailsQueryTests
{
    private readonly TestDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;

    public GetCourseDetailsQueryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _fileStorageMock = new Mock<IFileStorageService>();
    }

    [Fact]
    public async Task Handle_PublishedCourseAnonymous_ReturnsDetailsWithoutVideoUrl()
    {
        var instructor = new ApplicationUser { Id = Guid.NewGuid(), UserName = "inst", Email = "i@t.com", FirstName = "A", LastName = "B" };
        _context.Users.Add(instructor);
        await _context.SaveChangesAsync();

        var course = new Course
        {
            Title = "Test",
            Description = "Desc",
            ShortDescription = "Short",
            Price = 10,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            ThumbnailObjectKey = "",
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
                            VideoObjectKey = "secret/video.mp4"
                        }
                    }
                }
            },
            Reviews = new List<Review>()
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);
        _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(false);

        var handler = new GetCourseDetailsQueryHandler(_context, _currentUserServiceMock.Object, _fileStorageMock.Object);
        var result = await handler.Handle(new GetCourseDetailsQuery(course.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Modules.Should().HaveCount(1);
        result.Modules[0].Lessons.Should().HaveCount(1);
        result.Modules[0].Lessons[0].VideoObjectKey.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DraftCourseAnonymous_ReturnsWaitlistTeaser()
    {
        var instructor = new ApplicationUser { Id = Guid.NewGuid(), UserName = "inst", Email = "i@t.com", FirstName = "A", LastName = "B" };
        _context.Users.Add(instructor);
        await _context.SaveChangesAsync();

        var course = new Course
        {
            Title = "Draft",
            Description = "Secret full description",
            ShortDescription = "Short teaser",
            Price = 10,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Draft,
            ThumbnailObjectKey = "",
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
                            Description = "Hidden description",
                            Duration = 10,
                            Order = 1,
                            VideoObjectKey = "secret/video.mp4"
                        }
                    }
                }
            },
            Reviews = new List<Review>()
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);
        _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(false);

        var handler = new GetCourseDetailsQueryHandler(_context, _currentUserServiceMock.Object, _fileStorageMock.Object);
        var result = await handler.Handle(new GetCourseDetailsQuery(course.Id), CancellationToken.None);

        result.Title.Should().Be("Draft");
        result.Description.Should().Be("Short teaser");
        result.CanAccessContent.Should().BeFalse();
        result.CanJoinWaitlist.Should().BeFalse();
        result.Modules[0].Lessons[0].Description.Should().BeNull();
        result.Modules[0].Lessons[0].VideoObjectKey.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ActiveSubscriptionUser_LoadsProgressAndAccessFlags()
    {
        var instructor = new ApplicationUser { Id = Guid.NewGuid(), UserName = "inst", Email = "i@t.com", FirstName = "A", LastName = "B" };
        var student = new ApplicationUser { Id = Guid.NewGuid(), UserName = "stud", Email = "s@t.com", FirstName = "C", LastName = "D" };
        _context.Users.AddRange(instructor, student);

        var lessonOne = new Lesson
        {
            Title = "L1",
            Description = "Desc",
            Duration = 10,
            Order = 1,
            VideoObjectKey = "video-1"
        };

        var lessonTwo = new Lesson
        {
            Title = "L2",
            Description = "Desc",
            Duration = 10,
            Order = 2,
            VideoObjectKey = "video-2"
        };

        var course = new Course
        {
            Title = "Subscribed",
            Description = "Desc",
            ShortDescription = "Short",
            Price = 10,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            ThumbnailObjectKey = "",
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
                    Lessons = new List<Lesson> { lessonOne, lessonTwo }
                }
            },
            Reviews = new List<Review>()
        };

        _context.Courses.Add(course);
        _context.Subscriptions.Add(new Subscription
        {
            UserId = student.Id,
            StripeCustomerId = "cus_123",
            StripeSubscriptionId = "sub_123",
            Status = SubscriptionStatus.Active,
            CurrentPeriodEnd = DateTime.UtcNow.AddDays(30)
        });
        _context.LessonProgresses.Add(new LessonProgress
        {
            UserId = student.Id,
            LessonId = lessonOne.Id,
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(student.Id);
        _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(false);

        var handler = new GetCourseDetailsQueryHandler(_context, _currentUserServiceMock.Object, _fileStorageMock.Object);
        var result = await handler.Handle(new GetCourseDetailsQuery(course.Id), CancellationToken.None);

        result.IsEnrolled.Should().BeFalse();
        result.CanAccessContent.Should().BeTrue();
        result.HasSubscriptionAccess.Should().BeTrue();
        result.Modules[0].Lessons[0].IsCompleted.Should().BeTrue();
        result.Modules[0].Lessons[1].IsLocked.Should().BeFalse();
        result.CanReview.Should().BeTrue();
    }
}

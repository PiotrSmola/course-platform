using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using CoursePlatform.Application.Features.Certificates.Services;
using CoursePlatform.Application.UnitTests.Common;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.Certificates;

public class CertificateIssuerTests
{
    private readonly TestDbContext _context;
    private readonly FakeNotificationService _notifications = new();
    private readonly FakeEmailQueue _emailQueue = new();
    private readonly CertificateIssuer _issuer;

    public CertificateIssuerTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
        _issuer = new CertificateIssuer(_context, _notifications, _emailQueue, NullLogger<CertificateIssuer>.Instance);
    }

    private async Task<(Guid UserId, Guid CourseId, List<Guid> LessonIds)> SeedCourseWithLessonsAsync(int lessonCount)
    {
        var instructor = new ApplicationUser { Id = Guid.NewGuid(), UserName = "inst", Email = "i@t.com", FirstName = "A", LastName = "B" };
        var student = new ApplicationUser { Id = Guid.NewGuid(), UserName = "stud", Email = "s@t.com", FirstName = "C", LastName = "D" };
        _context.Users.AddRange(instructor, student);

        var course = new Course
        {
            Title = "Course",
            Description = "D",
            ShortDescription = "S",
            Price = 0,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            ThumbnailObjectKey = "",
            Language = "pl",
            InstructorId = instructor.Id
        };
        _context.Courses.Add(course);

        var module = new Module { Title = "M1", Order = 1, CourseId = course.Id };
        _context.Modules.Add(module);

        var lessonIds = new List<Guid>();
        for (var i = 0; i < lessonCount; i++)
        {
            var lesson = new Lesson { Title = $"L{i}", Order = i + 1, Duration = 5, ModuleId = module.Id };
            _context.Lessons.Add(lesson);
            lessonIds.Add(lesson.Id);
        }

        await _context.SaveChangesAsync();
        return (student.Id, course.Id, lessonIds);
    }

    private async Task CompleteLessonsAsync(Guid userId, IEnumerable<Guid> lessonIds)
    {
        foreach (var lessonId in lessonIds)
        {
            _context.LessonProgresses.Add(new LessonProgress
            {
                UserId = userId,
                LessonId = lessonId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task IssueIfCompleted_AllLessonsDone_IssuesCertificateAndNotifies()
    {
        var (userId, courseId, lessonIds) = await SeedCourseWithLessonsAsync(3);
        await CompleteLessonsAsync(userId, lessonIds);

        var certificate = await _issuer.IssueIfCompletedAsync(userId, courseId, CancellationToken.None);

        certificate.Should().NotBeNull();
        certificate!.Number.Should().StartWith("CERT-");
        _context.Certificates.Should().ContainSingle(c => c.UserId == userId && c.CourseId == courseId);
        _notifications.UserNotifications.Should().ContainSingle(n => n.Notification.Type == "certificate-issued");
        _emailQueue.Sent.Should().ContainSingle(m => m.Subject.Contains("Certyfikat"));
    }

    [Fact]
    public async Task IssueIfCompleted_NotAllLessonsDone_DoesNothing()
    {
        var (userId, courseId, lessonIds) = await SeedCourseWithLessonsAsync(3);
        await CompleteLessonsAsync(userId, lessonIds.Take(2));

        var certificate = await _issuer.IssueIfCompletedAsync(userId, courseId, CancellationToken.None);

        certificate.Should().BeNull();
        _context.Certificates.Should().BeEmpty();
        _notifications.UserNotifications.Should().BeEmpty();
    }

    [Fact]
    public async Task IssueIfCompleted_CalledTwice_IssuesOnlyOnce()
    {
        var (userId, courseId, lessonIds) = await SeedCourseWithLessonsAsync(2);
        await CompleteLessonsAsync(userId, lessonIds);

        var first = await _issuer.IssueIfCompletedAsync(userId, courseId, CancellationToken.None);
        var second = await _issuer.IssueIfCompletedAsync(userId, courseId, CancellationToken.None);

        first.Should().NotBeNull();
        second.Should().BeNull();
        _context.Certificates.Should().HaveCount(1);
    }

    [Fact]
    public async Task IssueIfCompleted_CourseWithoutLessons_DoesNothing()
    {
        var (userId, courseId, _) = await SeedCourseWithLessonsAsync(0);

        var certificate = await _issuer.IssueIfCompletedAsync(userId, courseId, CancellationToken.None);

        certificate.Should().BeNull();
        _context.Certificates.Should().BeEmpty();
    }
}

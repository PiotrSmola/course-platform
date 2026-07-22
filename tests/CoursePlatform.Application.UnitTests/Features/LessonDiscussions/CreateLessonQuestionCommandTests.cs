using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.LessonDiscussions.Commands.CreateLessonQuestion;
using CoursePlatform.Application.UnitTests.Common;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.LessonDiscussions;

public class CreateLessonQuestionCommandTests
{
    private readonly TestDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IHtmlSanitizer> _sanitizer = new();

    public CreateLessonQuestionCommandTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
        _sanitizer.Setup(s => s.Sanitize(It.IsAny<string>())).Returns<string>(x => x);
    }

    private async Task<(Course course, Lesson lesson, ApplicationUser student)> SeedAsync(bool enroll)
    {
        var instructor = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "inst",
            Email = "i@t.com",
            FirstName = "Inst",
            LastName = "Ructor"
        };
        var student = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "stud",
            Email = "s@t.com",
            FirstName = "Stu",
            LastName = "Dent"
        };
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

        var module = new Module
        {
            Title = "M1",
            Order = 1,
            CourseId = course.Id,
            Course = course
        };
        _context.Modules.Add(module);

        var lesson = new Lesson
        {
            Title = "L1",
            Duration = 10,
            Order = 1,
            ModuleId = module.Id,
            Module = module,
            VideoObjectKey = ""
        };
        _context.Lessons.Add(lesson);

        if (enroll)
        {
            _context.Enrollments.Add(new Enrollment
            {
                UserId = student.Id,
                CourseId = course.Id,
                EnrolledAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        _currentUser.Setup(x => x.UserId).Returns(student.Id);
        return (course, lesson, student);
    }

    [Fact]
    public async Task Handle_EnrolledStudent_CreatesQuestion()
    {
        var (course, lesson, student) = await SeedAsync(enroll: true);
        var handler = new CreateLessonQuestionCommandHandler(_context, _currentUser.Object, _sanitizer.Object);

        var id = await handler.Handle(
            new CreateLessonQuestionCommand(course.Id, lesson.Id, "Why?"),
            CancellationToken.None);

        var question = await _context.LessonQuestions.SingleAsync();
        question.Id.Should().Be(id);
        question.AuthorId.Should().Be(student.Id);
        question.Body.Should().Be("Why?");
    }

    [Fact]
    public async Task Handle_NotEnrolled_ThrowsForbidden()
    {
        var (course, lesson, _) = await SeedAsync(enroll: false);
        var handler = new CreateLessonQuestionCommandHandler(_context, _currentUser.Object, _sanitizer.Object);

        var act = () => handler.Handle(
            new CreateLessonQuestionCommand(course.Id, lesson.Id, "Why?"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}

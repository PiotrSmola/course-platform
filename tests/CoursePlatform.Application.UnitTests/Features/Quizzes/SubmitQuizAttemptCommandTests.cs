using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Quizzes;
using CoursePlatform.Application.Features.Quizzes.Commands.SubmitQuizAttempt;
using CoursePlatform.Application.UnitTests.Common;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.Quizzes;

public class SubmitQuizAttemptCommandTests
{
    private readonly TestDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUser = new();

    public SubmitQuizAttemptCommandTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
    }

    private async Task<(Course course, Lesson lesson, Quiz quiz, ApplicationUser student)> SeedAsync()
    {
        var instructor = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "inst",
            Email = "i@t.com",
            FirstName = "A",
            LastName = "B"
        };
        var student = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "stud",
            Email = "s@t.com",
            FirstName = "C",
            LastName = "D"
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

        var module = new Module { Title = "M", Order = 1, CourseId = course.Id, Course = course };
        _context.Modules.Add(module);

        var lesson = new Lesson
        {
            Title = "L",
            Duration = 5,
            Order = 1,
            ModuleId = module.Id,
            Module = module,
            VideoObjectKey = ""
        };
        _context.Lessons.Add(lesson);

        var correct = new QuizOption { Text = "Yes", IsCorrect = true, Order = 0 };
        var wrong = new QuizOption { Text = "No", IsCorrect = false, Order = 1 };
        var question = new QuizQuestion
        {
            Prompt = "2+2=4?",
            Order = 0,
            Options = { correct, wrong }
        };
        var quiz = new Quiz
        {
            LessonId = lesson.Id,
            Lesson = lesson,
            Title = "Quiz",
            PassThresholdPercent = 70,
            Questions = { question }
        };
        _context.Quizzes.Add(quiz);

        _context.Enrollments.Add(new Enrollment
        {
            UserId = student.Id,
            CourseId = course.Id,
            EnrolledAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        _currentUser.Setup(x => x.UserId).Returns(student.Id);
        return (course, lesson, quiz, student);
    }

    [Fact]
    public async Task Handle_CorrectAnswer_Passes()
    {
        var (course, lesson, quiz, _) = await SeedAsync();
        var question = quiz.Questions.Single();
        var correct = question.Options.Single(o => o.IsCorrect);
        var handler = new SubmitQuizAttemptCommandHandler(_context, _currentUser.Object);

        var result = await handler.Handle(
            new SubmitQuizAttemptCommand(
                course.Id,
                lesson.Id,
                new[] { new QuizAttemptAnswerInputDto(question.Id, correct.Id) }),
            CancellationToken.None);

        result.ScorePercent.Should().Be(100);
        result.Passed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WrongAnswer_Fails()
    {
        var (course, lesson, quiz, _) = await SeedAsync();
        var question = quiz.Questions.Single();
        var wrong = question.Options.Single(o => !o.IsCorrect);
        var handler = new SubmitQuizAttemptCommandHandler(_context, _currentUser.Object);

        var result = await handler.Handle(
            new SubmitQuizAttemptCommand(
                course.Id,
                lesson.Id,
                new[] { new QuizAttemptAnswerInputDto(question.Id, wrong.Id) }),
            CancellationToken.None);

        result.ScorePercent.Should().Be(0);
        result.Passed.Should().BeFalse();
    }
}

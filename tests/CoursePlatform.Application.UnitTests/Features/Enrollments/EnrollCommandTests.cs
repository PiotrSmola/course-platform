using FluentAssertions;
using FluentValidation;
using Moq;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Enrollments.Commands.Enroll;
using CoursePlatform.Application.UnitTests.Common;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.Enrollments;

public class EnrollCommandTests
{
    private readonly TestDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    public EnrollCommandTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
    }

    private async Task<Course> SeedCourseAsync(decimal price)
    {
        var instructor = new ApplicationUser { Id = Guid.NewGuid(), UserName = "inst", Email = "i@t.com", FirstName = "A", LastName = "B" };
        var student = new ApplicationUser { Id = Guid.NewGuid(), UserName = "stud", Email = "s@t.com", FirstName = "C", LastName = "D" };
        _context.Users.AddRange(instructor, student);

        var course = new Course
        {
            Title = "Course",
            Description = "D",
            ShortDescription = "S",
            Price = price,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            ThumbnailObjectKey = "",
            Language = "pl",
            InstructorId = instructor.Id
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(student.Id);
        return course;
    }

    [Fact]
    public async Task Handle_FreeCourse_Enrolls()
    {
        var course = await SeedCourseAsync(price: 0);
        var handler = new EnrollCommandHandler(_context, _currentUserServiceMock.Object);

        await handler.Handle(new EnrollCommand(course.Id), CancellationToken.None);

        _context.Enrollments.Should().ContainSingle(e => e.CourseId == course.Id);
    }

    [Fact]
    public async Task Handle_PaidCourse_ThrowsValidation()
    {
        var course = await SeedCourseAsync(price: 49);
        var handler = new EnrollCommandHandler(_context, _currentUserServiceMock.Object);

        var act = () => handler.Handle(new EnrollCommand(course.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        _context.Enrollments.Should().BeEmpty();
    }
}

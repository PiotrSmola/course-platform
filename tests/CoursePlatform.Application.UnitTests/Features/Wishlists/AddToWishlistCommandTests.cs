using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Wishlists.Commands.AddToWishlist;
using CoursePlatform.Application.UnitTests.Common;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.Wishlists;

public class AddToWishlistCommandTests
{
    private readonly TestDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    public AddToWishlistCommandTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_PublishedCourse_AddsToWishlist()
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
            Status = CourseStatus.Published,
            ThumbnailObjectKey = "",
            Language = "pl",
            InstructorId = instructor.Id
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(student.Id);
        var handler = new AddToWishlistCommandHandler(_context, _currentUserServiceMock.Object);

        await handler.Handle(new AddToWishlistCommand(course.Id), CancellationToken.None);

        _context.WishlistItems.Should().ContainSingle(w => w.UserId == student.Id && w.CourseId == course.Id);
    }

    [Fact]
    public async Task Handle_DraftCourse_ThrowsForbidden()
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
        var handler = new AddToWishlistCommandHandler(_context, _currentUserServiceMock.Object);

        var act = () => handler.Handle(new AddToWishlistCommand(course.Id), CancellationToken.None);

        await act.Should().ThrowAsync<CoursePlatform.Application.Common.Exceptions.ForbiddenAccessException>();
        _context.WishlistItems.Should().BeEmpty();
    }
}

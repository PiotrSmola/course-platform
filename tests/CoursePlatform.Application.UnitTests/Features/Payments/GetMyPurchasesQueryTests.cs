using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Payments.Queries.GetMyPurchases;
using CoursePlatform.Application.UnitTests.Common;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.Payments;

public class GetMyPurchasesQueryTests
{
    private readonly TestDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    public GetMyPurchasesQueryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _context = new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsOwnCompletedPurchasesNewestFirst()
    {
        var instructor = new ApplicationUser { Id = Guid.NewGuid(), UserName = "inst", Email = "i@t.com", FirstName = "A", LastName = "B" };
        var me = new ApplicationUser { Id = Guid.NewGuid(), UserName = "me", Email = "m@t.com", FirstName = "C", LastName = "D" };
        var other = new ApplicationUser { Id = Guid.NewGuid(), UserName = "other", Email = "o@t.com", FirstName = "E", LastName = "F" };
        _context.Users.AddRange(instructor, me, other);

        var courseA = NewCourse(instructor.Id, "Course A");
        var courseB = NewCourse(instructor.Id, "Course B");
        var courseC = NewCourse(instructor.Id, "Course C");
        _context.Courses.AddRange(courseA, courseB, courseC);

        _context.Payments.AddRange(
            new Payment
            {
                UserId = me.Id, CourseId = courseA.Id, Amount = 49, Currency = "pln",
                Status = PaymentStatus.Completed, StripeSessionId = "cs_1",
                CompletedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Payment
            {
                UserId = me.Id, CourseId = courseB.Id, Amount = 99, Currency = "pln",
                Status = PaymentStatus.Completed, StripeSessionId = "cs_2",
                CompletedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Payment
            {
                UserId = me.Id, CourseId = courseC.Id, Amount = 149, Currency = "pln",
                Status = PaymentStatus.Pending, StripeSessionId = "cs_3"
            },
            new Payment
            {
                UserId = other.Id, CourseId = courseA.Id, Amount = 49, Currency = "pln",
                Status = PaymentStatus.Completed, StripeSessionId = "cs_4",
                CompletedAt = DateTime.UtcNow
            });
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(me.Id);
        var handler = new GetMyPurchasesQueryHandler(_context, _currentUserServiceMock.Object);

        var result = await handler.Handle(new GetMyPurchasesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].CourseTitle.Should().Be("Course B");
        result[1].CourseTitle.Should().Be("Course A");
        result.Should().NotContain(p => p.CourseTitle == "Course C");
    }

    private static Course NewCourse(Guid instructorId, string title) => new()
    {
        Title = title,
        Description = "D",
        ShortDescription = "S",
        Price = 49,
        Level = CourseLevel.Beginner,
        Status = CourseStatus.Published,
        ThumbnailObjectKey = "",
        Language = "pl",
        InstructorId = instructorId
    };
}

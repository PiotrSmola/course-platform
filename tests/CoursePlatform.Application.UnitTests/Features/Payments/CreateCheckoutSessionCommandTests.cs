using FluentAssertions;
using FluentValidation;
using Moq;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Payments.Commands.CreateCheckoutSession;
using CoursePlatform.Application.UnitTests.Common;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.Payments;

public class CreateCheckoutSessionCommandTests
{
    private readonly TestDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly FakePaymentGateway _gateway = new();

    public CreateCheckoutSessionCommandTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
    }

    private CreateCheckoutSessionCommandHandler CreateHandler() =>
        new(_context, _currentUserServiceMock.Object, _gateway);

    private async Task<(ApplicationUser Student, Course Course)> SeedAsync(decimal price, CourseStatus status = CourseStatus.Published)
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
            Status = status,
            ThumbnailObjectKey = "",
            Language = "pl",
            InstructorId = instructor.Id
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(student.Id);
        return (student, course);
    }

    [Fact]
    public async Task Handle_PaidCourse_CreatesPendingPaymentAndReturnsRedirect()
    {
        var (student, course) = await SeedAsync(price: 49);

        var result = await CreateHandler().Handle(new CreateCheckoutSessionCommand(course.Id), CancellationToken.None);

        result.Enrolled.Should().BeFalse();
        result.RedirectUrl.Should().Contain("cs_test_123");

        var payment = _context.Payments.Single();
        payment.UserId.Should().Be(student.Id);
        payment.CourseId.Should().Be(course.Id);
        payment.Amount.Should().Be(49);
        payment.Currency.Should().Be("pln");
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.StripeSessionId.Should().Be("cs_test_123");

        _context.Enrollments.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_FreeCourse_EnrollsDirectlyWithoutPayment()
    {
        var (student, course) = await SeedAsync(price: 0);

        var result = await CreateHandler().Handle(new CreateCheckoutSessionCommand(course.Id), CancellationToken.None);

        result.Enrolled.Should().BeTrue();
        result.RedirectUrl.Should().BeNull();
        _context.Payments.Should().BeEmpty();
        _context.Enrollments.Should().ContainSingle(e => e.UserId == student.Id && e.CourseId == course.Id);
    }

    [Fact]
    public async Task Handle_AlreadyEnrolled_ThrowsValidation()
    {
        var (student, course) = await SeedAsync(price: 49);
        _context.Enrollments.Add(new Enrollment { UserId = student.Id, CourseId = course.Id });
        await _context.SaveChangesAsync();

        var act = () => CreateHandler().Handle(new CreateCheckoutSessionCommand(course.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        _context.Payments.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_GatewayNotConfigured_ThrowsValidation()
    {
        var (_, course) = await SeedAsync(price: 49);
        _gateway.IsConfigured = false;

        var act = () => CreateHandler().Handle(new CreateCheckoutSessionCommand(course.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}

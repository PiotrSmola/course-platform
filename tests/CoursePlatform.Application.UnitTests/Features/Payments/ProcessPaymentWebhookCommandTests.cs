using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Payments.Commands.ProcessPaymentWebhook;
using CoursePlatform.Application.UnitTests.Common;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.Payments;

public class ProcessPaymentWebhookCommandTests
{
    private readonly TestDbContext _context;
    private readonly FakePaymentGateway _gateway = new();

    public ProcessPaymentWebhookCommandTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
    }

    private ProcessPaymentWebhookCommandHandler CreateHandler() =>
        new(_context, _gateway, NullLogger<ProcessPaymentWebhookCommandHandler>.Instance);

    private async Task<Payment> SeedPendingPaymentAsync(decimal amount = 49)
    {
        var instructor = new ApplicationUser { Id = Guid.NewGuid(), UserName = "inst", Email = "i@t.com", FirstName = "A", LastName = "B" };
        var student = new ApplicationUser { Id = Guid.NewGuid(), UserName = "stud", Email = "s@t.com", FirstName = "C", LastName = "D" };
        _context.Users.AddRange(instructor, student);

        var course = new Course
        {
            Title = "Course",
            Description = "D",
            ShortDescription = "S",
            Price = amount,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            ThumbnailObjectKey = "",
            Language = "pl",
            InstructorId = instructor.Id
        };
        _context.Courses.Add(course);

        var payment = new Payment
        {
            UserId = student.Id,
            CourseId = course.Id,
            Amount = amount,
            Currency = "pln",
            Status = PaymentStatus.Pending,
            StripeSessionId = "cs_test_123"
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    private static ProcessPaymentWebhookCommand Command() => new("{}", "sig");

    [Fact]
    public async Task Handle_CheckoutCompleted_CompletesPaymentAndEnrolls()
    {
        var payment = await SeedPendingPaymentAsync(amount: 49);
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutCompleted, "cs_test_123", 4900);

        await CreateHandler().Handle(Command(), CancellationToken.None);

        var updated = _context.Payments.Single();
        updated.Status.Should().Be(PaymentStatus.Completed);
        updated.CompletedAt.Should().NotBeNull();
        _context.Enrollments.Should().ContainSingle(e => e.UserId == payment.UserId && e.CourseId == payment.CourseId);
    }

    [Fact]
    public async Task Handle_CheckoutCompletedTwice_IsIdempotent()
    {
        await SeedPendingPaymentAsync(amount: 49);
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutCompleted, "cs_test_123", 4900);

        await CreateHandler().Handle(Command(), CancellationToken.None);
        await CreateHandler().Handle(Command(), CancellationToken.None);

        _context.Payments.Single().Status.Should().Be(PaymentStatus.Completed);
        _context.Enrollments.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_AmountMismatch_MarksFailedWithoutEnrollment()
    {
        await SeedPendingPaymentAsync(amount: 49);
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutCompleted, "cs_test_123", 100);

        await CreateHandler().Handle(Command(), CancellationToken.None);

        _context.Payments.Single().Status.Should().Be(PaymentStatus.Failed);
        _context.Enrollments.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CheckoutExpired_MarksPendingPaymentExpired()
    {
        await SeedPendingPaymentAsync();
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutExpired, "cs_test_123", null);

        await CreateHandler().Handle(Command(), CancellationToken.None);

        _context.Payments.Single().Status.Should().Be(PaymentStatus.Expired);
        _context.Enrollments.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ExpiredAfterCompleted_DoesNotDowngradeStatus()
    {
        await SeedPendingPaymentAsync(amount: 49);
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutCompleted, "cs_test_123", 4900);
        await CreateHandler().Handle(Command(), CancellationToken.None);

        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutExpired, "cs_test_123", null);
        await CreateHandler().Handle(Command(), CancellationToken.None);

        _context.Payments.Single().Status.Should().Be(PaymentStatus.Completed);
        _context.Enrollments.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_UnknownSession_DoesNothing()
    {
        await SeedPendingPaymentAsync();
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutCompleted, "cs_unknown", 4900);

        await CreateHandler().Handle(Command(), CancellationToken.None);

        _context.Payments.Single().Status.Should().Be(PaymentStatus.Pending);
        _context.Enrollments.Should().BeEmpty();
    }
}

using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
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
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutCompleted, "evt_1", "cs_test_123", 4900, "pln", null);

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
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutCompleted, "evt_1", "cs_test_123", 4900, "pln", null);

        await CreateHandler().Handle(Command(), CancellationToken.None);
        await CreateHandler().Handle(Command(), CancellationToken.None);

        _context.Payments.Single().Status.Should().Be(PaymentStatus.Completed);
        _context.Enrollments.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_DuplicateStripeEventId_DoesNothing()
    {
        await SeedPendingPaymentAsync(amount: 49);
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutCompleted, "evt_dup", "cs_test_123", 4900, "pln", null);

        await CreateHandler().Handle(Command(), CancellationToken.None);
        await CreateHandler().Handle(Command(), CancellationToken.None);

        _context.ProcessedStripeEvents.Should().HaveCount(1);
        _context.Payments.Single().Status.Should().Be(PaymentStatus.Completed);
        _context.Enrollments.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_AmountMismatch_MarksFailedWithoutEnrollment()
    {
        await SeedPendingPaymentAsync(amount: 49);
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutCompleted, "evt_2", "cs_test_123", 100, "pln", null);

        await CreateHandler().Handle(Command(), CancellationToken.None);

        _context.Payments.Single().Status.Should().Be(PaymentStatus.Failed);
        _context.Enrollments.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CurrencyMismatch_MarksFailedWithoutEnrollment()
    {
        await SeedPendingPaymentAsync(amount: 49);
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutCompleted, "evt_3", "cs_test_123", 4900, "usd", null);

        await CreateHandler().Handle(Command(), CancellationToken.None);

        _context.Payments.Single().Status.Should().Be(PaymentStatus.Failed);
        _context.Enrollments.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CheckoutExpired_MarksPendingPaymentExpired()
    {
        await SeedPendingPaymentAsync();
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutExpired, "evt_4", "cs_test_123", null, null, null);

        await CreateHandler().Handle(Command(), CancellationToken.None);

        _context.Payments.Single().Status.Should().Be(PaymentStatus.Expired);
        _context.Enrollments.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ExpiredAfterCompleted_DoesNotDowngradeStatus()
    {
        await SeedPendingPaymentAsync(amount: 49);
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutCompleted, "evt_5", "cs_test_123", 4900, "pln", null);
        await CreateHandler().Handle(Command(), CancellationToken.None);

        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutExpired, "evt_6", "cs_test_123", null, null, null);
        await CreateHandler().Handle(Command(), CancellationToken.None);

        _context.Payments.Single().Status.Should().Be(PaymentStatus.Completed);
        _context.Enrollments.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_PaymentRefunded_RemovesEnrollment()
    {
        var payment = await SeedPendingPaymentAsync(amount: 49);
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutCompleted, "evt_7", "cs_test_123", 4900, "pln", null);
        await CreateHandler().Handle(Command(), CancellationToken.None);

        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.PaymentRefunded, "evt_8", null, 4900, "pln", payment.Id.ToString());
        await CreateHandler().Handle(Command(), CancellationToken.None);

        _context.Payments.Single().Status.Should().Be(PaymentStatus.Refunded);
        _context.Enrollments.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Chargeback_RemovesEnrollment()
    {
        var payment = await SeedPendingPaymentAsync(amount: 49);
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutCompleted, "evt_9", "cs_test_123", 4900, "pln", null);
        await CreateHandler().Handle(Command(), CancellationToken.None);

        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.Chargeback, "evt_10", null, 4900, "pln", payment.Id.ToString());
        await CreateHandler().Handle(Command(), CancellationToken.None);

        _context.Payments.Single().Status.Should().Be(PaymentStatus.Chargeback);
        _context.Enrollments.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_UnknownSession_DoesNothing()
    {
        await SeedPendingPaymentAsync();
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutCompleted, "evt_11", "cs_unknown", 4900, "pln", null);

        await CreateHandler().Handle(Command(), CancellationToken.None);

        _context.Payments.Single().Status.Should().Be(PaymentStatus.Pending);
        _context.Enrollments.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PartialRefund_KeepsEnrollmentAndCompletedStatus()
    {
        var payment = await SeedPendingPaymentAsync(amount: 49);
        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutCompleted, "evt_12", "cs_test_123", 4900, "pln", null);
        await CreateHandler().Handle(Command(), CancellationToken.None);

        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.PaymentRefunded, "evt_13", null, 1000, "pln", payment.Id.ToString());
        await CreateHandler().Handle(Command(), CancellationToken.None);

        _context.Payments.Single().Status.Should().Be(PaymentStatus.Completed);
        _context.Enrollments.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_UnknownSession_IsRetriable()
    {
        await SeedPendingPaymentAsync(amount: 49);
        var stored = _context.Payments.Single();
        stored.StripeSessionId = null;
        await _context.SaveChangesAsync();

        _gateway.EventToReturn = new PaymentGatewayEvent(PaymentGatewayEventType.CheckoutCompleted, "evt_14", "cs_test_123", 4900, "pln", null);
        await CreateHandler().Handle(Command(), CancellationToken.None);
        _context.Enrollments.Should().BeEmpty();
        _context.ChangeTracker.Clear();

        var payment = _context.Payments.Single();
        payment.StripeSessionId = "cs_test_123";
        await _context.SaveChangesAsync();

        await CreateHandler().Handle(Command(), CancellationToken.None);

        _context.Payments.Single().Status.Should().Be(PaymentStatus.Completed);
        _context.Enrollments.Should().HaveCount(1);
    }
}

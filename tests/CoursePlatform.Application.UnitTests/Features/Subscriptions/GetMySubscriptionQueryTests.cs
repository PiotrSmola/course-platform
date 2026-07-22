using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Subscriptions.Queries.GetMySubscription;
using CoursePlatform.Application.UnitTests.Common;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.Subscriptions;

public class GetMySubscriptionQueryTests
{
    private readonly TestDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    public GetMySubscriptionQueryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsSubscriptionWithLatestInvoice()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "student",
            Email = "student@test.com",
            FirstName = "A",
            LastName = "B"
        };

        var subscription = new Subscription
        {
            UserId = user.Id,
            StripeCustomerId = "cus_123",
            StripeSubscriptionId = "sub_123",
            Status = SubscriptionStatus.Active,
            CurrentPeriodEnd = DateTime.UtcNow.AddDays(15),
            Invoices =
            {
                new SubscriptionInvoice
                {
                    Amount = 399,
                    Currency = "PLN",
                    PaidAt = DateTime.UtcNow.AddDays(-30),
                    StripeInvoiceId = "in_old"
                },
                new SubscriptionInvoice
                {
                    Amount = 399,
                    Currency = "PLN",
                    PaidAt = DateTime.UtcNow.AddDays(-1),
                    StripeInvoiceId = "in_new"
                }
            }
        };

        _context.Users.Add(user);
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);
        var handler = new GetMySubscriptionQueryHandler(_context, _currentUserServiceMock.Object);

        var result = await handler.Handle(new GetMySubscriptionQuery(), CancellationToken.None);

        result.HasSubscription.Should().BeTrue();
        result.HasActiveAccess.Should().BeTrue();
        result.Status.Should().Be(SubscriptionStatus.Active);
        result.CanManageInPortal.Should().BeTrue();
        result.LatestInvoiceAmount.Should().Be(399);
        result.LatestInvoiceCurrency.Should().Be("PLN");
    }
}

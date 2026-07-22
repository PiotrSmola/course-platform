using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Options;
using CoursePlatform.Application.Features.Subscriptions.Commands.CreateSubscriptionCheckout;
using CoursePlatform.Application.UnitTests.Common;
using CoursePlatform.Application.UnitTests.Features.Payments;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.Subscriptions;

public class CreateSubscriptionCheckoutCommandTests
{
    private readonly TestDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly FakePaymentGateway _gateway = new();

    public CreateSubscriptionCheckoutCommandTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _context = new TestDbContext(options);
    }

    private CreateSubscriptionCheckoutCommandHandler CreateHandler() =>
        new(
            _context,
            _currentUserServiceMock.Object,
            _gateway,
            Options.Create(new SubscriptionOptions()),
            NullLogger<CreateSubscriptionCheckoutCommandHandler>.Instance);

    private async Task<ApplicationUser> SeedUserAsync()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "student",
            Email = "student@test.com",
            FirstName = "A",
            LastName = "B"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);
        return user;
    }

    [Fact]
    public async Task Handle_WhenUserHasNoActiveSubscription_ReturnsRedirectUrl()
    {
        await SeedUserAsync();

        var result = await CreateHandler().Handle(new CreateSubscriptionCheckoutCommand(), CancellationToken.None);

        result.RedirectUrl.Should().Contain("cs_test_123");
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyHasActiveSubscription_ThrowsValidation()
    {
        var user = await SeedUserAsync();
        _context.Subscriptions.Add(new Subscription
        {
            UserId = user.Id,
            StripeCustomerId = "cus_123",
            StripeSubscriptionId = "sub_123",
            Status = SubscriptionStatus.Active,
            CurrentPeriodEnd = DateTime.UtcNow.AddDays(20)
        });
        await _context.SaveChangesAsync();

        var act = () => CreateHandler().Handle(new CreateSubscriptionCheckoutCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}

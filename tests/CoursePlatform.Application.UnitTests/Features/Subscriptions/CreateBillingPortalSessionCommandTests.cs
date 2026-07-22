using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Options;
using CoursePlatform.Application.Features.Subscriptions.Commands.CreateBillingPortalSession;
using CoursePlatform.Application.UnitTests.Common;
using CoursePlatform.Application.UnitTests.Features.Payments;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.Subscriptions;

public class CreateBillingPortalSessionCommandTests
{
    private readonly TestDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly FakePaymentGateway _gateway = new();

    public CreateBillingPortalSessionCommandTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
    }

    private CreateBillingPortalSessionCommandHandler CreateHandler() =>
        new(
            _context,
            _currentUserServiceMock.Object,
            _gateway,
            Options.Create(new FrontendOptions { BaseUrl = "http://localhost:5173" }));

    [Fact]
    public async Task Handle_ReturnsBillingPortalRedirectUrl()
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
        _context.Subscriptions.Add(new Subscription
        {
            UserId = user.Id,
            StripeCustomerId = "cus_123",
            StripeSubscriptionId = "sub_123",
            Status = SubscriptionStatus.Active,
            CurrentPeriodEnd = DateTime.UtcNow.AddDays(10)
        });
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);

        var result = await CreateHandler().Handle(new CreateBillingPortalSessionCommand(), CancellationToken.None);

        result.RedirectUrl.Should().Be(_gateway.BillingPortalUrlToReturn);
    }

    [Fact]
    public async Task Handle_WithoutBillingProfile_ThrowsValidation()
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

        var act = () => CreateHandler().Handle(new CreateBillingPortalSessionCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}

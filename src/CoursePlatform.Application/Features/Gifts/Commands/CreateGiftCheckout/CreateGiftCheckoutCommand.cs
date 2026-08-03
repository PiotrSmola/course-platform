using System.Security.Cryptography;
using System.Text;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoursePlatform.Application.Features.Gifts.Commands.CreateGiftCheckout;

public record GiftCheckoutSessionDto(string RedirectUrl);

public record CreateGiftCheckoutCommand(Guid CourseId, string RecipientEmail) : IRequest<GiftCheckoutSessionDto>;

public class CreateGiftCheckoutCommandValidator : AbstractValidator<CreateGiftCheckoutCommand>
{
    public CreateGiftCheckoutCommandValidator()
    {
        RuleFor(command => command.CourseId).NotEmpty();
        RuleFor(command => command.RecipientEmail).NotEmpty().EmailAddress().MaximumLength(320);
    }
}

public class CreateGiftCheckoutCommandHandler
    : IRequestHandler<CreateGiftCheckoutCommand, GiftCheckoutSessionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IGiftCodeProtector _codeProtector;
    private readonly ILogger<CreateGiftCheckoutCommandHandler> _logger;

    public CreateGiftCheckoutCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPaymentGateway paymentGateway,
        IGiftCodeProtector codeProtector,
        ILogger<CreateGiftCheckoutCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _paymentGateway = paymentGateway;
        _codeProtector = codeProtector;
        _logger = logger;
    }

    public async Task<GiftCheckoutSessionDto> Handle(
        CreateGiftCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        if (!_paymentGateway.IsConfigured)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(string.Empty, "Payments are temporarily unavailable.")
            });
        }

        var userId = _currentUserService.UserId.Value;
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.CourseId, cancellationToken);

        if (course == null)
        {
            throw new NotFoundException($"Course {request.CourseId} not found.");
        }

        if (course.Status != CourseStatus.Published || course.Price <= 0)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("CourseId", "Only published paid courses can be bought as gifts.")
            });
        }

        var buyer = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

        if (buyer == null)
        {
            throw new ForbiddenAccessException("User not found.");
        }

        var code = GenerateCode();
        var gift = new GiftPurchase
        {
            BuyerUserId = userId,
            CourseId = course.Id,
            RecipientEmail = request.RecipientEmail.Trim(),
            CodeHash = HashCode(code),
            ProtectedCode = _codeProtector.Protect(code),
            Status = GiftStatus.Pending,
            Amount = course.Price,
            Currency = _paymentGateway.DefaultCurrency.ToUpperInvariant()
        };

        _context.GiftPurchases.Add(gift);
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            var session = await _paymentGateway.CreateGiftCheckoutSessionAsync(
                gift.Id,
                course.Id,
                course.Title,
                gift.Amount,
                buyer.Email ?? string.Empty,
                cancellationToken);

            gift.StripeSessionId = session.SessionId;
            gift.Currency = session.Currency;
            gift.MarkUpdated();
            await _context.SaveChangesAsync(cancellationToken);

            return new GiftCheckoutSessionDto(session.RedirectUrl);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to create checkout session for gift {GiftId}.", gift.Id);
            gift.Status = GiftStatus.Expired;
            gift.MarkUpdated();
            await _context.SaveChangesAsync(cancellationToken);

            throw new ValidationException(new[]
            {
                new ValidationFailure(string.Empty, "Could not start the gift payment. Please try again.")
            });
        }
    }

    private static string GenerateCode() => $"GIFT-{Convert.ToHexString(RandomNumberGenerator.GetBytes(12))}";

    private static string HashCode(string code) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(code.Trim().ToUpperInvariant())));
}

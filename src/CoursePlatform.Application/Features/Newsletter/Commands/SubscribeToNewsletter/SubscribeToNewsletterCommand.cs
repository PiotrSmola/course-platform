using System.Security.Cryptography;
using System.Text;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Options;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoursePlatform.Application.Features.Newsletter.Commands.SubscribeToNewsletter;

public record SubscribeToNewsletterCommand(string Email) : IRequest<NewsletterSubscriptionResult>;

public class SubscribeToNewsletterCommandValidator : AbstractValidator<SubscribeToNewsletterCommand>
{
    public SubscribeToNewsletterCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);
    }
}

public class SubscribeToNewsletterCommandHandler
    : IRequestHandler<SubscribeToNewsletterCommand, NewsletterSubscriptionResult>
{
    private const string AcceptedMessage = "Jeśli adres jest poprawny, wysłaliśmy wiadomość z linkiem potwierdzającym.";

    private readonly IApplicationDbContext _context;
    private readonly IEmailQueue _emailQueue;
    private readonly FrontendOptions _frontendOptions;

    public SubscribeToNewsletterCommandHandler(
        IApplicationDbContext context,
        IEmailQueue emailQueue,
        IOptions<FrontendOptions> frontendOptions)
    {
        _context = context;
        _emailQueue = emailQueue;
        _frontendOptions = frontendOptions.Value;
    }

    public async Task<NewsletterSubscriptionResult> Handle(
        SubscribeToNewsletterCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();
        var subscriptions = _context.NewsletterSubscriptions;
        var subscription = await subscriptions
            .FirstOrDefaultAsync(item => item.NormalizedEmail == normalizedEmail, cancellationToken);

        if (subscription?.Status == NewsletterSubscriptionStatus.Active)
        {
            return new NewsletterSubscriptionResult(AcceptedMessage);
        }

        var confirmationToken = CreateToken();
        var unsubscribeToken = CreateToken();

        if (subscription == null)
        {
            subscription = new NewsletterSubscription
            {
                Email = email,
                NormalizedEmail = normalizedEmail,
                Status = NewsletterSubscriptionStatus.Pending,
                ConfirmationTokenHash = HashToken(confirmationToken),
                UnsubscribeTokenHash = HashToken(unsubscribeToken)
            };
            subscriptions.Add(subscription);
        }
        else
        {
            subscription.Email = email;
            subscription.Status = NewsletterSubscriptionStatus.Pending;
            subscription.ConfirmationTokenHash = HashToken(confirmationToken);
            subscription.UnsubscribeTokenHash = HashToken(unsubscribeToken);
            subscription.ConfirmedAt = null;
            subscription.MarkUpdated();
        }

        await _context.SaveChangesAsync(cancellationToken);

        var confirmationUrl = $"{_frontendOptions.BaseUrl.TrimEnd('/')}/newsletter/confirm?token={Uri.EscapeDataString(confirmationToken)}";
        var (subject, html) = NewFeatureEmailTemplates.NewsletterConfirmation(confirmationUrl);
        _emailQueue.Enqueue(new EmailMessage(email, subject, html));

        return new NewsletterSubscriptionResult(AcceptedMessage);
    }

    private static string CreateToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    private static string HashToken(string token) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}

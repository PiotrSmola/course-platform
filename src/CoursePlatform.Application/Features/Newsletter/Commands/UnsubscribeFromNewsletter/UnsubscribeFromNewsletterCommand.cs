using System.Security.Cryptography;
using System.Text;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Application.Features.Newsletter.Commands.UnsubscribeFromNewsletter;

public record UnsubscribeFromNewsletterCommand(string Token) : IRequest<NewsletterSubscriptionResult>;

public class UnsubscribeFromNewsletterCommandValidator : AbstractValidator<UnsubscribeFromNewsletterCommand>
{
    public UnsubscribeFromNewsletterCommandValidator()
    {
        RuleFor(command => command.Token).NotEmpty().MaximumLength(256);
    }
}

public class UnsubscribeFromNewsletterCommandHandler
    : IRequestHandler<UnsubscribeFromNewsletterCommand, NewsletterSubscriptionResult>
{
    private readonly IApplicationDbContext _context;

    public UnsubscribeFromNewsletterCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NewsletterSubscriptionResult> Handle(
        UnsubscribeFromNewsletterCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)));
        var subscription = await _context.NewsletterSubscriptions
            .FirstOrDefaultAsync(item => item.UnsubscribeTokenHash == tokenHash, cancellationToken);

        if (subscription == null)
        {
            return new NewsletterSubscriptionResult("Link rezygnacji jest nieprawidłowy lub wygasł.");
        }

        if (subscription.Status != NewsletterSubscriptionStatus.Unsubscribed)
        {
            subscription.Status = NewsletterSubscriptionStatus.Unsubscribed;
            subscription.ConfirmationTokenHash = null;
            subscription.MarkUpdated();
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new NewsletterSubscriptionResult("Adres został usunięty z newslettera.");
    }
}

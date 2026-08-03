using System.Security.Cryptography;
using System.Text;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Application.Features.Newsletter.Commands.ConfirmNewsletterSubscription;

public record ConfirmNewsletterSubscriptionCommand(string Token) : IRequest<NewsletterSubscriptionResult>;

public class ConfirmNewsletterSubscriptionCommandValidator : AbstractValidator<ConfirmNewsletterSubscriptionCommand>
{
    public ConfirmNewsletterSubscriptionCommandValidator()
    {
        RuleFor(command => command.Token).NotEmpty().MaximumLength(256);
    }
}

public class ConfirmNewsletterSubscriptionCommandHandler
    : IRequestHandler<ConfirmNewsletterSubscriptionCommand, NewsletterSubscriptionResult>
{
    private readonly IApplicationDbContext _context;

    public ConfirmNewsletterSubscriptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NewsletterSubscriptionResult> Handle(
        ConfirmNewsletterSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)));
        var subscription = await _context.NewsletterSubscriptions
            .FirstOrDefaultAsync(item => item.ConfirmationTokenHash == tokenHash, cancellationToken);

        if (subscription == null)
        {
            return new NewsletterSubscriptionResult("Link potwierdzający jest nieprawidłowy lub wygasł.");
        }

        subscription.Status = NewsletterSubscriptionStatus.Active;
        subscription.ConfirmationTokenHash = null;
        subscription.ConfirmedAt = DateTime.UtcNow;
        subscription.MarkUpdated();
        await _context.SaveChangesAsync(cancellationToken);

        return new NewsletterSubscriptionResult("Subskrypcja newslettera została potwierdzona.");
    }
}

using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Options;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoursePlatform.Application.Features.Gifts.Commands.ResendGiftEmail;

public record ResendGiftEmailCommand(Guid GiftId) : IRequest;

public class ResendGiftEmailCommandValidator : AbstractValidator<ResendGiftEmailCommand>
{
    public ResendGiftEmailCommandValidator()
    {
        RuleFor(command => command.GiftId).NotEmpty();
    }
}

public class ResendGiftEmailCommandHandler : IRequestHandler<ResendGiftEmailCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailQueue _emailQueue;
    private readonly IGiftCodeProtector _codeProtector;
    private readonly FrontendOptions _frontendOptions;

    public ResendGiftEmailCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IEmailQueue emailQueue,
        IGiftCodeProtector codeProtector,
        IOptions<FrontendOptions> frontendOptions)
    {
        _context = context;
        _currentUserService = currentUserService;
        _emailQueue = emailQueue;
        _codeProtector = codeProtector;
        _frontendOptions = frontendOptions.Value;
    }

    public async Task Handle(ResendGiftEmailCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var gift = await _context.GiftPurchases
            .AsNoTracking()
            .Include(item => item.Course)
            .FirstOrDefaultAsync(
                item => item.Id == request.GiftId && item.BuyerUserId == _currentUserService.UserId.Value,
                cancellationToken);

        if (gift == null)
        {
            throw new NotFoundException("Gift purchase not found.");
        }

        if (gift.Status != GiftStatus.Active)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("GiftId", "Kod można wysłać ponownie tylko przed jego wykorzystaniem.")
            });
        }

        string code;
        try
        {
            code = _codeProtector.Unprotect(gift.ProtectedCode);
        }
        catch
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("GiftId", "Nie można ponownie odczytać kodu prezentowego.")
            });
        }

        var redeemUrl = $"{_frontendOptions.BaseUrl.TrimEnd('/')}/gifts/redeem";
        var (subject, html) = GiftEmailTemplates.GiftReceived(gift.Course.Title, code, redeemUrl);
        _emailQueue.Enqueue(new EmailMessage(gift.RecipientEmail, subject, html));
    }
}

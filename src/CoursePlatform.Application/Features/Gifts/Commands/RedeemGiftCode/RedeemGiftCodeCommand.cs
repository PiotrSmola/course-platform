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

namespace CoursePlatform.Application.Features.Gifts.Commands.RedeemGiftCode;

public record RedeemGiftCodeCommand(string Code) : IRequest<GiftRedemptionDto>;

public class RedeemGiftCodeCommandValidator : AbstractValidator<RedeemGiftCodeCommand>
{
    public RedeemGiftCodeCommandValidator()
    {
        RuleFor(command => command.Code).NotEmpty().MaximumLength(128);
    }
}

public class RedeemGiftCodeCommandHandler : IRequestHandler<RedeemGiftCodeCommand, GiftRedemptionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RedeemGiftCodeCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<GiftRedemptionDto> Handle(RedeemGiftCodeCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var codeHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Code.Trim().ToUpperInvariant())));
        var gift = await _context.GiftPurchases
            .Include(item => item.Course)
            .FirstOrDefaultAsync(item => item.CodeHash == codeHash, cancellationToken);

        if (gift == null || gift.Status != GiftStatus.Active)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("Code", "Kod prezentowy jest nieprawidłowy, został już wykorzystany lub nie jest aktywny.")
            });
        }

        var userId = _currentUserService.UserId.Value;
        var alreadyEnrolled = await _context.Enrollments
            .AnyAsync(enrollment => enrollment.UserId == userId && enrollment.CourseId == gift.CourseId, cancellationToken);

        if (alreadyEnrolled || _currentUserService.IsAdmin || gift.Course.InstructorId == userId)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("Code", "Masz już dostęp do tego kursu. Kod nie został wykorzystany.")
            });
        }

        gift.Status = GiftStatus.Redeemed;
        gift.RedeemedByUserId = userId;
        gift.RedeemedAt = DateTime.UtcNow;
        gift.MarkUpdated();

        _context.Enrollments.Add(new Enrollment
        {
            UserId = userId,
            CourseId = gift.CourseId,
            EnrolledAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new GiftRedemptionDto(gift.CourseId, gift.Course.Title);
    }
}

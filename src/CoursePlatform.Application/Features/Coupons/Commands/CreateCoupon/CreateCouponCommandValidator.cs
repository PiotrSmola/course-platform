using FluentValidation;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Coupons.Commands.CreateCoupon;

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Kod jest wymagany")
            .MaximumLength(64)
            .Matches(@"^[A-Za-z0-9_-]+$").WithMessage("Kod może zawierać litery, cyfry, _ i -");
        RuleFor(x => x.DiscountType).IsInEnum();
        RuleFor(x => x.Value).GreaterThan(0);
        RuleFor(x => x.Value)
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType == DiscountType.Percentage)
            .WithMessage("Rabat procentowy nie może przekraczać 100%");
        RuleFor(x => x.ExpiresAt)
            .GreaterThan(x => x.StartsAt)
            .When(x => x.ExpiresAt.HasValue)
            .WithMessage("Data wygaśnięcia musi być późniejsza niż data startu");
        RuleFor(x => x.MaxRedemptions)
            .GreaterThan(0)
            .When(x => x.MaxRedemptions.HasValue);
    }
}

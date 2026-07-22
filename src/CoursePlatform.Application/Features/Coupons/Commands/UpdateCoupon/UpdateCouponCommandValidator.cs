using FluentValidation;
using CoursePlatform.Application.Common.Validation;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Coupons.Commands.UpdateCoupon;

public class UpdateCouponCommandValidator : AbstractValidator<UpdateCouponCommand>
{
    public UpdateCouponCommandValidator()
    {
        RuleFor(x => x.CouponId).ValidGuid("Identyfikator kuponu");
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

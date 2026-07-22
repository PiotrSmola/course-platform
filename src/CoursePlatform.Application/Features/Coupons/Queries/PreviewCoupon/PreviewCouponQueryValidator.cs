using FluentValidation;
using CoursePlatform.Application.Common.Validation;

namespace CoursePlatform.Application.Features.Coupons.Queries.PreviewCoupon;

public class PreviewCouponQueryValidator : AbstractValidator<PreviewCouponQuery>
{
    public PreviewCouponQueryValidator()
    {
        RuleFor(x => x.CourseId).ValidGuid("Identyfikator kursu");
        RuleFor(x => x.CouponCode)
            .NotEmpty().WithMessage("Kod rabatowy jest wymagany")
            .MaximumLength(64);
    }
}

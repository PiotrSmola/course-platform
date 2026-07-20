using FluentValidation;
using CoursePlatform.Application.Common.Validation;

namespace CoursePlatform.Application.Features.Reviews.Commands.UpdateReview;

public class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.CourseId).ValidGuid("Identyfikator kursu");
        RuleFor(x => x.ReviewId).ValidGuid("Identyfikator opinii");
        RuleFor(x => x.Rating).ValidRating();
        RuleFor(x => x.Comment).ValidComment();
    }
}

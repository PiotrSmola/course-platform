using FluentValidation;

namespace CoursePlatform.Application.Features.LessonDiscussions.Queries.GetModerationQueue;

public class GetModerationQueueQueryValidator : AbstractValidator<GetModerationQueueQuery>
{
    public GetModerationQueueQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}

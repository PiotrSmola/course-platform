using FluentValidation;
using CoursePlatform.Application.Common.Validation;

namespace CoursePlatform.Application.Features.LessonDiscussions.Queries.GetLessonDiscussion;

public class GetLessonDiscussionQueryValidator : AbstractValidator<GetLessonDiscussionQuery>
{
    public GetLessonDiscussionQueryValidator()
    {
        RuleFor(x => x.CourseId).ValidGuid("Identyfikator kursu");
        RuleFor(x => x.LessonId).ValidGuid("Identyfikator lekcji");
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}

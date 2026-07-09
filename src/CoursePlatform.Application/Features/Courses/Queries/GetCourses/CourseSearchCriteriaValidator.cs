using FluentValidation;

namespace CoursePlatform.Application.Features.Courses.Queries.GetCourses;

public class CourseSearchCriteriaValidator : AbstractValidator<CourseSearchCriteria>
{
    public CourseSearchCriteriaValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage("Page number must be greater than or equal to 1");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50).WithMessage("Page size must be between 1 and 50");
    }
}

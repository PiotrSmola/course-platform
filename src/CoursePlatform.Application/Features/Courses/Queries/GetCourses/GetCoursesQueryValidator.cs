using FluentValidation;
using CoursePlatform.Application.Common.Validation;

namespace CoursePlatform.Application.Features.Courses.Queries.GetCourses;

public class GetCoursesQueryValidator : AbstractValidator<GetCoursesQuery>
{
    public GetCoursesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage("Numer strony musi być większy lub równy 1");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50).WithMessage("Rozmiar strony musi być między 1 a 50");
    }
}

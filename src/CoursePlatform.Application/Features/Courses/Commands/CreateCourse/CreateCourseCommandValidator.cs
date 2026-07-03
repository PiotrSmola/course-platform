using FluentValidation;
using CoursePlatform.Application.Common.Validation;

namespace CoursePlatform.Application.Features.Courses.Commands.CreateCourse;

public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Title).ValidCourseTitle();
        RuleFor(x => x.Description).ValidCourseDescription();
        RuleFor(x => x.ShortDescription).ValidShortDescription();
        RuleFor(x => x.Level).IsInEnum().WithMessage("Nieprawidłowy poziom kursu.");
        RuleFor(x => x.Price).ValidPrice();
        RuleFor(x => x.Language).ValidLanguage();
        RuleFor(x => x.CategoryIds).NotNull().WithMessage("Kategorie nie mogą być null");
        RuleFor(x => x.TechnologyIds).NotNull().WithMessage("Technologie nie mogą być null");
    }
}

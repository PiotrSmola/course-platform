using FluentValidation;
using CoursePlatform.Application.Common.Validation;

namespace CoursePlatform.Application.Features.Courses.Commands.UpdateCourse;

public class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.Id).ValidGuid("Identyfikator kursu");
        RuleFor(x => x.Title).ValidCourseTitle();
        RuleFor(x => x.Description).ValidCourseDescription();
        RuleFor(x => x.ShortDescription).ValidShortDescription();
        RuleFor(x => x.Level).IsInEnum().WithMessage("Nieprawidłowy poziom kursu.");
        RuleFor(x => x.Status).IsInEnum().WithMessage("Nieprawidłowy status kursu.");
        RuleFor(x => x.Price).ValidPrice();
        RuleFor(x => x.Language).ValidLanguage();
        RuleFor(x => x.CategoryIds).NotNull().WithMessage("Kategorie nie mogą być null");
        RuleFor(x => x.TechnologyIds).NotNull().WithMessage("Technologie nie mogą być null");
    }
}

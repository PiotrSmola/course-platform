using FluentValidation;
using CoursePlatform.Application.Common.Validation;

namespace CoursePlatform.Application.Features.Lessons.Commands.UpdateProgress;

public class UpdateProgressCommandValidator : AbstractValidator<UpdateProgressCommand>
{
    public UpdateProgressCommandValidator()
    {
        RuleFor(x => x.CourseId).ValidGuid("Identyfikator kursu");
        RuleFor(x => x.LessonId).ValidGuid("Identyfikator lekcji");
    }
}

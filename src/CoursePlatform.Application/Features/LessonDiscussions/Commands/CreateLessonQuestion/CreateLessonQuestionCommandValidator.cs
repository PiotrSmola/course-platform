using FluentValidation;
using CoursePlatform.Application.Common.Validation;

namespace CoursePlatform.Application.Features.LessonDiscussions.Commands.CreateLessonQuestion;

public class CreateLessonQuestionCommandValidator : AbstractValidator<CreateLessonQuestionCommand>
{
    public CreateLessonQuestionCommandValidator()
    {
        RuleFor(x => x.CourseId).ValidGuid("Identyfikator kursu");
        RuleFor(x => x.LessonId).ValidGuid("Identyfikator lekcji");
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Treść pytania jest wymagana")
            .ValidComment();
    }
}

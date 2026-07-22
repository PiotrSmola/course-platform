using FluentValidation;
using CoursePlatform.Application.Common.Validation;

namespace CoursePlatform.Application.Features.LessonDiscussions.Commands.CreateLessonAnswer;

public class CreateLessonAnswerCommandValidator : AbstractValidator<CreateLessonAnswerCommand>
{
    public CreateLessonAnswerCommandValidator()
    {
        RuleFor(x => x.CourseId).ValidGuid("Identyfikator kursu");
        RuleFor(x => x.LessonId).ValidGuid("Identyfikator lekcji");
        RuleFor(x => x.QuestionId).ValidGuid("Identyfikator pytania");
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Treść odpowiedzi jest wymagana")
            .ValidComment();
    }
}

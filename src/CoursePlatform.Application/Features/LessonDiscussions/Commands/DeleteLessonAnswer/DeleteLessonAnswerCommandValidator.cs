using FluentValidation;
using CoursePlatform.Application.Common.Validation;

namespace CoursePlatform.Application.Features.LessonDiscussions.Commands.DeleteLessonAnswer;

public class DeleteLessonAnswerCommandValidator : AbstractValidator<DeleteLessonAnswerCommand>
{
    public DeleteLessonAnswerCommandValidator()
    {
        RuleFor(x => x.CourseId).ValidGuid("Identyfikator kursu");
        RuleFor(x => x.LessonId).ValidGuid("Identyfikator lekcji");
        RuleFor(x => x.QuestionId).ValidGuid("Identyfikator pytania");
        RuleFor(x => x.AnswerId).ValidGuid("Identyfikator odpowiedzi");
    }
}

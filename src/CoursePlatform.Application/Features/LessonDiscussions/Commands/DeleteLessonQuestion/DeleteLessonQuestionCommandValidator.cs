using FluentValidation;
using CoursePlatform.Application.Common.Validation;

namespace CoursePlatform.Application.Features.LessonDiscussions.Commands.DeleteLessonQuestion;

public class DeleteLessonQuestionCommandValidator : AbstractValidator<DeleteLessonQuestionCommand>
{
    public DeleteLessonQuestionCommandValidator()
    {
        RuleFor(x => x.CourseId).ValidGuid("Identyfikator kursu");
        RuleFor(x => x.LessonId).ValidGuid("Identyfikator lekcji");
        RuleFor(x => x.QuestionId).ValidGuid("Identyfikator pytania");
    }
}

using FluentValidation;
using CoursePlatform.Application.Common.Validation;

namespace CoursePlatform.Application.Features.Quizzes.Commands.DeleteQuiz;

public class DeleteQuizCommandValidator : AbstractValidator<DeleteQuizCommand>
{
    public DeleteQuizCommandValidator()
    {
        RuleFor(x => x.CourseId).ValidGuid("Identyfikator kursu");
        RuleFor(x => x.LessonId).ValidGuid("Identyfikator lekcji");
    }
}

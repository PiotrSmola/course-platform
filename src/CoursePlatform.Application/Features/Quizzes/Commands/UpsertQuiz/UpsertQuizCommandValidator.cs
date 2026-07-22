using FluentValidation;
using CoursePlatform.Application.Common.Validation;

namespace CoursePlatform.Application.Features.Quizzes.Commands.UpsertQuiz;

public class UpsertQuizCommandValidator : AbstractValidator<UpsertQuizCommand>
{
    public UpsertQuizCommandValidator()
    {
        RuleFor(x => x.CourseId).ValidGuid("Identyfikator kursu");
        RuleFor(x => x.LessonId).ValidGuid("Identyfikator lekcji");
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tytuł quizu jest wymagany")
            .MaximumLength(200);
        RuleFor(x => x.PassThresholdPercent)
            .InclusiveBetween(1, 100).WithMessage("Próg zaliczenia musi być w zakresie 1-100");
        RuleFor(x => x.Questions)
            .NotEmpty().WithMessage("Quiz musi zawierać co najmniej jedno pytanie");
        RuleForEach(x => x.Questions).ChildRules(question =>
        {
            question.RuleFor(q => q.Prompt)
                .NotEmpty().WithMessage("Treść pytania jest wymagana")
                .MaximumLength(1000);
            question.RuleFor(q => q.Options)
                .Must(o => o.Count >= 2).WithMessage("Każde pytanie musi mieć co najmniej 2 opcje");
            question.RuleForEach(q => q.Options).ChildRules(option =>
            {
                option.RuleFor(o => o.Text)
                    .NotEmpty().WithMessage("Treść opcji jest wymagana")
                    .MaximumLength(500);
            });
        });
    }
}

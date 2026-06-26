using FluentValidation;

namespace CoursePlatform.Application.Common.Validation;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ValidName<T>(this IRuleBuilder<T, string> rule, string fieldName)
    {
        return rule
            .NotEmpty().WithMessage($"{fieldName} jest wymagane")
            .MaximumLength(100).WithMessage($"{fieldName} może mieć maksymalnie 100 znaków")
            .Matches(@"^[\p{L}\s'-]+$").WithMessage($"{fieldName} może zawierać tylko litery, spacje, dywizy i apostrofy");
    }

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Email jest wymagany")
            .EmailAddress().WithMessage("Podaj poprawny adres email")
            .MaximumLength(256);
    }

    public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Hasło jest wymagane")
            .MinimumLength(6).WithMessage("Hasło musi mieć minimum 6 znaków")
            .Matches(@"[A-Z]").WithMessage("Hasło musi zawierać co najmniej jedną wielką literę")
            .Matches(@"[a-z]").WithMessage("Hasło musi zawierać co najmniej jedną małą literę")
            .Matches(@"[0-9]").WithMessage("Hasło musi zawierać co najmniej jedną cyfrę")
            .Matches(@"[^A-Za-z0-9]").WithMessage("Hasło musi zawierać co najmniej jeden znak specjalny");
    }

    public static IRuleBuilderOptions<T, string> ValidCourseTitle<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Tytuł jest wymagany")
            .MaximumLength(200).WithMessage("Tytuł może mieć maksymalnie 200 znaków")
            .Matches(@"^[\p{L}\p{N}\s\-_.,!?()]+$").WithMessage("Tytuł zawiera niedozwolone znaki");
    }

    public static IRuleBuilderOptions<T, string> ValidCourseDescription<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Opis jest wymagany")
            .MaximumLength(5000).WithMessage("Opis może mieć maksymalnie 5000 znaków");
    }

    public static IRuleBuilderOptions<T, string> ValidShortDescription<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .MaximumLength(500).WithMessage("Krótki opis może mieć maksymalnie 500 znaków");
    }

    public static IRuleBuilderOptions<T, string> ValidLanguage<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Język jest wymagany")
            .MaximumLength(50).WithMessage("Język może mieć maksymalnie 50 znaków")
            .Matches(@"^[\p{L}\s]+$").WithMessage("Język może zawierać tylko litery i spacje");
    }

    public static IRuleBuilderOptions<T, string> ValidThumbnailUrl<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .MaximumLength(500).WithMessage("URL miniaturki może mieć maksymalnie 500 znaków")
            .Must(uri => string.IsNullOrEmpty(uri) || Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("Podaj poprawny adres URL miniaturki");
    }

    public static IRuleBuilderOptions<T, decimal> ValidPrice<T>(this IRuleBuilder<T, decimal> rule)
    {
        return rule
            .GreaterThanOrEqualTo(0).WithMessage("Cena nie może być ujemna")
            .LessThanOrEqualTo(100000).WithMessage("Cena przekracza maksymalną dopuszczalną wartość");
    }

    public static IRuleBuilderOptions<T, int> ValidRating<T>(this IRuleBuilder<T, int> rule)
    {
        return rule
            .InclusiveBetween(1, 5).WithMessage("Ocena musi być w zakresie 1-5");
    }

    public static IRuleBuilderOptions<T, string> ValidComment<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .MaximumLength(2000).WithMessage("Komentarz może mieć maksymalnie 2000 znaków");
    }

    public static IRuleBuilderOptions<T, Guid> ValidGuid<T>(this IRuleBuilder<T, Guid> rule, string fieldName)
    {
        return rule
            .NotEmpty().WithMessage($"{fieldName} jest wymagany");
    }
}

using FluentValidation;
using CoursePlatform.Application.Common.Validation;

namespace CoursePlatform.Application.Features.Enrollments.Commands.Enroll;

public class EnrollCommandValidator : AbstractValidator<EnrollCommand>
{
    public EnrollCommandValidator()
    {
        RuleFor(x => x.CourseId).ValidGuid("Identyfikator kursu");
    }
}

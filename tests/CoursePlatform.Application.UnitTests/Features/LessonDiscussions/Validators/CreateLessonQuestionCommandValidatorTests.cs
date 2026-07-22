using FluentAssertions;
using FluentValidation.TestHelper;
using CoursePlatform.Application.Features.LessonDiscussions.Commands.CreateLessonQuestion;

namespace CoursePlatform.Application.UnitTests.Features.LessonDiscussions.Validators;

public class CreateLessonQuestionCommandValidatorTests
{
    private readonly CreateLessonQuestionCommandValidator _validator = new();

    [Fact]
    public void Body_Empty_HasError()
    {
        var cmd = new CreateLessonQuestionCommand(Guid.NewGuid(), Guid.NewGuid(), "");
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Body);
    }

    [Fact]
    public void CourseId_Empty_HasError()
    {
        var cmd = new CreateLessonQuestionCommand(Guid.Empty, Guid.NewGuid(), "Question?");
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.CourseId);
    }

    [Fact]
    public void ValidCommand_NoErrors()
    {
        var cmd = new CreateLessonQuestionCommand(Guid.NewGuid(), Guid.NewGuid(), "How does this work?");
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

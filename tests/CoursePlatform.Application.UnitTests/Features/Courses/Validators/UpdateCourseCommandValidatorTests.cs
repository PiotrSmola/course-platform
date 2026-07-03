using FluentValidation.TestHelper;
using CoursePlatform.Application.Features.Courses.Commands.UpdateCourse;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.Courses.Validators;

public class UpdateCourseCommandValidatorTests
{
    private readonly UpdateCourseCommandValidator _validator = new();

    private static UpdateCourseCommand ValidCommand(CourseLevel level = CourseLevel.Beginner, CourseStatus status = CourseStatus.Draft) =>
        new(Guid.NewGuid(), "Title", "Description", "Short", 10, level, status, "pl", new List<Guid>(), new List<Guid>());

    [Fact]
    public void InvalidLevel_HasError()
    {
        var result = _validator.TestValidate(ValidCommand(level: (CourseLevel)999));
        result.ShouldHaveValidationErrorFor(x => x.Level);
    }

    [Fact]
    public void InvalidStatus_HasError()
    {
        var result = _validator.TestValidate(ValidCommand(status: (CourseStatus)999));
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void EmptyId_HasError()
    {
        var cmd = ValidCommand() with { Id = Guid.Empty };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void ValidCommand_NoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }
}

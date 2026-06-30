using FluentAssertions;
using FluentValidation.TestHelper;
using CoursePlatform.Application.Features.Courses.Commands.CreateCourse;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.UnitTests.Features.Courses.Validators;

public class CreateCourseCommandValidatorTests
{
    private readonly CreateCourseCommandValidator _validator = new();

    [Fact]
    public void EmptyTitle_HasError()
    {
        var cmd = new CreateCourseCommand("", "Description", "Short", 0, CourseLevel.Beginner, "", "pl", new List<Guid>(), new List<Guid>());
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void TitleTooLong_HasError()
    {
        var longTitle = new string('a', 201);
        var cmd = new CreateCourseCommand(longTitle, "Description", "Short", 0, CourseLevel.Beginner, "", "pl", new List<Guid>(), new List<Guid>());
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void NegativePrice_HasError()
    {
        var cmd = new CreateCourseCommand("Title", "Description", "Short", -1, CourseLevel.Beginner, "", "pl", new List<Guid>(), new List<Guid>());
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void InvalidLevel_HasError()
    {
        var cmd = new CreateCourseCommand("Title", "Description", "Short", 10, (CourseLevel)999, "", "pl", new List<Guid>(), new List<Guid>());
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Level);
    }

    [Fact]
    public void ValidCommand_NoErrors()
    {
        var cmd = new CreateCourseCommand("Title", "Description", "Short", 10, CourseLevel.Beginner, "", "pl", new List<Guid>(), new List<Guid>());
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
using FluentAssertions;
using FluentValidation.TestHelper;
using CoursePlatform.Application.Features.Reviews.Commands.CreateReview;

namespace CoursePlatform.Application.UnitTests.Features.Reviews.Validators;

public class CreateReviewCommandValidatorTests
{
    private readonly CreateReviewCommandValidator _validator = new();

    [Fact]
    public void Rating_Below1_HasError()
    {
        var cmd = new CreateReviewCommand(Guid.NewGuid(), 0, "Ok");
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Rating_Above5_HasError()
    {
        var cmd = new CreateReviewCommand(Guid.NewGuid(), 6, "Ok");
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Rating_WithinRange_NoError()
    {
        var cmd = new CreateReviewCommand(Guid.NewGuid(), 3, "Ok");
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void CourseId_Empty_HasError()
    {
        var cmd = new CreateReviewCommand(Guid.Empty, 3, "Ok");
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.CourseId);
    }
}
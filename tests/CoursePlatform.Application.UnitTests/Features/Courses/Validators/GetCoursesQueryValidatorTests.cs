using FluentAssertions;
using FluentValidation.TestHelper;
using CoursePlatform.Application.Features.Courses.Queries.GetCourses;

namespace CoursePlatform.Application.UnitTests.Features.Courses.Validators;

public class GetCoursesQueryValidatorTests
{
    private readonly GetCoursesQueryValidator _validator = new();

    [Fact]
    public void PageSize_Over50_HasError()
    {
        var query = new GetCoursesQuery(null, null, null, null, null, null, null, null, null, null, 1, 100);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void PageSize_Below1_HasError()
    {
        var query = new GetCoursesQuery(null, null, null, null, null, null, null, null, null, null, 1, 0);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void PageNumber_Below1_HasError()
    {
        var query = new GetCoursesQuery(null, null, null, null, null, null, null, null, null, null, 0, 10);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }

    [Fact]
    public void PageSize_WithinRange_NoError()
    {
        var query = new GetCoursesQuery(null, null, null, null, null, null, null, null, null, null, 1, 25);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }
}
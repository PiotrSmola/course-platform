using FluentAssertions;
using NetArchTest.Rules;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Infrastructure.Persistence;

namespace CoursePlatform.ArchitectureTests;

public class CleanArchitectureTests
{
    private static readonly System.Reflection.Assembly DomainAssembly = typeof(Course).Assembly;
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(IApplicationDbContext).Assembly;
    private static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(ApplicationDbContext).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Application_Infrastructure_Or_Api()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("CoursePlatform.Application")
            .And()
            .NotHaveDependencyOn("CoursePlatform.Infrastructure")
            .And()
            .NotHaveDependencyOn("CoursePlatform.API")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Because(result));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn("CoursePlatform.Infrastructure")
            .And()
            .NotHaveDependencyOn("CoursePlatform.API")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Because(result));
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn("CoursePlatform.API")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Because(result));
    }

    private static string Because(TestResult result)
    {
        if (result.IsSuccessful || result.FailingTypeNames is null)
        {
            return "Clean Architecture dependency rule violated.";
        }

        return "Violating types: " + string.Join(", ", result.FailingTypeNames);
    }
}

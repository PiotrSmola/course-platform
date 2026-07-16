using FluentAssertions;
using CoursePlatform.Application.Features.Courses.Queries.GetCategories;
using CoursePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.UnitTests.Common;

namespace CoursePlatform.Application.UnitTests.Features.Courses.Queries;

public class GetCategoriesQueryTests
{
    [Fact]
    public async Task Handle_ReturnsCategoriesOrderedByName()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new TestDbContext(options);

        context.Categories.AddRange(
            new Category { Name = "Backend", Slug = "backend", Description = "Be" },
            new Category { Name = "AI", Slug = "ai", Description = "AI" },
            new Category { Name = "Frontend", Slug = "frontend", Description = "Fe" }
        );
        await context.SaveChangesAsync();

        var handler = new GetCategoriesQueryHandler(context, new PassThroughAppCache());
        var result = await handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
        result[0].Name.Should().Be("AI");
        result[1].Name.Should().Be("Backend");
        result[2].Name.Should().Be("Frontend");
    }
}
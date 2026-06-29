using FluentAssertions;
using CoursePlatform.Application.Features.Courses.Queries.GetTechnologies;
using CoursePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.UnitTests.Common;

namespace CoursePlatform.Application.UnitTests.Features.Courses.Queries;

public class GetTechnologiesQueryTests
{
    [Fact]
    public async Task Handle_ReturnsTechnologiesOrderedByName()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new TestDbContext(options);

        context.Technologies.AddRange(
            new Technology { Id = Guid.NewGuid(), Name = "React", Slug = "react", Description = "R" },
            new Technology { Id = Guid.NewGuid(), Name = "Angular", Slug = "angular", Description = "A" },
            new Technology { Id = Guid.NewGuid(), Name = "Vue", Slug = "vue", Description = "V" }
        );
        await context.SaveChangesAsync();

        var handler = new GetTechnologiesQueryHandler(context);
        var result = await handler.Handle(new GetTechnologiesQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Angular");
        result[1].Name.Should().Be("React");
        result[2].Name.Should().Be("Vue");
    }
}
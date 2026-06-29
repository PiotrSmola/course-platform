using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Modules.Commands.CreateModule;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using CoursePlatform.Application.UnitTests.Common;

namespace CoursePlatform.Application.UnitTests.Features.Modules.Commands;

public class CreateModuleCommandTests
{
    [Fact]
    public async Task Handle_ValidRequest_CreatesModule()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new TestDbContext(options);

        var instructorId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        context.Courses.Add(new Course
        {
            Id = courseId,
            Title = "Test",
            Description = "D",
            ShortDescription = "S",
            Price = 10,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Draft,
            ThumbnailUrl = "",
            Language = "pl",
            InstructorId = instructorId,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(instructorId);

        var userManager = TestUserManagerFactory.Create();
        var handler = new CreateModuleCommandHandler(context, currentUser.Object, userManager);

        var moduleId = await handler.Handle(new CreateModuleCommand(courseId, "Module 1", 0), CancellationToken.None);

        moduleId.Should().NotBeEmpty();
        var module = await context.Modules.FindAsync(moduleId);
        module.Should().NotBeNull();
        module!.Title.Should().Be("Module 1");
    }
}

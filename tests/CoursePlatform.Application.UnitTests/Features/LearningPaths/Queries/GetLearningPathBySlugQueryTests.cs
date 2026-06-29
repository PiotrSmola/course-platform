using FluentAssertions;
using CoursePlatform.Application.Features.LearningPaths.Queries.GetLearningPathBySlug;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.UnitTests.Common;

namespace CoursePlatform.Application.UnitTests.Features.LearningPaths.Queries;

public class GetLearningPathBySlugQueryTests
{
    private readonly TestDbContext _context;

    public GetLearningPathBySlugQueryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_PathWithDraftCourse_ExcludesDraft()
    {
        var instructor = new ApplicationUser { Id = Guid.NewGuid(), UserName = "i@t.com", Email = "i@t.com", FirstName = "A", LastName = "B" };
        var publishedCourse = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Published",
            Description = "D",
            ShortDescription = "S",
            Price = 10,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            ThumbnailUrl = "",
            Language = "pl",
            InstructorId = instructor.Id,
            CreatedAt = DateTime.UtcNow,
            Categories = new List<Category>(),
            Technologies = new List<Technology>(),
            Modules = new List<Module>(),
            Reviews = new List<Review>()
        };
        var draftCourse = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Draft",
            Description = "D",
            ShortDescription = "S",
            Price = 10,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Draft,
            ThumbnailUrl = "",
            Language = "pl",
            InstructorId = instructor.Id,
            CreatedAt = DateTime.UtcNow,
            Categories = new List<Category>(),
            Technologies = new List<Technology>(),
            Modules = new List<Module>(),
            Reviews = new List<Review>()
        };
        var path = new LearningPath
        {
            Id = Guid.NewGuid(),
            Title = "Path",
            Slug = "path-slug",
            ShortDescription = "S",
            Description = "D",
            DifficultyLevel = PathDifficultyLevel.Beginner,
            EstimatedHours = 10,
            ThumbnailUrl = "",
            DisplayOrder = 1,
            IsPublished = true,
            PathCourses = new List<LearningPathCourse>
            {
                new() { Id = Guid.NewGuid(), CourseId = publishedCourse.Id, Course = publishedCourse, Order = 1, IsOptional = false },
                new() { Id = Guid.NewGuid(), CourseId = draftCourse.Id, Course = draftCourse, Order = 2, IsOptional = false }
            }
        };
        _context.Users.Add(instructor);
        _context.Courses.Add(publishedCourse);
        _context.Courses.Add(draftCourse);
        _context.LearningPaths.Add(path);
        await _context.SaveChangesAsync();

        var handler = new GetLearningPathBySlugQueryHandler(_context);
        var result = await handler.Handle(new GetLearningPathBySlugQuery("path-slug"), CancellationToken.None);

        result.Courses.Should().HaveCount(1);
        result.Courses.Should().NotContain(c => c.CourseTitle == "Draft");
        result.Courses.Should().Contain(c => c.CourseTitle == "Published");
    }

    [Fact]
    public async Task Handle_UnpublishedPath_ThrowsNotFound()
    {
        var path = new LearningPath
        {
            Id = Guid.NewGuid(),
            Title = "P",
            Slug = "unpublished-slug",
            ShortDescription = "S",
            Description = "D",
            DifficultyLevel = PathDifficultyLevel.Beginner,
            EstimatedHours = 10,
            ThumbnailUrl = "",
            DisplayOrder = 1,
            IsPublished = false,
            PathCourses = new List<LearningPathCourse>()
        };
        _context.LearningPaths.Add(path);
        await _context.SaveChangesAsync();

        var handler = new GetLearningPathBySlugQueryHandler(_context);
        var act = async () => await handler.Handle(new GetLearningPathBySlugQuery("unpublished-slug"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
using FluentAssertions;
using Moq;
using CoursePlatform.Application.Common.Interfaces;
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
    private readonly Mock<IFileStorageService> _fileStorageMock;

    public GetLearningPathBySlugQueryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
        _fileStorageMock = new Mock<IFileStorageService>();
    }

    [Fact]
    public async Task Handle_PathWithDraftCourse_ExcludesDraft()
    {
        var instructor = new ApplicationUser { Id = Guid.NewGuid(), UserName = "i@t.com", Email = "i@t.com", FirstName = "A", LastName = "B" };
        _context.Users.Add(instructor);
        await _context.SaveChangesAsync();

        var publishedCourse = new Course
        {
            Title = "Published",
            Description = "D",
            ShortDescription = "S",
            Price = 10,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            ThumbnailObjectKey = "",
            Language = "pl",
            InstructorId = instructor.Id,
            Categories = new List<Category>(),
            Technologies = new List<Technology>(),
            Modules = new List<Module>(),
            Reviews = new List<Review>()
        };
        var draftCourse = new Course
        {
            Title = "Draft",
            Description = "D",
            ShortDescription = "S",
            Price = 10,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Draft,
            ThumbnailObjectKey = "",
            Language = "pl",
            InstructorId = instructor.Id,
            Categories = new List<Category>(),
            Technologies = new List<Technology>(),
            Modules = new List<Module>(),
            Reviews = new List<Review>()
        };
        var path = new LearningPath
        {
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
                new() { CourseId = publishedCourse.Id, Course = publishedCourse, Order = 1, IsOptional = false },
                new() { CourseId = draftCourse.Id, Course = draftCourse, Order = 2, IsOptional = false }
            }
        };
        _context.Courses.Add(publishedCourse);
        _context.Courses.Add(draftCourse);
        _context.LearningPaths.Add(path);
        await _context.SaveChangesAsync();

        var handler = new GetLearningPathBySlugQueryHandler(_context, _fileStorageMock.Object);
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

        var handler = new GetLearningPathBySlugQueryHandler(_context, _fileStorageMock.Object);
        var act = async () => await handler.Handle(new GetLearningPathBySlugQuery("unpublished-slug"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
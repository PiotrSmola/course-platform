using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using CoursePlatform.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CoursePlatform.IntegrationTests;

internal static class IntegrationTestHelpers
{
    public static async Task<AuthResponse> RegisterAsync(HttpClient client, string? email = null)
    {
        email ??= $"user-{Guid.NewGuid():N}@example.com";
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = "Student123!",
            FirstName = "Test",
            LastName = "User"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    public static async Task<AuthResponse> LoginAsync(HttpClient client, string email, string password = "Student123!")
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    public static void Authenticate(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<(Guid CourseId, Guid LessonId, string InstructorEmail, string StudentEmail)> SeedPublishedCourseWithLessonAsync(
        CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var instructorEmail = $"instr-{Guid.NewGuid():N}@example.com";
        var studentEmail = $"stud-{Guid.NewGuid():N}@example.com";

        var instructor = new ApplicationUser
        {
            UserName = instructorEmail,
            Email = instructorEmail,
            FirstName = "Instr",
            LastName = "Uctor",
            EmailConfirmed = true
        };
        (await userManager.CreateAsync(instructor, "Instructor123!")).Succeeded.Should().BeTrue();
        (await userManager.AddToRoleAsync(instructor, "Instructor")).Succeeded.Should().BeTrue();
        (await userManager.AddToRoleAsync(instructor, "Student")).Succeeded.Should().BeTrue();

        var student = new ApplicationUser
        {
            UserName = studentEmail,
            Email = studentEmail,
            FirstName = "Stud",
            LastName = "Ent",
            EmailConfirmed = true
        };
        (await userManager.CreateAsync(student, "Student123!")).Succeeded.Should().BeTrue();
        (await userManager.AddToRoleAsync(student, "Student")).Succeeded.Should().BeTrue();

        var course = new Course
        {
            Title = "Integration Free Course",
            Description = "For access tests",
            ShortDescription = "Free",
            Price = 0,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            Language = "pl",
            InstructorId = instructor.Id,
            Modules =
            [
                new Module
                {
                    Title = "Module 1",
                    Order = 1,
                    Lessons =
                    [
                        new Lesson
                        {
                            Title = "Lesson 1",
                            Description = "Intro",
                            Duration = 10,
                            Order = 1,
                            VideoObjectKey = "placeholder/videos/integration-test.mp4"
                        }
                    ]
                }
            ]
        };

        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var lessonId = course.Modules.Single().Lessons.Single().Id;
        return (course.Id, lessonId, instructorEmail, studentEmail);
    }
}

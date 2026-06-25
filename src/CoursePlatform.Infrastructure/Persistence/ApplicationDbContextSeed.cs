using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Infrastructure.Persistence;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedCategoriesAsync(context);
        await SeedTechnologiesAsync(context);
        await SeedCoursesAsync(context, userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = new[] { "Student", "Instructor", "Admin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        if (await userManager.FindByEmailAsync("admin@courseplatform.com") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@courseplatform.com",
                Email = "admin@courseplatform.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (await userManager.FindByEmailAsync("instructor@courseplatform.com") == null)
        {
            var instructor = new ApplicationUser
            {
                UserName = "instructor@courseplatform.com",
                Email = "instructor@courseplatform.com",
                FirstName = "John",
                LastName = "Doe",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(instructor, "Instructor123!");
            await userManager.AddToRoleAsync(instructor, "Instructor");
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        var categories = new[]
        {
            new Category { Id = Guid.NewGuid(), Name = "Backend", Slug = "backend", CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Frontend", Slug = "frontend", CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "AI", Slug = "ai", CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Databases", Slug = "databases", CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "DevOps", Slug = "devops", CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Mobile", Slug = "mobile", CreatedAt = DateTime.UtcNow }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTechnologiesAsync(ApplicationDbContext context)
    {
        if (await context.Technologies.AnyAsync()) return;

        var technologies = new[]
        {
            new Technology { Id = Guid.NewGuid(), Name = ".NET", Slug = "dotnet", CreatedAt = DateTime.UtcNow },
            new Technology { Id = Guid.NewGuid(), Name = "Laravel", Slug = "laravel", CreatedAt = DateTime.UtcNow },
            new Technology { Id = Guid.NewGuid(), Name = "Python", Slug = "python", CreatedAt = DateTime.UtcNow },
            new Technology { Id = Guid.NewGuid(), Name = "React", Slug = "react", CreatedAt = DateTime.UtcNow },
            new Technology { Id = Guid.NewGuid(), Name = "Vue", Slug = "vue", CreatedAt = DateTime.UtcNow },
            new Technology { Id = Guid.NewGuid(), Name = "Angular", Slug = "angular", CreatedAt = DateTime.UtcNow },
            new Technology { Id = Guid.NewGuid(), Name = "SQL", Slug = "sql", CreatedAt = DateTime.UtcNow },
            new Technology { Id = Guid.NewGuid(), Name = "MongoDB", Slug = "mongodb", CreatedAt = DateTime.UtcNow },
            new Technology { Id = Guid.NewGuid(), Name = "Docker", Slug = "docker", CreatedAt = DateTime.UtcNow },
            new Technology { Id = Guid.NewGuid(), Name = "AWS", Slug = "aws", CreatedAt = DateTime.UtcNow }
        };

        context.Technologies.AddRange(technologies);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCoursesAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        if (await context.Courses.AnyAsync()) return;

        var instructor = await userManager.FindByEmailAsync("instructor@courseplatform.com");
        if (instructor == null) return;

        var categories = await context.Categories.ToListAsync();
        var technologies = await context.Technologies.ToListAsync();

        var frontendCat = categories.First(c => c.Slug == "frontend");
        var backendCat = categories.First(c => c.Slug == "backend");
        var vueTech = technologies.First(t => t.Slug == "vue");
        var dotnetTech = technologies.First(t => t.Slug == "dotnet");

        var course1 = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Introduction to Vue 3",
            Description = "Learn Vue 3 from scratch with Composition API and TypeScript.",
            ShortDescription = "Vue 3 + TS crash course",
            Price = 49.99m,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            ThumbnailUrl = "https://placeholder.local/vue3.jpg",
            Language = "English",
            InstructorId = instructor.Id,
            CreatedAt = DateTime.UtcNow,
            Categories = new List<Category> { frontendCat },
            Technologies = new List<Technology> { vueTech }
        };

        var course2 = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Advanced .NET 9 Web API",
            Description = "Build production-ready APIs with ASP.NET Core 9, Clean Architecture and CQRS.",
            ShortDescription = "Master .NET 9 backend development",
            Price = 89.99m,
            Level = CourseLevel.Advanced,
            Status = CourseStatus.Published,
            ThumbnailUrl = "https://placeholder.local/dotnet9.jpg",
            Language = "English",
            InstructorId = instructor.Id,
            CreatedAt = DateTime.UtcNow,
            Categories = new List<Category> { backendCat },
            Technologies = new List<Technology> { dotnetTech }
        };

        context.Courses.AddRange(course1, course2);
        await context.SaveChangesAsync();
    }
}

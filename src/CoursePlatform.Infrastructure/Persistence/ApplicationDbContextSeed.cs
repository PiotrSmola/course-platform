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

    private static async Task SeedCoursesAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        if (await context.Courses.AnyAsync()) return;

        var instructor = await userManager.FindByEmailAsync("instructor@courseplatform.com");
        if (instructor == null) return;

        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Introduction to Vue 3",
            Description = "Learn Vue 3 from scratch with Composition API and TypeScript.",
            ShortDescription = "Vue 3 + TS crash course",
            Price = 49.99m,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            ThumbnailUrl = "https://placeholder.local/vue3.jpg",
            InstructorId = instructor.Id,
            CreatedAt = DateTime.UtcNow
        };

        context.Courses.Add(course);
        await context.SaveChangesAsync();
    }
}

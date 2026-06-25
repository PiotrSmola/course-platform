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
        var categoryData = new (string Name, string Slug, string Description)[]
        {
            ("Backend", "backend", "Kursy tworzenia API, mikroserwisów, architektur systemowych i wzorców projektowych po stronie serwera."),
            ("Frontend", "frontend", "Tworzenie nowoczesnych interfejsów webowych, frameworki SPA, dostępność i animacje."),
            ("AI", "ai", "Sztuczna inteligencja, uczenie maszynowe, LLM, przetwarzanie języka naturalnego i wizja komputerowa."),
            ("Databases", "databases", "Bazy relacyjne i NoSQL, modelowanie danych, optymalizacja zapytań i skalowanie."),
            ("DevOps", "devops", "CI/CD, konteneryzacja, orkiestracja, infrastruktura jako kod i monitoring produkcyjny."),
            ("Mobile", "mobile", "Tworzenie aplikacji na iOS i Androida, React Native, Flutter oraz natywne SDK.")
        };

        var existing = await context.Categories.ToListAsync();
        foreach (var (name, slug, description) in categoryData)
        {
            var cat = existing.FirstOrDefault(c => c.Slug == slug);
            if (cat == null)
            {
                context.Categories.Add(new Category
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Slug = slug,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (string.IsNullOrEmpty(cat.Description))
            {
                cat.Description = description;
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedTechnologiesAsync(ApplicationDbContext context)
    {
        var techData = new (string Name, string Slug, string Description)[]
        {
            (".NET", "dotnet", "Platforma .NET od podstaw do zaawansowanych wzorców: ASP.NET Core, EF Core, Blazor."),
            ("Laravel", "laravel", "Framework PHP do szybkiego budowania aplikacji webowych i API."),
            ("Python", "python", "Język Python w analizie danych, automatyzacji, AI i tworzeniu API."),
            ("React", "react", "Biblioteka React, JSX, hooki, server components i ekosystem."),
            ("Vue", "vue", "Framework Vue 3 z Composition API, Pinia, Vue Router i testowaniem."),
            ("Angular", "angular", "Platforma Angular, RxJS, NgRx i architektura enterprise."),
            ("SQL", "sql", "Język SQL, relacyjne bazy danych i zaawansowane zapytania."),
            ("MongoDB", "mongodb", "Dokumentowa baza NoSQL, agregacje i modelowanie danych."),
            ("Docker", "docker", "Konteneryzacja aplikacji, Dockerfile, Compose i wielostopniowe buildy."),
            ("AWS", "aws", "Amazon Web Services: EC2, S3, Lambda, RDS i architektura chmurowa.")
        };

        var existing = await context.Technologies.ToListAsync();
        foreach (var (name, slug, description) in techData)
        {
            var tech = existing.FirstOrDefault(t => t.Slug == slug);
            if (tech == null)
            {
                context.Technologies.Add(new Technology
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Slug = slug,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (string.IsNullOrEmpty(tech.Description))
            {
                tech.Description = description;
            }
        }

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

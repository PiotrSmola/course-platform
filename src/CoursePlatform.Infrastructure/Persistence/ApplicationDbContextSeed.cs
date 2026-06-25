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
        await SeedLearningPathsAsync(context);
        await SeedBusinessPlansAsync(context);
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

    private static async Task SeedLearningPathsAsync(ApplicationDbContext context)
    {
        if (await context.LearningPaths.AnyAsync()) return;

        var courses = await context.Courses.ToListAsync();
        var vueCourse = courses.FirstOrDefault(c => c.Title == "Introduction to Vue 3");
        var dotnetCourse = courses.FirstOrDefault(c => c.Title == "Advanced .NET 9 Web API");
        if (vueCourse == null || dotnetCourse == null) return;

        var backendPath = new LearningPath
        {
            Id = Guid.NewGuid(),
            Title = "Backend Developer",
            Slug = "backend-developer",
            ShortDescription = "Zostań backend developerem — od podstaw API do zaawansowanych wzorców w .NET.",
            Description = "Kompleksowa ścieżka dla osób, które chcą budować solidne systemy serwerowe. Nauczysz się projektować REST API, pracować z bazami danych, stosować wzorce CQRS i Clean Architecture w ekosystemie .NET 9.",
            DifficultyLevel = PathDifficultyLevel.Intermediate,
            EstimatedHours = 60,
            ThumbnailUrl = "https://placeholder.local/paths/backend-developer.jpg",
            DisplayOrder = 1,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            PathCourses = new List<LearningPathCourse>
            {
                new() { Id = Guid.NewGuid(), CourseId = dotnetCourse.Id, Order = 1, IsOptional = false }
            }
        };

        var frontendPath = new LearningPath
        {
            Id = Guid.NewGuid(),
            Title = "Frontend Developer",
            Slug = "frontend-developer",
            ShortDescription = "Naucz się budować nowoczesne interfejsy webowe w Vue 3.",
            Description = "Ścieżka dla osób stawiających pierwsze kroki w frontendzie. Composition API, TypeScript, zarządzanie stanem i testowanie — wszystko w jednym kursie.",
            DifficultyLevel = PathDifficultyLevel.Beginner,
            EstimatedHours = 40,
            ThumbnailUrl = "https://placeholder.local/paths/frontend-developer.jpg",
            DisplayOrder = 2,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            PathCourses = new List<LearningPathCourse>
            {
                new() { Id = Guid.NewGuid(), CourseId = vueCourse.Id, Order = 1, IsOptional = false }
            }
        };

        var fullstackPath = new LearningPath
        {
            Id = Guid.NewGuid(),
            Title = "Full-Stack Web Developer",
            Slug = "full-stack-web-developer",
            ShortDescription = "Połącz frontend i backend — kompletna ścieżka od interfejsu po API.",
            Description = "Dla tych, którzy chcą rozumieć całość stosu technologicznego. Zaczynasz od interfejsu użytkownika w Vue 3, a następnie budujesz solidne API w .NET 9. Na końcu wiesz, jak spiąć obie warstwy w działający produkt.",
            DifficultyLevel = PathDifficultyLevel.Advanced,
            EstimatedHours = 100,
            ThumbnailUrl = "https://placeholder.local/paths/fullstack-developer.jpg",
            DisplayOrder = 3,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            PathCourses = new List<LearningPathCourse>
            {
                new() { Id = Guid.NewGuid(), CourseId = vueCourse.Id, Order = 1, IsOptional = false },
                new() { Id = Guid.NewGuid(), CourseId = dotnetCourse.Id, Order = 2, IsOptional = false }
            }
        };

        context.LearningPaths.AddRange(backendPath, frontendPath, fullstackPath);
        await context.SaveChangesAsync();
    }

    private static async Task SeedBusinessPlansAsync(ApplicationDbContext context)
    {
        if (await context.BusinessPlans.AnyAsync()) return;

        var starter = new BusinessPlan
        {
            Id = Guid.NewGuid(),
            Name = "Starter",
            Slug = "starter",
            ShortDescription = "Idealny na start — dostęp do biblioteki kursów dla jednego pracownika.",
            Price = 0m,
            Currency = "PLN",
            BillingPeriod = BillingPeriod.Monthly,
            PriceLabel = "Bezpłatny",
            CallToActionText = "Rozpocznij",
            CallToActionUrl = "/register",
            IsFeatured = false,
            DisplayOrder = 1,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        var team = new BusinessPlan
        {
            Id = Guid.NewGuid(),
            Name = "Team",
            Slug = "team",
            ShortDescription = "Dla małych zespołów — zarządzanie użytkownikami i raportowanie postępów.",
            Price = 99m,
            Currency = "PLN",
            BillingPeriod = BillingPeriod.Monthly,
            PriceLabel = null,
            CallToActionText = "Skontaktuj się z nami",
            CallToActionUrl = "/business/contact?plan=team",
            IsFeatured = true,
            DisplayOrder = 2,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        var enterprise = new BusinessPlan
        {
            Id = Guid.NewGuid(),
            Name = "Enterprise",
            Slug = "enterprise",
            ShortDescription = "Dla dużych organizacji — indywidualne warunki, dedykowany opiekun, integracje SSO.",
            Price = null,
            Currency = null,
            BillingPeriod = null,
            PriceLabel = "Cena indywidualna",
            CallToActionText = "Porozmawiajmy",
            CallToActionUrl = "/business/contact?plan=enterprise",
            IsFeatured = false,
            DisplayOrder = 3,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        starter.Features = new List<BusinessPlanFeature>
        {
            new() { Id = Guid.NewGuid(), Text = "Dostęp do wybranych kursów", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Text = "1 miejsce dla pracownika", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Text = "Certyfikaty ukończenia", DisplayOrder = 3 }
        };

        team.Features = new List<BusinessPlanFeature>
        {
            new() { Id = Guid.NewGuid(), Text = "Pełen dostęp do biblioteki kursów", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Text = "Do 25 miejsc w zespole", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Text = "Panel postępów i raportowanie", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Text = "Priorytetowe wsparcie", DisplayOrder = 4 }
        };

        enterprise.Features = new List<BusinessPlanFeature>
        {
            new() { Id = Guid.NewGuid(), Text = "Nieograniczone miejsca", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Text = "SSO i integracje (SAML, SCIM)", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Text = "Dedykowany opiekun klienta", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), Text = "Własne ścieżki szkoleniowe", DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), Text = "SLA i umowy powierzenia danych", DisplayOrder = 5 }
        };

        context.BusinessPlans.AddRange(starter, team, enterprise);
        await context.SaveChangesAsync();
    }
}

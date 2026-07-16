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
        await SeedEnrollmentsAsync(context);
        await SeedReviewsAsync(context);
        await SeedLessonProgressAsync(context);
        await SeedLearningPathsAsync(context);
        await SeedBusinessPlansAsync(context);
    }

    public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
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

        var instructors = new (string Email, string First, string Last, string Password)[]
        {
            ("instructor@courseplatform.com", "John", "Doe", "Instructor123!"),
            ("anna.kowalska@courseplatform.com", "Anna", "Kowalska", "Instructor123!"),
            ("marcin.wisniewski@courseplatform.com", "Marcin", "Wiśniewski", "Instructor123!"),
        };

        foreach (var (email, first, last, password) in instructors)
        {
            if (await userManager.FindByEmailAsync(email) != null) continue;
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = first,
                LastName = last,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, "Instructor");
        }

        var students = new (string Email, string First, string Last, string Password)[]
        {
            ("piotr.nowak@courseplatform.com", "Piotr", "Nowak", "Student123!"),
            ("karolina.zielinska@courseplatform.com", "Karolina", "Zielińska", "Student123!"),
            ("tomasz.wojcik@courseplatform.com", "Tomasz", "Wójcik", "Student123!"),
            ("monika.kaminska@courseplatform.com", "Monika", "Kamińska", "Student123!"),
            ("jakub.lewandowski@courseplatform.com", "Jakub", "Lewandowski", "Student123!"),
        };

        foreach (var (email, first, last, password) in students)
        {
            if (await userManager.FindByEmailAsync(email) != null) continue;
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = first,
                LastName = last,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, "Student");
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
                    Name = name,
                    Slug = slug,
                    Description = description,
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
                    Name = name,
                    Slug = slug,
                    Description = description,
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

        var john = await userManager.FindByEmailAsync("instructor@courseplatform.com");
        var anna = await userManager.FindByEmailAsync("anna.kowalska@courseplatform.com");
        var marcin = await userManager.FindByEmailAsync("marcin.wisniewski@courseplatform.com");
        if (john == null || anna == null || marcin == null) return;

        var categories = await context.Categories.ToListAsync();
        var technologies = await context.Technologies.ToListAsync();

        Category Cat(string slug) => categories.First(c => c.Slug == slug);
        Technology Tech(string slug) => technologies.First(t => t.Slug == slug);

        var now = DateTime.UtcNow;
        var courses = new List<Course>();

        var vueCourse = NewCourse(
            "Vue 3 Fundamentals — Composition API i TypeScript",
            "Kompletny kurs Vue 3 dla osób, które znają podstawy HTML, CSS i JavaScript i chcą wejść na poziom profesjonalnego frontendu. Zaczynasz od zrozumienia reaktywności i różnic między Options API a Composition API, a kończysz na budowaniu w pełni typowanej aplikacji SPA z wykorzystaniem Vue Router, Pinia oraz narzędzi deweloperskich (Vite, Volar, Vitest). W trakcie kursu poznasz nowoczesne wzorce projektowe, takie jak composables, provide/inject, teleport czy suspense, a także nauczysz się testować komponenty i pisać kod zgodny z konwencjami społeczności. Każdy moduł kończy się praktycznym ćwiczeniem, które pozwala utrwalić wiedzę na realnym przykładzie. Po ukończeniu kursu samodzielnie zaprojektujesz, zaimplementujesz i wdrożysz aplikację webową w Vue 3.",
            "Vue 3 od zera — Composition API, TypeScript, Pinia, Vue Router, testy komponentów",
            149.00m,
            CourseLevel.Beginner,
            john.Id,
            new List<Category> { Cat("frontend") },
            new List<Technology> { Tech("vue") });
        AddModules(vueCourse,
            ("Wprowadzenie do Vue 3", new[]
            {
                ("Czym jest Vue 3 i jak działa reaktywność", "Omówienie filozofii frameworka, wirtualnego DOM-u i systemu reaktywnego opartego na Proxy.", 720),
                ("Konfiguracja środowiska z Vite i Volar", "Przygotowanie projektu, konfiguracja TypeScript, ESLint i Prettier pod kątem jakości kodu.", 900),
                ("Pierwszy komponent — szablony, dyrektywy i propsy", "Budowa komponentu krok po kroku, przekazywanie danych i obsługa zdarzeń.", 1080),
            }),
            ("Composition API w praktyce", new[]
            {
                ("ref, reactive i computed — kiedy czego używać", "Porównanie typów reaktywnych, typowanie w TypeScript i pułapki mutacji.", 1320),
                ("Watchery, watchEffect i cykl życia", "Reagowanie na zmiany stanu, czyszczenie efektów ubocznych i wzorce anulowania.", 1140),
                ("Composables — wyciąganie logiki poza komponenty", "Tworzenie własnych composables i testowanie ich w izolacji.", 1260),
            }),
            ("Stan, routing i formularze", new[]
            {
                ("Pinia — centralny magazyn stanu", "Konfiguracja store'a, moduły, gettery i akcje, persistowanie stanu.", 1380),
                ("Vue Router — trasy, parametry i lazy loading", "Budowanie struktury nawigacji, strażnicy tras i code splitting.", 1200),
                ("Formularze i walidacja z VeeValidate + Zod", "Obsługa formularzy, walidacja schematów i komunikacja błędów.", 1440),
            }),
            ("Testy i ekosystem", new[]
            {
                ("Testy jednostkowe komponentów z Vitest", "Renderowanie komponentów, asercje i mockowanie zależności.", 1080),
                ("Testy end-to-end z Playwright", "Scenariusze użytkownika, debugowanie i CI.", 1320),
                ("Budowanie i wdrażanie na Netlify", "Bundle, zmienne środowiskowe i continuous deployment.", 900),
            })
        );
        courses.Add(vueCourse);

        var dotnetCourse = NewCourse(
            "Zaawansowane .NET 9 Web API — Clean Architecture i CQRS",
            "Kurs dla programistów C#, którzy chcą budować produkcyjne API w ekosystemie .NET 9 z zachowaniem najwyższych standardów jakości. Zaczynasz od fundamentów Clean Architecture i zasad odwróconych zależności, następnie przechodzisz do implementacji wzorca CQRS z MediatR, walidacji z FluentValidation i mapowania obiektów. Duży nacisk położony jest na testowalność — piszesz testy jednostkowe z xUnit i Moq oraz integracyjne z WebApplicationFactory. Omawiane są też tematy zaawansowane: caching wielopoziomowy, obserwowalność przez OpenTelemetry, bezpieczeństwo (JWT, refresh tokeny, RBAC) oraz wdrożenie kontenerowe. Kurs kończy się pełnym projektem reference implementation, który można wykorzystać jako szablon we własnej organizacji.",
            "Clean Architecture, CQRS, MediatR, EF Core, testy integracyjne, OpenTelemetry i wdrożenie",
            299.00m,
            CourseLevel.Advanced,
            john.Id,
            new List<Category> { Cat("backend") },
            new List<Technology> { Tech("dotnet") });
        AddModules(dotnetCourse,
            ("Architektura i podstawy", new[]
            {
                ("Przegląd Clean Architecture i granic warstw", "Omówienie Domain, Application, Infrastructure i API oraz kierunków zależności.", 1200),
                ("Konfiguracja solucji .NET 9 i SDK", "Konwencje projektu, central package management, Directory.Build.props.", 1080),
                ("Wzorzec CQRS i MediatR w praktyce", "Implementacja command i query handlerów, behaviors i pipeline'ów.", 1500),
            }),
            ("Domain i Application", new[]
            {
                ("Bogate modele domeny i value objects", "Budowa encji, invarianty i wyrażenia domenowe w czystym C#.", 1320),
                ("Walidacja z FluentValidation", "Definiowanie reguł, walidacja asynchroniczna i integracja z pipeline.", 1140),
                ("Mapowanie obiektów i DTO", "Profile AutoMapper, kontra Mapster i ręczne mappery w testowalnym kodzie.", 1080),
            }),
            ("Infrastructure i EF Core", new[]
            {
                ("Konfiguracja DbContext i migracje", "Fluent API, konwencje, seed data i strategie migracji w produkcji.", 1260),
                ("Wydajność i diagnostyka zapytań", "Profilowanie, AsNoTracking, split queries, interceptory.", 1200),
                ("Cache wielopoziomowy i Redis", "Pamięć podręczna w pamięci, distributed cache i Cache-Aside.", 1380),
            }),
            ("API, bezpieczeństwo i obserwowalność", new[]
            {
                ("JWT, refresh tokeny i polityki autoryzacji", "Implementacja bezpiecznego flow uwierzytelniania i RBAC.", 1440),
                ("OpenTelemetry, Prometheus i logi strukturalne", "Śledzenie żądań, metryki i korelacja zdarzeń w systemie rozproszonym.", 1320),
                ("Testy integracyjne z WebApplicationFactory", "Izolowane środowisko testowe, Testcontainers i asercje HTTP.", 1500),
            })
        );
        courses.Add(dotnetCourse);

        var reactCourse = NewCourse(
            "React 18 w Praktyce — Hooks, Suspense i Server Components",
            "Praktyczny kurs Reacta dla osób, które znają już JavaScript i chcą budować nowoczesne, wydajne interfejsy użytkownika. Zaczynasz od solidnego zrozumienia hooków i mental modelu komponentów funkcyjnych, następnie przechodzisz do zaawansowanych tematów: Suspense dla danych, React Server Components, transitions oraz zarządzanie stanem serwera z React Query. Sporo czasu poświęcamy architekturze aplikacji — folder feature-based, separacja warstw, testowalność i wzorce kompozycji. Kurs kończy się pełnym projektem aplikacji z routingiem, formularzami, autoryzacją i wdrożeniem. Wszystkie przykłady pisane w TypeScript, zgodnie z aktualnymi best practices społeczności React.",
            "React 18 z TypeScript, React Query, Server Components, testy i architektura feature-based",
            199.00m,
            CourseLevel.Intermediate,
            anna.Id,
            new List<Category> { Cat("frontend") },
            new List<Technology> { Tech("react") });
        AddModules(reactCourse,
            ("Fundamenty React 18", new[]
            {
                ("Hooki od podstaw — useState, useEffect, useMemo", "Zasady działania, pułapki zależności i wzorce wydajności.", 1080),
                ("Kompozycja komponentów i render props", "Tworzenie reużywalnych abstrakcji bez HOC-ów.", 960),
                ("TypeScript dla React — typowanie propsów i hooków", "Generyki, discriminated union i utility types w praktyce.", 1200),
            }),
            ("Stan, formularze i routing", new[]
            {
                ("React Query — stan serwera i cache", "Queries, mutacje, invalidacja i optimistic updates.", 1320),
                ("Formularze z React Hook Form i Zod", "Walidacja schematów, wydajność i obsługa złożonych formularzy.", 1140),
                ("React Router v6 — trasy, loadery i akcje", "Nowy model routingu oparty o dane, lazy routes i nawigacja programistyczna.", 1080),
            }),
            ("Zaawansowane wzorce", new[]
            {
                ("Suspense i granice błędów", "Leniwe ładowanie komponentów i obsługa błędów w drzewie React.", 960),
                ("Server Components i architektura RSC", "Granica klient-serwer, streaming i wybór strategii renderowania.", 1380),
                ("Wzorce globalnego stanu — Zustand vs Redux Toolkit", "Porównanie narzędzi i praktyczne zastosowania.", 1200),
            }),
            ("Testowanie i wdrożenie", new[]
            {
                ("Testing Library i testy komponentów", "Filozofia testów, dobór zapytań i unikanie kruchości.", 1080),
                ("Playwright w testach end-to-end", "Scenariusze użytkownika, trace viewer i CI.", 1200),
                ("Bundle, podział kodu i wdrożenie na Vercel", "Analiza bundle'a, dynamiczne importy i continuous deployment.", 960),
            })
        );
        courses.Add(reactCourse);

        var pythonCourse = NewCourse(
            "Python dla Data Science i Machine Learning",
            "Kompleksowy kurs Pythona skierowany do analityków danych i osób wchodzących w świat uczenia maszynowego. Zaczynasz od solidnych podstaw języka w kontekście pracy z danymi — typy, kolekcje, comprehensions, obsługa plików i wirtualne środowiska. Następnie przechodzisz do ekosystemu NumPy, pandas i Matplotlib, gdzie uczysz się czyścić, transformować i wizualizować dane w sposób powtarzalny. Duża część kursu poświęcona jest klasycznemu uczeniu maszynowemu z scikit-learn: regresja, klasyfikacja, klasteryzacja, walidacja krzyżowa i tuning hiperparametrów. Całość zamyka wprowadzenie do deep learningu z PyTorch — budowa pierwszych sieci neuronowych i trening na GPU. Kurs oparty na rzeczywistych zbiorach danych i case'ach biznesowych.",
            "Python, NumPy, pandas, scikit-learn, wizualizacja danych, klasyczne ML i wprowadzenie do PyTorch",
            249.00m,
            CourseLevel.Intermediate,
            anna.Id,
            new List<Category> { Cat("ai") },
            new List<Technology> { Tech("python") });
        AddModules(pythonCourse,
            ("Python dla analityka", new[]
            {
                ("Środowisko pracy — venv, poetry i Jupyter", "Konfiguracja izolowanych środowisk i powtarzalne projekty analityczne.", 900),
                ("Typy, kolekcje i comprehensions", "Praca z listami, słownikami, zbiorami i generatorami w kontekście danych.", 1080),
                ("Obsługa plików CSV, JSON i baz SQL", "Wczytywanie danych z różnych źródeł i pierwsze podglądy.", 960),
            }),
            ("NumPy i pandas", new[]
            {
                ("NumPy — wektory, macierze i broadcasting", "Operacje zwektoryzowane zamiast pętli i wydajność obliczeń.", 1200),
                ("pandas DataFrame — indeksy, filtrowanie i grupowanie", "Operacje na danych tabelarycznych, metoda chaining i pipe.", 1440),
                ("Czyste dane — braki, duplikaty i normalizacja", "Techniki oczyszczania danych i strategie imputacji.", 1320),
            }),
            ("Wizualizacja i eksploracja", new[]
            {
                ("Matplotlib i Seaborn — wykresy, które mówią prawdę", "Dobór typu wykresu do danych i unikanie wizualnych pułapek.", 1080),
                ("Analiza eksploracyjna (EDA) na realnym datasecie", "Kompletny workflow od pytania biznesowego do wniosków.", 1500),
                ("Raportowanie w Jupyter i Quarto", "Powtarzalne raporty analityczne dla nietechnicznych odbiorców.", 960),
            }),
            ("Machine Learning z scikit-learn", new[]
            {
                ("Podstawy ML — podział danych, overfitting, metryki", "Zrozumienie granic modeli i sposobów ich oceny.", 1260),
                ("Regresja liniowa i regularyzacja", "Modelowanie zależności ciągłych i walka z overfittingiem.", 1320),
                ("Klasyfikacja — drzewa, lasy i gradient boosting", "Modelowanie decyzji, tuning hiperparametrów i interpretacja.", 1500),
            }),
            ("Wprowadzenie do deep learningu", new[]
            {
                ("PyTorch — tensory i autograd", "Podstawy biblioteki i budowa pierwszej sieci neuronowej.", 1380),
                ("Trening sieci na GPU i MLflow", "Śledzenie eksperymentów i reprodukowalność wyników.", 1440),
                ("Wdrożenie modelu jako serwis HTTP", "Eksport modelu, konteneryzacja i minimalne API predykcyjne.", 1200),
            })
        );
        courses.Add(pythonCourse);

        var sqlCourse = NewCourse(
            "SQL od Podstaw do Zaawansowanych Zapytań",
            "Kurs SQL dla osób, które chcą płynnie poruszać się po relacyjnych bazach danych — od pierwszego SELECT po złożone zapytania analityczne. Zaczynasz od modelu relacyjnego, normalizacji i projektowania schematów, następnie przechodzisz do pracy z pojedynczą tabelą: filtrowanie, sortowanie, funkcje agregujące. W kolejnych modułach uczysz się łączyć dane z wielu tabel (JOIN, APPLY, CTE), budować zapytania okienkowe (window functions) i pisać czytelny kod SQL zgodny z konwencjami. Osobny moduł poświęcony jest wydajności — planom wykonania, indeksom, normalizacji i denormalizacji. Całość oparta na PostgreSQL, ale omawiane są też różnice wobec SQL Server i MySQL, dzięki czemu wiedza jest uniwersalna.",
            "Modelowanie danych, JOINy, CTE, funkcje okienkowe, indeksy i optymalizacja zapytań w PostgreSQL",
            149.00m,
            CourseLevel.Beginner,
            marcin.Id,
            new List<Category> { Cat("databases") },
            new List<Technology> { Tech("sql") });
        AddModules(sqlCourse,
            ("Relacyjny model danych", new[]
            {
                ("Czym jest relacja, klucze i ograniczenia", "Podstawy modelowania, primary key, foreign key i integrity constraints.", 960),
                ("Tworzenie schematu i typy danych w PostgreSQL", "DDL, typy, domeny i strategie nazewnictwa.", 1080),
                ("Normalizacja 1NF, 2NF, 3NF i kiedy przestać", "Eliminacja redundancji i praktyczne granice normalizacji.", 1140),
            }),
            ("Język manipulacji danymi", new[]
            {
                ("SELECT, WHERE i ORDER BY", "Filtrowanie, sortowanie i operatory logiczne.", 900),
                ("Funkcje agregujące i GROUP BY", "HAVING, COUNT, SUM, AVG i typowe pułapki.", 1020),
                ("INSERT, UPDATE, DELETE i transakcje", "ACID, BEGIN/COMMIT/ROLLBACK i poziomy izolacji.", 1080),
            }),
            ("Łączenie tabel i podzapytania", new[]
            {
                ("INNER, LEFT, RIGHT, FULL JOIN", "Dobór typu złączenia, warunki ON vs WHERE, anti-join.", 1260),
                ("CTE i rekurencyjne CTE", "Czytelne zapytania z WITH i przetwarzanie drzew hierarchicznych.", 1200),
                ("Podzapytania skorelowane i niepowiązane", "Kiedy lepiej użyć podzapytania, a kiedy CTE lub JOIN.", 1080),
            }),
            ("Funkcje okienkowe i analiza danych", new[]
            {
                ("ROW_NUMBER, RANK, DENSE_RANK", "Numerowanie wierszy, partycje i obsługa remisów.", 1140),
                ("LAG, LEAD i agregacje okienkowe", "Obliczanie różnic, trendów i kroczących sum.", 1320),
                ("PIVOT, UNPIVOT i raportowanie SQL", "Przekształcanie wierszy w kolumny i odwrotnie.", 1200),
            }),
            ("Wydajność i dobre praktyki", new[]
            {
                ("Plany wykonania i EXPLAIN ANALYZE", "Czytanie planów, kosztów i wąskich gardeł.", 1320),
                ("Indeksy — B-tree, GIN, partial index", "Kiedy indeks pomaga, a kiedy szkodzi.", 1200),
                ("Konwencje nazewnictwa i formatowania SQL", "Style guide, code review i utrzymanie kodu SQL.", 900),
            })
        );
        courses.Add(sqlCourse);

        var dockerCourse = NewCourse(
            "Docker i Kubernetes w Środowisku Produkcyjnym",
            "Praktyczny kurs konteneryzacji i orkiestracji dla zespołów, które chcą przenieść swoje aplikacje do środowiska produkcyjnego z pełną powtarzalnością. Zaczynasz od podstaw Dockera — obrazów, warstw, sieci i wolumenów — i szybko przechodzisz do budowania wielostopniowych obrazów dla aplikacji .NET, Node.js i Python. Duża część kursu poświęcona jest Kubernetesowi: od pojedynczego poda, przez Deploymenty i Service, po Ingress, ConfigMapy, Secreti i zarządzanie konfiguracją. Omawiamy też strategie wdrożeń (rolling update, blue/green, canary), autoskalowanie, HPA, VPA oraz cluster autoscaler, a także narzędzia ekosystemu: Helm, Kustomize, Argo CD i observability. Kurs zakończony jest pełnym laboratorium, w którym budujesz klaster produkcyjny w chmurze.",
            "Konteneryzacja, wielostopniowe buildy, Kubernetes, Helm, Argo CD, autoskalowanie i GitOps",
            299.00m,
            CourseLevel.Intermediate,
            marcin.Id,
            new List<Category> { Cat("devops") },
            new List<Technology> { Tech("docker") });
        AddModules(dockerCourse,
            ("Docker od podstaw", new[]
            {
                ("Architektura Dockera i model kontenerowy", "Daemon, klient, registry i przestrzenie nazw w Linuxie.", 960),
                ("Dockerfile — warstwy, cache i optymalizacja", "Budowanie małych, bezpiecznych obrazów i .dockerignore.", 1080),
                ("Sieci, wolumeny i docker compose", "Komunikacja między kontenerami i trwałe dane.", 1200),
            }),
            ("Zaawansowane kontenery", new[]
            {
                ("Wielostopniowe buildy dla .NET i Node", "Minimalne obrazy produkcyjne i warstwa runtime.", 1140),
                ("Bezpieczeństwo obrazów — skanowanie i podpisywanie", "Trivy, cosign, dystrybucja zaufanych obrazów.", 1080),
                ("Registry prywatne i cache warstw", "Harbor, GitHub Container Registry i strategie mirrorów.", 1020),
            }),
            ("Kubernetes — fundamenty", new[]
            {
                ("Architektura klastra i obiekty API", "Control plane, węzły, API server i reconciler.", 1200),
                ("Pod, Deployment, Service i Ingress", "Podstawowe prymitywy i ekspozycja aplikacji na zewnątrz.", 1320),
                ("Konfiguracja i sekrety", "ConfigMap, Secret, zmienne środowiskowe i montowanie plików.", 1140),
            }),
            ("Orkiestracja i strategie wdrożeń", new[]
            {
                ("Helm i Kustomize — paczkowanie aplikacji", "Templates, values, overlays i strategie rozwoju.", 1260),
                ("Rolling update, blue/green i canary", "Bezpieczne wdrożenia bez przestoju i szybki rollback.", 1320),
                ("Autoskalowanie — HPA, VPA i cluster autoscaler", "Dynamiczne dopasowanie zasobów do obciążenia.", 1200),
            }),
            ("GitOps i observability", new[]
            {
                ("Argo CD i declarative deployments", "Ciągła synchronizacja klastra z repozytorium Git.", 1320),
                ("Monitoring z Prometheus i Grafana", "Metryki, alerty i ServiceMonitor.", 1260),
                ("Centralne logi i tracing — Loki, Tempo, OpenTelemetry", "Diagnostyka rozproszonych systemów produkcyjnych.", 1440),
            })
        );
        courses.Add(dockerCourse);

        var awsCourse = NewCourse(
            "AWS Solutions Architect — Kompletne Przygotowanie",
            "Kurs przygotowujący do certyfikacji AWS Certified Solutions Architect (Associate) i jednocześnie budujący praktyczne kompetencje projektowania systemów w chmurze Amazon Web Services. Zaczynasz od modelu odpowiedzialności shared responsibility, globalnej infrastruktury AWS i kont AWS, następnie przechodzisz do kluczowych usług obliczeniowych (EC2, Lambda, ECS, EKS), storage (S3, EBS, FSx, Glacier), baz danych (RDS, DynamoDB, Aurora) i sieci (VPC, Route 53, CloudFront, Direct Connect). Duży nacisk położony jest na well-architected framework — projektowanie systemów bezpiecznych, odpornych, wydajnych i optymalnych kosztowo. Kurs zawiera liczne laboratoria praktyczne, a także realistyczne scenariusze architektoniczne omawiane krok po kroku.",
            "EC2, S3, VPC, RDS, Lambda, ECS i projektowanie systemów zgodne z well-architected framework",
            349.00m,
            CourseLevel.Intermediate,
            marcin.Id,
            new List<Category> { Cat("devops") },
            new List<Technology> { Tech("aws") });
        AddModules(awsCourse,
            ("Fundamenty AWS", new[]
            {
                ("Globalna infrastruktura i regiony", "Availability Zones, edge locations i projektowanie dla wysokiej dostępności.", 960),
                ("IAM — tożsamość, role i polityki", "Zasada najmniejszych uprawnień i cross-account access.", 1200),
                ("Model odpowiedzialności shared responsibility", "Co odpowiada AWS, a co Ty jako użytkownik chmury.", 900),
            }),
            ("Obliczenia i storage", new[]
            {
                ("EC2 — instancje, AMI, autoscaling grupy", "Dobór typu instancji, placement groups i lifecycle.", 1320),
                ("Lambda i architektura serverless", "Event-driven design, cold starty i integracje z innymi usługami.", 1260),
                ("S3 — klasy storage, polityki, lifecycle", "Optymalizacja kosztów i wzorce dostępu do danych.", 1140),
            }),
            ("Sieć i dostarczanie treści", new[]
            {
                ("VPC — podsieci, route tables, NAT i NACL", "Projektowanie izolowanej sieci w chmurze.", 1380),
                ("Load Balancery i Route 53", "ALB, NLB, GLB i strategie DNS.", 1200),
                ("CloudFront i edge caching", "CDN, podpisywanie URL-i i origin shield.", 1080),
            }),
            ("Bazy danych w AWS", new[]
            {
                ("RDS i Aurora — wysoka dostępność i repliki", "Multi-AZ, read replicas i automatyczny failover.", 1320),
                ("DynamoDB — modelowanie i indeksy", "Single-table design, GSI/LSI i adaptive capacity.", 1380),
                ("ElastiCache i strategie cache", "Redis vs Memcached i wzorce cache-aside.", 1080),
            }),
            ("Bezpieczeństwo i automatyzacja", new[]
            {
                ("KMS, Secrets Manager i szyfrowanie danych", "Klucze, rotacja i polityki dostępu.", 1200),
                ("CloudFormation i CDK", "Infrastructure as code w AWS.", 1320),
                ("Well-architected framework w praktyce", "Przegląd pięciu filarów i checklisty.", 1260),
            }),
            ("Scenariusze egzaminacyjne i laboratoria", new[]
            {
                ("Projektowanie systemu multi-tier", "Kompletny przypadek z wymaganiami HA i RPO/RTO.", 1440),
                ("Optymalizacja kosztów i tagging", "Cost Explorer, Budgets i strategie oszczędności.", 1080),
                ("Symulacja egzaminu i omówienie pytań", "100 pytań w stylu SAA-C03 z omówieniem.", 1500),
            })
        );
        courses.Add(awsCourse);

        var mongoCourse = NewCourse(
            "MongoDB — Agregacje i Modelowanie Danych",
            "Kurs dla programistów i analityków, którzy pracują z MongoDB i chcą świadomie projektować schematy dokumentowe oraz pisać zaawansowane zapytania agregacyjne. Zaczynasz od modelowania danych — denormalizacja, embedding vs referencing, reguły projektowania dla różnych wzorców dostępu. Następnie przechodzisz do pipeline'ów agregacyjnych: od prostych $match i $group, po $lookup, $facet, $bucket i $graphLookup. Sporo czasu poświęcamy indeksom (single field, compound, partial, TTL), planom wykonania (explain) i diagnostyce wolnych zapytań. Omawiamy też transakcje wielodokumentowe, change streams i wzorce event sourcing. Kurs oparty na MongoDB 7, ale wiedza jest w dużej mierze przenoszalna na wcześniejsze wersje.",
            "Modelowanie dokumentowe, pipeline'y agregacyjne, indeksy, transakcje i change streams",
            199.00m,
            CourseLevel.Intermediate,
            anna.Id,
            new List<Category> { Cat("databases") },
            new List<Technology> { Tech("mongodb") });
        AddModules(mongoCourse,
            ("Modelowanie danych w MongoDB", new[]
            {
                ("Dokument, kolekcja i BSON", "Anatomia dokumentu, typy danych i ograniczenia BSON.", 960),
                ("Embedding vs referencing — kiedy co wybrać", "Wzorce projektowe dla relacji jeden-do-wielu i wiele-do-wielu.", 1200),
                ("Schema validation i wzorce migracji", "JSON Schema, versioning danych i ewolucja schematu.", 1080),
            }),
            ("Podstawy zapytań i CRUD", new[]
            {
                ("find, projekcje i operatory", "Filtrowanie złożonych struktur i projekcje elementów.", 1080),
                ("Aktualizacje — updateOne, array operators", "Modyfikacje dokumentów, $set, $push, $pull i $elemMatch.", 1140),
                ("Bulk operations i writeConcern", "Wydajny zapis dużych wolumenów danych.", 1020),
            }),
            ("Pipeline'y agregacyjne", new[]
            {
                ("$match, $group, $project i $sort", "Podstawowe etapy pipeline i kolejność operatorów.", 1260),
                ("$lookup i $unwind", "Łączenie kolekcji i praca z tablicami dokumentów.", 1320),
                ("$facet, $bucket i $graphLookup", "Zaawansowane analizy i przetwarzanie grafów.", 1380),
            }),
            ("Wydajność i produkcja", new[]
            {
                ("Indeksy — single, compound, partial, TTL", "Dobór indeksów pod wzorce zapytań.", 1200),
                ("explain() i profilowanie zapytań", "Czytanie planu wykonania i optymalizacja.", 1320),
                ("Change streams i event-driven architecture", "Reagowanie na zmiany w czasie rzeczywistym.", 1260),
            })
        );
        courses.Add(mongoCourse);

        var llmCourse = NewCourse(
            "Aplikacje z Dużymi Modelami Językowymi (LLM)",
            "Kurs dla programistów, którzy chcą budować produkcyjne aplikacje oparte o duże modele językowe. Zaczynasz od zrozumienia architektury transformerów i różnic między modelami open-source (LLaMA, Mistral, Qwen) a komercyjnymi API (OpenAI, Anthropic, Google). Następnie przechodzisz do prompt engineeringu, retrieval-augmented generation (RAG), agentów i wzorców orkiestracji (LangChain, LlamaIndex, Semantic Kernel). Duży nacisk położony jest na inżynierię oprogramowania: ewaluacja modeli, observability, koszty, bezpieczeństwo i ochrona przed atakami (prompt injection, jailbreak). Całość zakończona budową własnego chatbota dokumentowego z wektorową bazą danych, guardrailsami i deploymentem w chmurze.",
            "LLM, RAG, agenci, LangChain, ewaluacja modeli, bezpieczeństwo i deployment aplikacji AI",
            349.00m,
            CourseLevel.Advanced,
            john.Id,
            new List<Category> { Cat("ai") },
            new List<Technology> { Tech("python") });
        AddModules(llmCourse,
            ("Podstawy LLM", new[]
            {
                ("Architektura transformerów — intuicja", "Self-attention, tokenizacja i emergent abilities.", 1080),
                ("Przegląd modeli open-source i komercyjnych", "LLaMA, Mistral, GPT-4o, Claude — kiedy co wybrać.", 1140),
                ("Pierwsze wywołania API i streaming", "OpenAI, Anthropic i self-hosting z vLLM/Ollama.", 1260),
            }),
            ("Prompt engineering i RAG", new[]
            {
                ("Techniki promptowania — few-shot, chain-of-thought", "Strategie budowania niezawodnych promptów.", 1080),
                ("Embeddings i bazy wektorowe", "pgvector, Qdrant, Pinecone i dobór modelu embeddingów.", 1320),
                ("Retrieval-Augmented Generation w praktyce", "Chunking, hybrydowe wyszukiwanie i re-ranking.", 1380),
            }),
            ("Agenci i orkiestracja", new[]
            {
                ("Function calling i tool use", "Modele wywołujące zewnętrzne API i obsługa błędów.", 1200),
                ("LangChain i LlamaIndex — porównanie", "Dobór frameworka i wzorce kompozycji agentów.", 1320),
                ("Wielokrokowe agenty i pamięć", "Planowanie zadań, pamięć krótko- i długoterminowa.", 1260),
            }),
            ("Ewaluacja, bezpieczeństwo i koszty", new[]
            {
                ("Ewaluacja modeli i promptów", "Zbiory testowe, metryki i A/B testing LLM.", 1260),
                ("Bezpieczeństwo — prompt injection i guardrails", "NeMo Guardrails, Llama Guard i wzorce obrony.", 1200),
                ("Optymalizacja kosztów i cache", "Prompt caching, batching i dobór modeli do zadań.", 1080),
            }),
            ("Wdrożenie i MLOps dla LLM", new[]
            {
                ("Observability i tracing aplikacji LLM", "LangSmith, Phoenix i monitorowanie jakości odpowiedzi.", 1200),
                ("Fine-tuning i adaptery LoRA", "Dostosowanie modelu do domeny bez pełnego retrainingu.", 1320),
                ("Deployment — konteneryzacja i skalowanie", "Kubernetes, autoskalowanie GPU i architektura event-driven.", 1380),
            })
        );
        courses.Add(llmCourse);

        var laravelCourse = NewCourse(
            "Laravel 11 — Nowoczesne API w PHP",
            "Kurs dla programistów PHP, którzy chcą budować nowoczesne, testowalne i bezpieczne API w Laravel 11. Zaczynasz od konfiguracji projektu, kontenera IoC i architektury hexagonalnej wewnątrz Laravela. Następnie przechodzisz do wzorca Repository, serwisów i warstwy DTO, formularzy z Form Requests i walidacji. Duży nacisk położony jest na testowanie — PHPUnit, Pest, testy integracyjne z HTTP i testy end-to-end z Laravel Dusk. Osobny moduł poświęcony jest autoryzacji z Sanctum i Passport, politykom Gates oraz rolom. Kurs zamyka budowa kompletnego API dla aplikacji e-commerce, z paginacją, filtrami, kolejkami, eventami i deploymentem w kontenerze.",
            "Laravel 11, wzorzec Repository, testy Pest, Sanctum, kolejki i deployment w Dockerze",
            199.00m,
            CourseLevel.Intermediate,
            john.Id,
            new List<Category> { Cat("backend") },
            new List<Technology> { Tech("laravel") });
        AddModules(laravelCourse,
            ("Fundamenty Laravel 11", new[]
            {
                ("Instalacja i struktura projektu", "Composer, .env, konwencje katalogów i nowy streamlined skeleton.", 960),
                ("Kontener IoC i Service Provider", "Wstrzykiwanie zależności i rejestracja serwisów.", 1080),
                ("Routing, middleware i kontrolery", "RESTful API, grupowanie tras i parametry.", 1140),
            }),
            ("Model i warstwa danych", new[]
            {
                ("Eloquent ORM i migracje", "Definicja modeli, relacji i migracji bazy danych.", 1260),
                ("Wzorzec Repository i DTO", "Oddzielenie warstwy danych i przenoszenie obiektów.", 1200),
                ("Form Requests i walidacja", "Walidacja regułami, autoryzacja i komunikaty błędów.", 1080),
            }),
            ("Autoryzacja i bezpieczeństwo", new[]
            {
                ("Laravel Sanctum — tokeny API", "Bearer tokens, abilities i SPA authentication.", 1140),
                ("Polityki, Gates i role", "Resource-based authorization i integracja z kontrolerami.", 1200),
                ("Ochrona przed atakami — CSRF, XSS, rate limiting", "Bezpieczeństwo API i hardening.", 1080),
            }),
            ("Testowanie i jakość kodu", new[]
            {
                ("PHPUnit i Pest — testy jednostkowe", "Wzorce AAA, mockowanie i testy parametryzowane.", 1140),
                ("Testy integracyjne HTTP", "Symfony test client dla Laravela i asercje odpowiedzi.", 1260),
                ("Laravel Dusk i testy E2E", "Browser automation i scenariusze użytkownika.", 1200),
            }),
            ("Kolejki, eventy i deployment", new[]
            {
                ("Kolejki i joby — Redis, Horizon", "Przetwarzanie w tle i observability z Horizon.", 1080),
                ("Eventy, listenery i broadcasting", "Architektura event-driven i WebSockets.", 1140),
                ("Docker, CI/CD i deployment produkcyjny", "Wielostopniowy Dockerfile, GitHub Actions i Laravel Forge.", 1320),
            })
        );
        courses.Add(laravelCourse);

        var angularCourse = NewCourse(
            "Angular dla Aplikacji Enterprise",
            "Kurs dla programistów, którzy znają już podstawy TypeScriptu i chcą budować duże, długowieczne aplikacje webowe w Angular. Zaczynasz od architektury modułowej, lazy loadingu i strategii rozdziału kodu, następnie przechodzisz do reaktywnego RxJS — strumienie, operatory, Subjecty i wzorce kompozycji. Duża część kursu poświęcona jest zarządzaniu stanem z NgRx (Store, Effects, Selectors) oraz Signals — nowemu reaktywnemu modelowi Angulara. Omawiamy też formularze reaktywne, autoryzację z JWT i OAuth2, internacjonalizację (i18n), testy (Jest, Cypress) i wzorce utrzymania dużych aplikacji. Całość oparta na Angular 18+ z standalone components i nowym kontrolerem przepływu.",
            "Angular 18+, RxJS, NgRx, Signals, formularze reaktywne, testy i architektura enterprise",
            299.00m,
            CourseLevel.Advanced,
            anna.Id,
            new List<Category> { Cat("frontend") },
            new List<Technology> { Tech("angular") });
        AddModules(angularCourse,
            ("Architektura Angular", new[]
            {
                ("Standalone components i nowy control flow", "Anatomia aplikacji Angular 18+ i signals.", 1080),
                ("Moduły, lazy loading i feature folders", "Strategie podziału aplikacji i code splitting.", 1200),
                ("Dependency Injection — providers i inject()", "Zaawansowane wzorce DI i hierarchical injectors.", 1140),
            }),
            ("RxJS i programowanie reaktywne", new[]
            {
                ("Observable, Subject i BehaviorSubject", "Strumienie danych i ich typy.", 1080),
                ("Operatory — map, switchMap, combineLatest", "Kompozycja strumieni i wzorce anulowania.", 1320),
                ("Signals i interop z RxJS", "Nowy model reaktywny Angulara i integracja z observables.", 1260),
            }),
            ("NgRx — zarządzanie stanem", new[]
            {
                ("Store, akcje i reduktory", "Implementacja centralnego stanu aplikacji.", 1260),
                ("Effects i selectors", "Obsługa efektów ubocznych i memoizacja selektorów.", 1200),
                ("NgRx Signals i nowe API", "Signal Store jako alternatywa dla klasycznego Store.", 1320),
            }),
            ("Formularze, routing i autoryzacja", new[]
            {
                ("Formularze reaktywne i walidacja", "FormBuilder, walidatory synchroniczne i asynchroniczne.", 1140),
                ("Routing zaawansowany — strażnicy i resolvery", "CanActivate, CanMatch i wstępne ładowanie danych.", 1080),
                ("Autoryzacja z interceptorami i OAuth2", "JWT, refresh tokens i integracja z identity provider.", 1320),
            }),
            ("Testowanie i utrzymanie", new[]
            {
                ("Testy jednostkowe z Jest", "Testowanie komponentów, serwisów i pipe'ów.", 1080),
                ("Testy E2E z Cypress i Playwright", "Scenariusze integracyjne i debugowanie.", 1200),
                ("i18n, a11y i wzorce dużych aplikacji", "Internacjonalizacja, dostępność i utrzymanie kodu.", 1140),
            })
        );
        courses.Add(angularCourse);

        var mlIntroCourse = NewCourse(
            "Wprowadzenie do Machine Learning z scikit-learn",
            "Kurs dla osób, które dopiero zaczynają przygodę z uczeniem maszynowym i chcą zbudować solidne fundamenty bez konieczności zagłębiania się w matematykę wyższą od razu. Zaczynasz od intuicyjnego zrozumienia, czym jest model, overfitting i podział danych, następnie przechodzisz do pierwszych eksperymentów z regresją liniową i logistyczną. W kolejnych modułach uczysz się klasycznych algorytmów (drzewa decyzyjne, k-nearest neighbors, SVM), oceny modeli (precision, recall, F1, ROC AUC) i technik feature engineeringu. Całość zakończona budową kompletnego projektu ML — od problemu biznesowego, przez eksplorację danych, po deployment modelu jako serwisu HTTP. Kurs oparty na Pythonie, pandas i scikit-learn.",
            "Podstawy ML, regresja, klasyfikacja, feature engineering, ewaluacja i pierwszy projekt end-to-end",
            149.00m,
            CourseLevel.Beginner,
            anna.Id,
            new List<Category> { Cat("ai") },
            new List<Technology> { Tech("python") });
        AddModules(mlIntroCourse,
            ("Czym jest uczenie maszynowe", new[]
            {
                ("Definicja, typy ML i zastosowania", "Nadzorowane, nienadzorowane i uczenie ze wzmocnieniem — przykłady.", 960),
                ("Środowisko pracy — Jupyter, scikit-learn, pandas", "Konfiguracja i pierwszy notebook analityczny.", 900),
                ("Podział danych — train, validation, test", "Strategie podziału i zapobieganie wyciekowi danych.", 1080),
            }),
            ("Regresja", new[]
            {
                ("Regresja liniowa — prosta i wielokrotna", "Model, współczynniki i interpretacja wyników.", 1080),
                ("Metryki regresji — MAE, MSE, R²", "Dobór metryki do problemu biznesowego.", 900),
                ("Regularyzacja L1 i L2", "Walka z overfittingiem i selekcja cech.", 1080),
            }),
            ("Klasyfikacja", new[]
            {
                ("Regresja logistyczna — podstawy", "Model probabilistyczny i granica decyzyjna.", 1080),
                ("Drzewa decyzyjne i lasy losowe", "Intuicja, hiperparametry i interpretacja.", 1200),
                ("KNN i SVM — kiedy warto po nie sięgnąć", "Porównanie algorytmów i scenariusze użycia.", 1140),
            }),
            ("Ocena modeli i feature engineering", new[]
            {
                ("Precision, recall, F1 i macierz pomyłek", "Metryki klasyfikacji i ich interpretacja.", 1080),
                ("Krzywa ROC i AUC", "Dobór progu decyzyjnego i porównanie modeli.", 1020),
                ("Feature engineering i preprocessing", "Skalowanie, kodowanie kategorii i selekcja cech.", 1260),
            }),
            ("Projekt end-to-end", new[]
            {
                ("Definicja problemu i eksploracja danych", "Od pytania biznesowego do planu eksperymentów.", 1080),
                ("Trening, walidacja i tuning hiperparametrów", "Grid search, random search i cross-validation.", 1320),
                ("Deployment modelu jako API w FastAPI", "Eksport modelu, konteneryzacja i serwis predykcyjny.", 1320),
            })
        );
        courses.Add(mlIntroCourse);

        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();
    }

    private static Course NewCourse(
        string title,
        string description,
        string shortDescription,
        decimal price,
        CourseLevel level,
        Guid instructorId,
        List<Category> categories,
        List<Technology> technologies)
    {
        return new Course
        {
            Title = title,
            Description = description,
            ShortDescription = shortDescription,
            Price = price,
            Level = level,
            Status = CourseStatus.Published,
            ThumbnailObjectKey = string.Empty,
            Language = "Polski",
            InstructorId = instructorId,
            Categories = categories,
            Technologies = technologies,
            Modules = new List<Module>()
        };
    }

    private static void AddModules(Course course, params (string Title, (string Title, string Description, int DurationSeconds)[] Lessons)[] modules)
    {
        for (var i = 0; i < modules.Length; i++)
        {
            var (title, lessonData) = modules[i];
            var module = new Module
            {
                Title = title,
                Order = i + 1,
                CourseId = course.Id,
                Lessons = new List<Lesson>()
            };

            for (var j = 0; j < lessonData.Length; j++)
            {
                var (lessonTitle, lessonDescription, durationSeconds) = lessonData[j];
                module.Lessons.Add(new Lesson
                {
                    Title = lessonTitle,
                    Description = lessonDescription,
                    VideoObjectKey = $"placeholder/videos/{course.Id:N}-{i + 1}-{j + 1}.mp4",
                    Duration = durationSeconds,
                    Order = j + 1,
                    ModuleId = module.Id,
                });
            }

            course.Modules.Add(module);
        }
    }

    private static async Task SeedEnrollmentsAsync(ApplicationDbContext context)
    {
        if (await context.Enrollments.AnyAsync()) return;

        var students = await context.Users
            .Where(u => u.Email!.EndsWith("@courseplatform.com") &&
                        (u.Email.StartsWith("piotr.") || u.Email.StartsWith("karolina.") ||
                         u.Email.StartsWith("tomasz.") || u.Email.StartsWith("monika.") ||
                         u.Email.StartsWith("jakub.")))
            .ToListAsync();

        var courses = await context.Courses.ToListAsync();

        var studentCourseMatrix = new (string StudentEmail, string[] CourseTitles)[]
        {
            ("piotr.nowak@courseplatform.com", new[]
            {
                "Vue 3 Fundamentals — Composition API i TypeScript",
                "Zaawansowane .NET 9 Web API — Clean Architecture i CQRS",
                "Docker i Kubernetes w Środowisku Produkcyjnym",
                "SQL od Podstaw do Zaawansowanych Zapytań",
                "Wprowadzenie do Machine Learning z scikit-learn",
            }),
            ("karolina.zielinska@courseplatform.com", new[]
            {
                "React 18 w Praktyce — Hooks, Suspense i Server Components",
                "Angular dla Aplikacji Enterprise",
                "Python dla Data Science i Machine Learning",
                "AWS Solutions Architect — Kompletne Przygotowanie",
                "MongoDB — Agregacje i Modelowanie Danych",
            }),
            ("tomasz.wojcik@courseplatform.com", new[]
            {
                "Zaawansowane .NET 9 Web API — Clean Architecture i CQRS",
                "Laravel 11 — Nowoczesne API w PHP",
                "SQL od Podstaw do Zaawansowanych Zapytań",
                "Docker i Kubernetes w Środowisku Produkcyjnym",
            }),
            ("monika.kaminska@courseplatform.com", new[]
            {
                "Vue 3 Fundamentals — Composition API i TypeScript",
                "React 18 w Praktyce — Hooks, Suspense i Server Components",
                "Python dla Data Science i Machine Learning",
                "Aplikacje z Dużymi Modelami Językowymi (LLM)",
                "Wprowadzenie do Machine Learning z scikit-learn",
                "MongoDB — Agregacje i Modelowanie Danych",
            }),
            ("jakub.lewandowski@courseplatform.com", new[]
            {
                "Angular dla Aplikacji Enterprise",
                "Laravel 11 — Nowoczesne API w PHP",
                "AWS Solutions Architect — Kompletne Przygotowanie",
                "Aplikacje z Dużymi Modelami Językowymi (LLM)",
                "Docker i Kubernetes w Środowisku Produkcyjnym",
            }),
        };

        var now = DateTime.UtcNow;
        foreach (var (email, courseTitles) in studentCourseMatrix)
        {
            var student = students.FirstOrDefault(s => s.Email == email);
            if (student == null) continue;
            for (var i = 0; i < courseTitles.Length; i++)
            {
                var course = courses.FirstOrDefault(c => c.Title == courseTitles[i]);
                if (course == null) continue;
                context.Enrollments.Add(new Enrollment
                {
                    UserId = student.Id,
                    CourseId = course.Id,
                    EnrolledAt = now.AddDays(-(60 - i * 7L))
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedReviewsAsync(ApplicationDbContext context)
    {
        if (await context.Reviews.AnyAsync()) return;

        var students = await context.Users
            .Where(u => u.Email!.EndsWith("@courseplatform.com") &&
                        (u.Email.StartsWith("piotr.") || u.Email.StartsWith("karolina.") ||
                         u.Email.StartsWith("tomasz.") || u.Email.StartsWith("monika.") ||
                         u.Email.StartsWith("jakub.")))
            .ToListAsync();

        var courses = await context.Courses.ToListAsync();

        var reviewData = new (string CourseTitle, (string StudentEmail, int Rating, string Comment)[] Reviews)[]
        {
            ("Vue 3 Fundamentals — Composition API i TypeScript", new[]
            {
                ("piotr.nowak@courseplatform.com", 5, "Świetny kurs — w końcu zrozumiałem różnicę między Options API a Composition API. Lekcje o composables są złotem."),
                ("monika.kaminska@courseplatform.com", 5, "Bardzo dobrze przygotowane ćwiczenia, każdy moduł kończy się praktycznym projektem. Polecam osobom, które znają JS, ale dopiero zaczynają z Vue."),
                ("jakub.lewandowski@courseplatform.com", 4, "Solidny kurs, choć momentami tempo jest dość szybkie. Moduł o Pinia i Vue Router wyjaśnia wszystko krok po kroku."),
            }),
            ("Zaawansowane .NET 9 Web API — Clean Architecture i CQRS", new[]
            {
                ("piotr.nowak@courseplatform.com", 5, "Najlepszy kurs o Clean Architecture i CQRS, jaki przeszedłem. Testy integracyjne z WebApplicationFactory są świetnie omówione."),
                ("tomasz.wojcik@courseplatform.com", 5, "Kurs zmienił sposób, w jaki buduję API w .NET. Moduł o OpenTelemetry i observability jest wyjątkowo wartościowy."),
                ("karolina.zielinska@courseplatform.com", 4, "Wiedza na najwyższym poziomie, ale wymaga już solidnego doświadczenia w C#. Dla juniora może być za dużo naraz."),
            }),
            ("React 18 w Praktyce — Hooks, Suspense i Server Components", new[]
            {
                ("karolina.zielinska@courseplatform.com", 5, "Doskonałe wprowadzenie do React Server Components. Po kursie w końcu rozumiem, kiedy używać 'use client', a kiedy nie."),
                ("monika.kaminska@courseplatform.com", 5, "React Query zostało omówione wyczerpująco. Optymistyczne aktualizacje i invalidacja — wszystko jasne."),
                ("jakub.lewandowski@courseplatform.com", 4, "Bardzo dobry kurs. Brakuje mi może więcej materiału o Server Actions, ale ogólnie solidny punkt wyjścia."),
            }),
            ("Python dla Data Science i Machine Learning", new[]
            {
                ("karolina.zielinska@courseplatform.com", 5, "Kompletny kurs od podstaw po zaawansowane techniki. Moduł o feature engineering i czyszczeniu danych jest bezcenny."),
                ("monika.kaminska@courseplatform.com", 5, "Pandas i NumPy świetnie wyjaśnione. EDA na realnym datasecie to najlepsza część kursu."),
                ("piotr.nowak@courseplatform.com", 4, "Bardzo dobry kurs, chociaż moduł o PyTorch mógłby być nieco dłuższy. Mimo to wiedza przekazana w sposób bardzo przystępny."),
            }),
            ("SQL od Podstaw do Zaawansowanych Zapytań", new[]
            {
                ("tomasz.wojcik@courseplatform.com", 5, "W końcu ktoś wytłumaczył funkcje okienkowe w sposób, który ma sens. Polecam każdemu, kto pracuje z relacyjnymi bazami."),
                ("piotr.nowak@courseplatform.com", 5, "Świetny kurs zarówno dla początkujących, jak i dla osób, które chcą uporządkować wiedzę. Moduł o EXPLAIN ANALYZE bezcenny."),
                ("jakub.lewandowski@courseplatform.com", 4, "Dobrze zbudowany kurs, dużo praktycznych przykładów. Przydałby się osobny moduł o optymalizacji schematu."),
            }),
            ("Docker i Kubernetes w Środowisku Produkcyjnym", new[]
            {
                ("piotr.nowak@courseplatform.com", 5, "Kurs dał mi pewność w deployowaniu aplikacji produkcyjnych. Argo CD i GitOps omówione wzorcowo."),
                ("tomasz.wojcik@courseplatform.com", 5, "Najlepszy kurs o Kubernetes, jaki przeszedłem. Moduł o strategiach wdrożeń (canary, blue/green) praktycznie wdrożyłem w pracy następnego dnia."),
                ("jakub.lewandowski@courseplatform.com", 5, "Od zera do produkcyjnego klastra w jednym kursie. Sekcja o Helm i Kustomize jest świetna."),
                ("monika.kaminska@courseplatform.com", 4, "Wymaga doświadczenia z Linuxem, ale w zamian daje bardzo praktyczną wiedzę. Observability zostało omówione wyczerpująco."),
            }),
            ("AWS Solutions Architect — Kompletne Przygotowanie", new[]
            {
                ("karolina.zielinska@courseplatform.com", 5, "Zdałam SAA-C03 po tym kursie za pierwszym razem. Scenariusze egzaminacyjne są świetnie przygotowane."),
                ("jakub.lewandowski@courseplatform.com", 5, "Bardzo praktyczne podejście — nie tylko do egzaminu, ale też do realnego projektowania systemów w AWS."),
                ("tomasz.wojcik@courseplatform.com", 4, "Solidne przygotowanie do certyfikacji. Moduł o VPC jest wyjątkowo dobrze wytłumaczony."),
            }),
            ("MongoDB — Agregacje i Modelowanie Danych", new[]
            {
                ("karolina.zielinska@courseplatform.com", 5, "Embedding vs referencing w końcu jasne. Pipeline'y agregacyjne pokazane krok po kroku na realnych przykładach."),
                ("monika.kaminska@courseplatform.com", 4, "Świetny kurs, ale przydałoby się więcej o strategiach shardingu. Reszta materiału wyśmienita."),
                ("piotr.nowak@courseplatform.com", 5, "explain() i indeksy omówione tak, jak powinno być od dawna. Polecam każdemu, kto pracuje z MongoDB."),
            }),
            ("Aplikacje z Dużymi Modelami Językowymi (LLM)", new[]
            {
                ("monika.kaminska@courseplatform.com", 5, "Najbardziej aktualny i praktyczny kurs o LLM, jaki znalazłem. Sekcja o bezpieczeństwie i prompt injection bezcenna."),
                ("jakub.lewandowski@courseplatform.com", 5, "Budowa chatbota dokumentowego w ostatnim module to świetne zwieńczenie. Wdrożyłem podobne rozwiązanie w firmie."),
                ("piotr.nowak@courseplatform.com", 4, "Wymaga doświadczenia w Pythonie i konteneryzacji, ale daje ogromną wartość. Moduł o RAG bardzo dobrze przygotowany."),
            }),
            ("Laravel 11 — Nowoczesne API w PHP", new[]
            {
                ("tomasz.wojcik@courseplatform.com", 5, "W końcu kurs Laravela, który nie traktuje mnie jak juniora. Wzorzec Repository i testy Pest wyjaśnione wzorcowo."),
                ("jakub.lewandowski@courseplatform.com", 4, "Solidny kurs. Deployment z Dockerem i GitHub Actions bardzo praktyczny, choć przydałoby się więcej o Laravel Forge."),
                ("piotr.nowak@courseplatform.com", 5, "Sanctum i autoryzacja w końcu zrozumiałe. Kurs wart swojej ceny."),
            }),
            ("Angular dla Aplikacji Enterprise", new[]
            {
                ("karolina.zielinska@courseplatform.com", 5, "NgRx i Signals w jednym kursie — dokładnie tego brakowało na rynku. Standalone components świetnie wyjaśnione."),
                ("jakub.lewandowski@courseplatform.com", 5, "Dużo wzorców architektonicznych, które od razu można zastosować w prawdziwym projekcie. Polecam seniorom Angulara."),
                ("monika.kaminska@courseplatform.com", 4, "Wymaga solidnego doświadczenia w TypeScript i RxJS. RxJS w module 2 jest wyśmienity."),
            }),
            ("Wprowadzenie do Machine Learning z scikit-learn", new[]
            {
                ("piotr.nowak@courseplatform.com", 5, "Idealny kurs na start. Wszystko wyjaśnione bez nadmiernej matematyki, ale z zachowaniem głębi merytorycznej."),
                ("monika.kaminska@courseplatform.com", 5, "Projekt end-to-end w ostatnim module to majstersztyk. Wdrożyłam podobny flow w swoim zespole."),
                ("jakub.lewandowski@courseplatform.com", 4, "Świetny punkt wejścia w ML. Po kursie mogłem już swobodnie czytać bardziej zaawansowane materiały."),
            }),
        };

        var now = DateTime.UtcNow;
        foreach (var (courseTitle, reviews) in reviewData)
        {
            var course = courses.FirstOrDefault(c => c.Title == courseTitle);
            if (course == null) continue;
            for (var i = 0; i < reviews.Length; i++)
            {
                var (email, rating, comment) = reviews[i];
                var student = students.FirstOrDefault(s => s.Email == email);
                if (student == null) continue;
                context.Reviews.Add(new Review
                {
                    CourseId = course.Id,
                    UserId = student.Id,
                    Rating = rating,
                    Comment = comment,
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedLearningPathsAsync(ApplicationDbContext context)
    {
        if (await context.LearningPaths.AnyAsync()) return;

        var courses = await context.Courses.ToListAsync();

        Course FindCourse(string title) => courses.First(c => c.Title == title);

        var backendPath = new LearningPath
        {
            Title = "Backend Developer w .NET",
            Slug = "backend-developer-dotnet",
            ShortDescription = "Zostań backend developerem — od podstaw API do zaawansowanych wzorców w ekosystemie .NET.",
            Description = "Ścieżka dla osób, które chcą budować solidne systemy serwerowe w ekosystemie Microsoft. Zaczynasz od zaawansowanego kursu ASP.NET Core 9 i Clean Architecture, następnie uczysz się konteneryzacji z Dockerem i Kubernetesem, by wreszcie poznać SQL i wzorce modelowania relacyjnego. Na końcu ścieżki potrafisz samodzielnie zaprojektować, zaimplementować, przetestować i wdrożyć produkcyjne API.",
            DifficultyLevel = PathDifficultyLevel.Intermediate,
            EstimatedHours = 95,
            ThumbnailUrl = "https://placeholder.local/paths/backend-dotnet.jpg",
            DisplayOrder = 1,
            IsPublished = true,
            PathCourses = new List<LearningPathCourse>
            {
                new() { CourseId = FindCourse("Zaawansowane .NET 9 Web API — Clean Architecture i CQRS").Id, Order = 1, IsOptional = false },
                new() { CourseId = FindCourse("SQL od Podstaw do Zaawansowanych Zapytań").Id, Order = 2, IsOptional = false },
                new() { CourseId = FindCourse("Docker i Kubernetes w Środowisku Produkcyjnym").Id, Order = 3, IsOptional = false },
            }
        };

        var frontendPath = new LearningPath
        {
            Title = "Frontend Developer — Vue i React",
            Slug = "frontend-developer-vue-react",
            ShortDescription = "Naucz się budować nowoczesne interfejsy webowe w najpopularniejszych frameworkach.",
            Description = "Ścieżka dla osób stawiających pierwsze kroki w frontendzie lub chcących poszerzyć kompetencje o kolejny framework. Zaczynasz od solidnych podstaw Vue 3 z Composition API i TypeScriptem, następnie przechodzisz do Reacta 18 z Server Components i React Query. Całość uzupełnia kurs o Angulary dla systemów enterprise. Po ukończeniu ścieżki swobodnie poruszasz się w trzech najważniejszych frameworkach JavaScript.",
            DifficultyLevel = PathDifficultyLevel.Beginner,
            EstimatedHours = 75,
            ThumbnailUrl = "https://placeholder.local/paths/frontend-vue-react.jpg",
            DisplayOrder = 2,
            IsPublished = true,
            PathCourses = new List<LearningPathCourse>
            {
                new() { CourseId = FindCourse("Vue 3 Fundamentals — Composition API i TypeScript").Id, Order = 1, IsOptional = false },
                new() { CourseId = FindCourse("React 18 w Praktyce — Hooks, Suspense i Server Components").Id, Order = 2, IsOptional = false },
                new() { CourseId = FindCourse("Angular dla Aplikacji Enterprise").Id, Order = 3, IsOptional = true },
            }
        };

        var dataSciencePath = new LearningPath
        {
            Title = "Data Scientist — od Pythona do LLM",
            Slug = "data-scientist-python-llm",
            ShortDescription = "Kompletna ścieżka od podstaw Pythona do budowy aplikacji z dużymi modelami językowymi.",
            Description = "Ścieżka dla osób, które chcą zostać praktykami uczenia maszynowego i sztucznej inteligencji. Zaczynasz od wprowadzenia do Pythona i klasycznego ML z scikit-learn, następnie przechodzisz do pełnego kursu Data Science z pandas i NumPy. Ścieżkę wieńczy zaawansowany kurs o aplikacjach LLM — prompt engineering, RAG, agenci i deployment. Po ukończeniu potrafisz zbudować end-to-end aplikację AI.",
            DifficultyLevel = PathDifficultyLevel.Advanced,
            EstimatedHours = 110,
            ThumbnailUrl = "https://placeholder.local/paths/data-scientist.jpg",
            DisplayOrder = 3,
            IsPublished = true,
            PathCourses = new List<LearningPathCourse>
            {
                new() { CourseId = FindCourse("Wprowadzenie do Machine Learning z scikit-learn").Id, Order = 1, IsOptional = false },
                new() { CourseId = FindCourse("Python dla Data Science i Machine Learning").Id, Order = 2, IsOptional = false },
                new() { CourseId = FindCourse("Aplikacje z Dużymi Modelami Językowymi (LLM)").Id, Order = 3, IsOptional = false },
            }
        };

        var devopsPath = new LearningPath
        {
            Title = "DevOps Engineer — Cloud Native",
            Slug = "devops-engineer-cloud-native",
            ShortDescription = "Zostań inżynierem DevOps — konteneryzacja, orkiestracja i chmura AWS.",
            Description = "Ścieżka dla specjalistów, którzy chcą budować i utrzymywać nowoczesną infrastrukturę dla aplikacji webowych. Zaczynasz od konteneryzacji i orkiestracji z Dockerem i Kubernetesem, następnie poznajesz architekturę AWS i przygotowanie do certyfikacji Solutions Architect. Całość daje praktyczne kompetencje do pracy w zespołach platform engineering i SRE.",
            DifficultyLevel = PathDifficultyLevel.Intermediate,
            EstimatedHours = 85,
            ThumbnailUrl = "https://placeholder.local/paths/devops-cloud.jpg",
            DisplayOrder = 4,
            IsPublished = true,
            PathCourses = new List<LearningPathCourse>
            {
                new() { CourseId = FindCourse("Docker i Kubernetes w Środowisku Produkcyjnym").Id, Order = 1, IsOptional = false },
                new() { CourseId = FindCourse("AWS Solutions Architect — Kompletne Przygotowanie").Id, Order = 2, IsOptional = false },
                new() { CourseId = FindCourse("Zaawansowane .NET 9 Web API — Clean Architecture i CQRS").Id, Order = 3, IsOptional = true },
            }
        };

        context.LearningPaths.AddRange(backendPath, frontendPath, dataSciencePath, devopsPath);
        await context.SaveChangesAsync();
    }

    private static async Task SeedBusinessPlansAsync(ApplicationDbContext context)
    {
        if (await context.BusinessPlans.AnyAsync()) return;

        var starter = new BusinessPlan
        {
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
        };

        var team = new BusinessPlan
        {
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
        };

        var enterprise = new BusinessPlan
        {
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
        };

        starter.Features = new List<BusinessPlanFeature>
        {
            new() { Text = "Dostęp do wybranych kursów", DisplayOrder = 1 },
            new() { Text = "1 miejsce dla pracownika", DisplayOrder = 2 },
            new() { Text = "Certyfikaty ukończenia", DisplayOrder = 3 }
        };

        team.Features = new List<BusinessPlanFeature>
        {
            new() { Text = "Pełen dostęp do biblioteki kursów", DisplayOrder = 1 },
            new() { Text = "Do 25 miejsc w zespole", DisplayOrder = 2 },
            new() { Text = "Panel postępów i raportowanie", DisplayOrder = 3 },
            new() { Text = "Priorytetowe wsparcie", DisplayOrder = 4 }
        };

        enterprise.Features = new List<BusinessPlanFeature>
        {
            new() { Text = "Nieograniczone miejsca", DisplayOrder = 1 },
            new() { Text = "SSO i integracje (SAML, SCIM)", DisplayOrder = 2 },
            new() { Text = "Dedykowany opiekun klienta", DisplayOrder = 3 },
            new() { Text = "Własne ścieżki szkoleniowe", DisplayOrder = 4 },
            new() { Text = "SLA i umowy powierzenia danych", DisplayOrder = 5 }
        };

        context.BusinessPlans.AddRange(starter, team, enterprise);
        await context.SaveChangesAsync();
    }

    private static async Task SeedLessonProgressAsync(ApplicationDbContext context)
    {
        var students = await context.Users
            .Where(u => u.Email!.EndsWith("@courseplatform.com") &&
                        (u.Email.StartsWith("piotr.") || u.Email.StartsWith("karolina.") ||
                         u.Email.StartsWith("tomasz.") || u.Email.StartsWith("monika.") ||
                         u.Email.StartsWith("jakub.")))
            .ToListAsync();

        var studentIds = students.Select(s => s.Id).ToList();
        var existingProgressCount = await context.LessonProgresses
            .Where(lp => studentIds.Contains(lp.UserId))
            .CountAsync();

        if (existingProgressCount > 0) return;

        var courses = await context.Courses
            .Include(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .ToListAsync();

        var enrollments = await context.Enrollments.ToListAsync();

        var progressEntries = new List<LessonProgress>();
        var random = new Random(42);
        int studentIndex = 0;

        foreach (var student in students)
        {
            var studentEnrollments = enrollments.Where(e => e.UserId == student.Id).ToList();
            int courseIndex = 0;

            foreach (var enrollment in studentEnrollments)
            {
                var course = courses.FirstOrDefault(c => c.Id == enrollment.CourseId);
                if (course == null) continue;

                var lessons = course.Modules.SelectMany(m => m.Lessons).OrderBy(l => l.Order).ToList();
                if (lessons.Count == 0) continue;

                double completionRate = studentIndex switch
                {
                    0 => 0.70 + courseIndex * 0.05,
                    1 => 0.55 + courseIndex * 0.08,
                    2 => 0.40 + courseIndex * 0.10,
                    3 => 0.80 + courseIndex * 0.03,
                    4 => 0.30 + courseIndex * 0.12,
                    _ => 0.50
                };

                int lessonsToComplete = Math.Min((int)(lessons.Count * completionRate), lessons.Count);
                for (int i = 0; i < lessonsToComplete; i++)
                {
                    progressEntries.Add(new LessonProgress
                    {
                        UserId = student.Id,
                        LessonId = lessons[i].Id,
                        IsCompleted = true,
                        CompletedAt = enrollment.EnrolledAt.AddDays(random.Next(1, 30))
                    });
                }

                courseIndex++;
            }

            studentIndex++;
        }

        context.LessonProgresses.AddRange(progressEntries);
        await context.SaveChangesAsync();
    }
}

# Kompletny kurs ASP.NET Core — od zera do juniora z rocznym stażem

> **Dla kogo:** programista z ~rocznym doświadczeniem, który ogarnia bazy danych i trochę frontu (np. Vue), ale chce **dogłębnie** zrozumieć ekosystem .NET / ASP.NET Core zgodnie z [roadmapą roadmap.sh](https://roadmap.sh/aspnet-core).
>
> **Projekt Course Platform** (to repozytorium) jest **dodatkiem** — pokazuje, jak wybrane koncepcje wyglądają w prawdziwym kodzie. Większość tematów z roadmapy (Redis, SignalR, GraphQL, Elasticsearch…) omawiamy teoretycznie + przykłady „jak by to wyglądało”, bo nie wszystko jest jeszcze zaimplementowane w projekcie.
>
> **Priorytet nauki:** zgodnie z oficjalną [roadmapą roadmap.sh](https://roadmap.sh/aspnet-core) (stan z repozytorium GitHub, czerwiec 2026) — najpierw pozycje z legendą **Personal Recommendation / Opinion** (niebieskie połączenie na mapie), potem główna ścieżka (węzły bez legendy), na końcu *Alternative* i *Optional*.

---

## Spis treści

### Część 0 — Audyt roadmapy (PRZECZYTAJ NAJPIERW)
- [0. Legenda kolorów roadmap.sh](#0-legenda-kolorów-roadmapsh)
- [0.1. Pełna lista REKOMENDOWANYCH (niebieskich) — 64 pozycje](#01-pełna-lista-rekomendowanych-niebieskich--64-pozycje)
- [0.2. Alternative Options i Optional — co pominąć na start](#02-alternative-options-i-optional--co-pominąć-na-start)
- [0.3. Korekty na czerwiec 2026 (roadmapa vs rynek)](#03-korekty-na-czerwiec-2026-roadmapa-vs-rynek)

### Część A — Fundamenty
- [A1. Jak myśleć o ASP.NET Core](#a1-jak-myśleć-o-aspnet-core)
- [A2. HTTP — co musisz wiedzieć na pamięć](#a2-http--co-musisz-wiedzieć-na-pamięć)
- [A3. C# i .NET — rzeczy, które ciągle wracają](#a3-c-i-net--rzeczy-które-ciągle-wracają)
- [A4. .NET CLI i struktura projektu](#a4-net-cli-i-struktura-projektu)

### Część B — Serce ASP.NET Core
- [B1. Host, Kestrel, `Program.cs`](#b1-host-kestrel-programcs)
- [B2. Middleware — ULTRA szczegółowo](#b2-middleware--ultra-szczegółowo)
- [B3. Filtry, atrybuty, Minimal APIs vs kontrolery](#b3-filtry-atrybuty-minimal-apis-vs-kontrolery)
- [B4. Konfiguracja (`appsettings`, env, secrets)](#b4-konfiguracja-appsettings-env-secrets)
- [B5. MVC, Razor Pages, Blazor — kiedy co](#b5-mvc-razor-pages-blazor--kiedy-co)

### Część C — Dane
- [C1. Entity Framework Core — od podstaw do Change Trackera](#c1-entity-framework-core--od-podstaw-do-change-trackera)
- [C2. SQL Server w świecie .NET](#c2-sql-server-w-świecie-net)
- [C3. PostgreSQL, NoSQL, Cosmos — panorama](#c3-postgresql-nosql-cosmos--panorama)
- [C4. Dapper i alternatywy dla EF](#c4-dapper-i-alternatywy-dla-ef)

### Część D — Wydajność i skala
- [D1. Cache — Memory Cache](#d1-cache--memory-cache)
- [D2. Distributed Cache i Redis](#d2-distributed-cache-i-redis)
- [D3. Elasticsearch — wyszukiwanie i analiza](#d3-elasticsearch--wyszukiwanie-i-analiza)

### Część E — Komunikacja
- [E1. REST API — standard, który musisz znać](#e1-rest-api--standard-który-musisz-znać)
- [E2. OData i Gridify](#e2-odata-i-gridify)
- [E3. GraphQL i HotChocolate](#e3-graphql-i-hotchocolate)
- [E4. gRPC](#e4-grpc)
- [E5. SignalR i WebSockets](#e5-signalr-i-websockets)

### Część F — Operacje w tle
- [F1. `BackgroundService` i Hosted Services](#f1-backgroundservice-i-hosted-services)
- [F2. Hangfire, Quartz, Coravel](#f2-hangfire-quartz-coravel)

### Część G — Jakość i architektura
- [G1. Dependency Injection — dogłębnie](#g1-dependency-injection--dogłębnie)
- [G2. Logowanie: Serilog, NLog](#g2-logowanie-serilog-nlog)
- [G3. Architektura oprogramowania](#g3-architektura-oprogramowania)
- [G4. CQRS, MediatR, FluentValidation](#g4-cqrs-mediatr-fluentvalidation)
- [G5. Testowanie](#g5-testowanie)

### Część H — Produkcja
- [H1. Docker i konteneryzacja](#h1-docker-i-konteneryzacja)
- [H2. Microservices, message brokers — panorama](#h2-microservices-message-brokers--panorama)
- [H3. CI/CD — co junior powinien wiedzieć](#h3-cicd--co-junior-powinien-wiedzieć)

### Część I — Integracja z frontendem
- [I1. ASP.NET Core + Vue (lub inny SPA)](#i1-aspnet-core--vue-lub-inny-spa)

### Część J — Praktyka
- [J1. Course Platform — mapa przykładów](#j1-course-platform--mapa-przykładów)
- [J2. Plan nauki na 12 tygodni](#j2-plan-nauki-na-12-tygodni)
- [J3. Checklist juniora .NET](#j3-checklist-juniora-net)
- [J4. Słownik](#j4-słownik)

### Część K — Uzupełnienia z audytu (wcześniej brakujące / za krótkie)
- [K1. Git i współpraca (rekomendowane)](#k1-git-i-współpraca-rekomendowane)
- [K2. StyleCop i jakość kodu C#](#k2-stylecop-i-jakość-kodu-c)
- [K3. Microsoft.Extensions — co to jest](#k3-microsoftextensions--co-to-jest)
- [K4. Scalar — dokumentacja API w 2026](#k4-scalar--dokumentacja-api-w-2026)
- [K5. RabbitMQ + MassTransit (rekomendowane)](#k5-rabbitmq--masstransit-rekomendowane)
- [K6. Ocelot i YARP — API Gateway](#k6-ocelot-i-yarp--api-gateway)
- [K7. Playwright + SpecFlow — testy E2E i BDD](#k7-playwright--specflow--testy-e2e-i-bdd)
- [K8. AutoFixture, Shouldly, Testcontainers — testy (rekomendowane)](#k8-autofixture-shouldly-testcontainers--testy-rekomendowane)
- [K9. DSA i Database Design — minimum dla juniora .NET](#k9-dsa-i-database-design--minimum-dla-juniora-net)
- [K10. EF „Framework Basics” — co oznacza ten węzeł](#k10-ef-framework-basics--co-oznacza-ten-węzeł)

---

# Część 0 — Audyt roadmapy

## 0. Legenda kolorów roadmap.sh

Oficjalny plik mapy (`aspnet-core.json` w repozytorium [kamranahmedse/developer-roadmap](https://github.com/kamranahmedse/developer-roadmap)) przypisuje każdemu **subtopic** pole `legend`:

| Legenda na mapie | Kolor linii | Co to znaczy dla Ciebie |
|------------------|-------------|-------------------------|
| **Personal Recommendation / Opinion** | Niebieski `#2B78E4` | **Ucz się w pierwszej kolejności** — autorzy roadmapy i społeczność uznają to za domyślny wybór w ekosystemie .NET |
| *(brak legendy — główna ścieżka)* | Szara ścieżka | **Kontenery tematyczne** — musisz wejść w temat, ale szczegóły wybierasz z niebieskich gałęzi (np. węzeł „Caching” → uczysz Memory Cache + Redis, nie Memcached) |
| **Alternative Options** | Inny kolor | Znasz z nazwy + wiesz kiedy wybrać zamiast rekomendacji (np. Dapper zamiast EF na hot path) |
| **Optional / Learn anytime** | Inny kolor | Kiedy projekt lub praca tego wymaga — nie blokuj kariery, jeśli nie znasz od razu |

**Ważne:** PDF, który pobrałeś, **nie pokazuje kolorów** — lista jest płaska. Niebieskie znaczniki widać tylko na interaktywnej mapie lub w JSON.

---

## 0.1. Pełna lista REKOMENDOWANYCH (niebieskich) — 64 pozycje

Poniżej **wszystkie** unikalne pozycje z legendą *Personal Recommendation* ze stanu roadmapy czerwiec 2026. Kolumna **Kurs** mówi, gdzie to omówiono w tym dokumencie. **Projekt** — czy Course Platform to demonstruje.

| # | Temat (rekomendowany) | Kurs | Projekt |
|---|------------------------|------|---------|
| 1 | C# | A3 | ✅ encje, handlery |
| 2 | .NET | A1, A4 | ✅ .NET 9 |
| 3 | .NET CLI | A4 | ✅ Docker |
| 4 | HTTP / HTTPS Protocol | A2 | ✅ REST |
| 5 | Data Structures and Algorithms | K9 | — |
| 6 | Database Fundamentals | K9 | ✅ relacje |
| 7 | SQL Basics | K9 | (ogarniasz) |
| 8 | Database Design Basics | K9 | ✅ Fluent API |
| 9 | Stored Procedures | C2 | — |
| 10 | Constraints | C1, C2, K9 | ✅ unique index |
| 11 | Git - Version Control | K1 | ✅ repo |
| 12 | GitHub, GitLab, BitBucket | K1 | ✅ |
| 13 | MVC | B3, B5 | — (API, nie MVC) |
| 14 | REST | E1 | ✅ |
| 15 | Middlewares | B2 | ✅ |
| 16 | Filters and Attributes | B3 | ✅ `[Authorize]` |
| 17 | App Settings and Configs | B4 | ✅ Jwt, .env |
| 18 | StyleCop Rules | K2 | — |
| 19 | Minimal APIs | B3 | — (kontrolery) |
| 20 | Entity Framework Core | C1 | ✅ |
| 21 | Framework Basics | K10 | ✅ |
| 22 | Code First + Migrations | C1 | ✅ |
| 23 | Lazy, Eager, Explicit Loading | C1 | ✅ Include |
| 24 | Change Tracker API | C1 | ✅ |
| 25 | Dependency Injection — Life Cycles | G1 | ✅ |
| 26 | DI Containers | G1 | ✅ built-in |
| 27 | Microsoft.Extensions | K3 | ✅ |
| 28 | Scoped / Transient / Singleton | G1 | ✅ |
| 29 | Memory Cache | D1 | — |
| 30 | Distributed Cache | D2 | — |
| 31 | Redis | D2 | — |
| 32 | Elastic Search | D3 | — (EF Like) |
| 33 | SQL Server | C2 | — (PostgreSQL) |
| 34 | Relational | C2, C3 | ✅ |
| 35 | Dynamo DB | C3 | — |
| 36 | MongoDB | C3 | — |
| 37 | Serilog | G2 | ✅ |
| 38 | Gridlify | E2 | — |
| 39 | HotChocolate | E3 | — |
| 40 | SignalR Core | E5 | — |
| 41 | Manual Mapping | G5 | ✅ Select() |
| 42 | Native Background Service | F1 | — |
| 43 | Hangfire | F2 | — |
| 44 | XUnit | G5, K8 | ✅ |
| 45 | Shouldly | G5, K8 | ✅ w testach |
| 46 | Moq | G5, K8 | — |
| 47 | AutoFixture | K8 | — |
| 48 | WebApplicationFactory | G5 | ✅ |
| 49 | Test Containers | G5, K8 | wzmianka |
| 50 | Playwright | K7 | — |
| 51 | Specflow | K7 | — |
| 52 | RabbitMQ | K5 | — |
| 53 | Mass Transit | K5 | — |
| 54 | Ocelot | K6 | — |
| 55 | YARP | K6 | — |
| 56 | Docker | H1 | ✅ |
| 57 | Kubernetes | H1 | panorama |
| 58 | GitHub Actions | H3 | panorama |
| 59 | MediatR | G4 | ✅ |
| 60 | FluentValidation | G4 | ✅ |
| 61 | Scalar | K4 | — (Swagger) |
| 62 | Blazor | B5 | — (Vue) |

**Podsumowanie audytu:** większość **niebieskich** tematów jest w częściach A–J; **Część K** uzupełnia luki: Git, StyleCop, Microsoft.Extensions, Scalar, MassTransit/RabbitMQ, Ocelot/YARP, Playwright/SpecFlow, AutoFixture/Testcontainers, DSA/DB design, EF Framework Basics.

---

## 0.2. Alternative Options i Optional — co pominąć na start

### Alternative (39 pozycji) — znasz z nazwy, nie uczysz na pamięć

| Grupa | Alternative | Zamiast tego (rekomendacja) |
|-------|-------------|----------------------------|
| ORM | Dapper, RepoDB, NHibernate | **EF Core** |
| Cache | Memcached, EF 2nd Level Cache | **Memory Cache + Redis** |
| DB | PostgreSQL*, Cosmos, CouchDB, LiteDB, Cassandra | **SQL Server** (ekosystem MS) + relacyjna w .NET |
| API | GraphQL, gRPC | **REST** (+ HotChocolate jak osobny skill) |
| Search | Solr, Sphinx | **Elasticsearch** |
| Mapping | AutoMapper, Mapperly | **Manual mapping** (jak w Course Platform) |
| Test | NUnit, Bogus | **xUnit + AutoFixture** |
| CI | Azure Pipelines, Circle CI, GitLab | **GitHub Actions** |
| Gateway | — | **Ocelot lub YARP** (oba niebieskie!) |
| Inne | Polly, Benchmark.NET, .NET Aspire, .NET MAUI, Razor Pages | Gdy projekt wymaga |

\*PostgreSQL jest *alternative* na mapie ASP.NET, ale **dominuje w nowych projektach open source** — Course Platform słusznie go używa. Umiejętność EF przenosi się 1:1.

### Optional (23 pozycje) — „kiedy trzeba”

AutoFac, NLog, Quartz, Coravel, OData, NSubstitute, FakeItEasy, MSTest, WebSockets (osobno od SignalR), MariaDB, MySQL, Azure Service Bus, EasyNetQ, Dapr, SteelToe, Scriban, Fluid, Razor Components, ActiveMQ, Respawn, Marten, Nuke, GraphQL .NET (osobny node od GraphQL).

**Junior z rocznym stażem:** nie musisz znać optional na rozmowę — wystarczy *„słyszałem, używamy X w firmie”*.

---

## 0.3. Korekty na czerwiec 2026 (roadmapa vs rynek)

| Temat | Roadmapa | Stan 2026 | Co robić |
|-------|----------|-----------|----------|
| **FluentAssertions** | Usunięte z mapy (2025, licencja komercyjna) | **Shouldly** lub asercje xUnit | Ucz Shouldly — jest niebieskie |
| **Swagger UI** | Nie wymienione osobno | Wciąż standard; **Scalar** rośnie | Course Platform: Swagger; nowe projekty: Scalar (niebieskie) |
| **.NET wersja** | Ogólne „.NET” | **.NET 9** LTS path | Projekt na .NET 9 |
| **Minimal APIs vs kontrolery** | Oba niebieskie | Oba żyją; korporacje → kontrolery | Znaj oba; w projekcie czytaj kontrolery |
| **Ocelot vs YARP** | **Oba niebieskie** | YARP — projekt Microsoft, aktywny rozwój | Na rozmowie: znasz oba; greenfield MS stack → YARP |
| **GraphQL** | Alternative; **HotChocolate** niebieskie | HotChocolate = domyślny stack GraphQL w .NET | Ucz HotChocolate, nie „GraphQL ogólnie” z node alternative |
| **Playwright** | Niebieskie | Dominuje nad Puppeteer/Cypress w .NET E2E | Ucz Playwright, nie Cypress (alternative) |
| **Identity** | Nie ma osobnego węzła | **ASP.NET Identity + JWT** standard | Course Platform ✅ |
| **PostgreSQL** | Alternative | Bardzo popularny w Docker/dev | EF + Npgsql — praktyczny wybór mimo „alternative” na mapie |

---

# Część A — Fundamenty

## A1. Jak myśleć o ASP.NET Core

### Co to w ogóle jest?

**ASP.NET Core** to framework Microsoftu do budowania aplikacji webowych i API w języku **C#** na platformie **.NET**.

Rozbijmy to na części pierwsze:

| Słowo | Co to znaczy „jak dla debila” |
|-------|-------------------------------|
| **.NET** | Środowisko uruchomieniowe + biblioteki. Jak „Node.js + npm”, tylko dla C#. |
| **ASP.NET Core** | Warstwa do HTTP — przyjmuje requesty z internetu i odsyła odpowiedzi. |
| **C#** | Język, w którym piszesz kod. |
| **Kestrel** | Wbudowany serwer HTTP (nasłuchuje na porcie, np. 8080). |
| **Middleware** | „Filtry na taśmie produkcyjnej” — każdy request przechodzi przez łańcuch kroków. |

### Dwa główne „kształty” aplikacji

1. **API (REST/GraphQL/gRPC)** — zwraca JSON. Frontend (Vue, React, mobile) sam renderuje UI.
2. **Server-rendered (MVC, Razor Pages, Blazor Server)** — serwer generuje HTML i odsyła gotową stronę.

**Course Platform** to wariant 1: backend = API, frontend = Vue. To najczęstszy układ w nowoczesnych projektach.

### Co musisz umieć jako junior z rocznym stażem?

Nie musisz znać wszystkiego z roadmapy na pamięć. Musisz **wiedzieć, że coś istnieje, do czego służy i kiedy sięgać po dokumentację**. Ten kurs daje ci mapę terenu.

---

## A2. HTTP — co musisz wiedzieć na pamięć

HTTP to **protokół tekstowy**: klient wysyła **request**, serwer odsyła **response**.

### Anatomia requestu

```
POST /api/courses HTTP/1.1
Host: localhost:8080
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

{"title":"Vue 3","price":99.99}
```

| Element | Znaczenie |
|---------|-----------|
| **Metoda** | Co chcesz zrobić: GET (czytaj), POST (twórz), PUT/PATCH (zmień), DELETE (usuń) |
| **URL / path** | Który zasób: `/api/courses`, `/api/courses/3fa85f64-...` |
| **Headers** | Metadane: typ treści, token auth, język |
| **Body** | Treść (zwykle JSON w API) — tylko przy POST/PUT/PATCH |

### Kody statusu — te 10 musisz znać

| Kod | Nazwa | Kiedy |
|-----|-------|-------|
| **200** | OK | Sukces, jest body |
| **201** | Created | Utworzono zasób (czasem zamiast 200) |
| **204** | No Content | Sukces, brak body (np. po DELETE) |
| **400** | Bad Request | Złe dane wejściowe (walidacja) |
| **401** | Unauthorized | Brak logowania / zły token |
| **403** | Forbidden | Zalogowany, ale brak uprawnień |
| **404** | Not Found | Zasób nie istnieje |
| **409** | Conflict | Konflikt (np. duplikat unique key) |
| **429** | Too Many Requests | Rate limiting |
| **500** | Internal Server Error | Błąd serwera (bug) |

### HTTPS

HTTP + szyfrowanie TLS. W dev często `http://localhost`. W produkcji **zawsze** HTTPS — certyfikat (Let's Encrypt, Azure, Cloudflare).

### Stateless

HTTP **nie pamięta** poprzednich requestów. Każdy request jest „świeży”. Dlatego:
- **Sesje** (cookie po stronie serwera) albo
- **JWT** (token po stronie klienta, serwer tylko weryfikuje podpis)

Course Platform używa JWT — patrz [G4](#g4-cqrs-mediatr-fluentvalidation) i `JwtTokenGenerator.cs`.

---

## A3. C# i .NET — rzeczy, które ciągle wracają

### `async` / `await` — obowiązkowe w ASP.NET

Serwer obsługuje **wiele requestów naraz**. Gdy czekasz na bazę danych, **nie blokuj wątku** — użyj `await`:

```csharp
// ŹLE — blokuje wątek (w ASP.NET może obniżyć przepustowość)
var courses = _context.Courses.ToList();

// DOBRZE
var courses = await _context.Courses.ToListAsync(cancellationToken);
```

**`CancellationToken`** — gdy klient anuluje request (zamknie kartę), operacja może się przerwać. Zawsze przekazuj dalej w handlerach.

### `record` vs `class`

```csharp
// record — niezmienny „pakiet danych”, idealny na DTO i commandy
public record CreateCourseCommand(string Title, decimal Price) : IRequest<Guid>;

// class — encja z tożsamością, może się zmieniać w czasie
public class Course { public string Title { get; set; } }
```

### Nullable (`string?`, `Guid?`)

`?` = może być `null`. Kompilator ostrzega, żebyś sprawdził przed użyciem.

### LINQ

Język zapytań do kolekcji i EF:

```csharp
var published = courses.Where(c => c.Status == CourseStatus.Published)
                       .OrderBy(c => c.Title)
                       .Take(10);
```

EF Core **tłumaczy** LINQ na SQL — nie pobieraj wszystkiego do pamięci, filtruj w bazie.

### Interfejsy i DI

```csharp
public interface IEmailService { Task SendAsync(string to, string body); }
public class SmtpEmailService : IEmailService { ... }
```

Kod zależy od **abstrakcji** (`IEmailService`), nie od konkretnej implementacji. Kontener DI podstawia implementację w runtime.

---

## A4. .NET CLI i struktura projektu

### Najważniejsze komendy

```bash
dotnet new webapi -n MyApi          # nowy projekt API
dotnet build                         # kompilacja
dotnet run                           # uruchomienie
dotnet test                          # testy
dotnet add package Npgsql            # dodaj paczkę NuGet
dotnet ef migrations add Initial       # migracja EF (wymaga dotnet-ef tool)
```

W Course Platform wszystko przez Docker: `docker compose exec api dotnet build`.

### Co to jest NuGet?

Menadżer paczek .NET = npm dla Node. `MediatR`, `Serilog`, `HotChocolate` — wszystko z NuGet.

### Pliki `.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="MediatR" Version="12.x" />
  </ItemGroup>
</Project>
```

Definiuje wersję .NET i zależności.

---

# Część B — Serce ASP.NET Core

## B1. Host, Kestrel, `Program.cs`

### Cykl życia aplikacji

```
1. dotnet run
2. Tworzy się WebApplicationBuilder
3. Rejestrujesz serwisy (DI): builder.Services.Add...
4. Budujesz app: var app = builder.Build()
5. Konfigurujesz pipeline: app.Use...
6. app.Run() — Kestrel nasłuchuje na porcie
```

### Przykład z Course Platform

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddApplication();      // MediatR, FluentValidation
builder.Services.AddInfrastructure(...);  // EF, JWT generator

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

**`WebApplication.CreateBuilder`** ładuje też:
- `appsettings.json`
- zmienne środowiskowe
- argumenty wiersza poleceń

Kolejność źródeł konfiguracji ma priorytety (env nadpisuje appsettings).

---

## B2. Middleware — ULTRA szczegółowo

### Co to jest middleware?

**Middleware** to funkcja o sygnaturze:

```csharp
public class MyMiddleware
{
    private readonly RequestDelegate _next;

    public MyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // PRZED następnym middleware — request idzie „w dół”
        await _next(context);
        // PO następnym middleware — response idzie „w górę”
    }
}
```

Każdy middleware może:
1. **Zrobić coś przed** wywołaniem `_next` (np. sprawdzić auth)
2. **Wywołać następny** middleware (`_next`)
3. **Zrobić coś po** powrocie (np. dodać nagłówek do response)
4. **Nie wywołać `_next`** — przerwać pipeline (np. zwrócić 401 od razu)

### Pipeline = stos cebuli

```
Request  ──►  [SecurityHeaders]  ──►  [Serilog]  ──►  [RateLimit]  ──►  [CORS]
                │                        │                │               │
                ▼                        ▼                ▼               ▼
              ...                    ...              ...             ...
                │                        │                │               │
                ▼                        ▼                ▼               ▼
Response ◄──  [SecurityHeaders]  ◄──  [Serilog]  ◄──  [RateLimit]  ◄──  [CORS]
```

Request wchodzi od lewej, przechodzi przez wszystkie warstwy do kontrolera, odpowiedź wraca tą samą drogą w odwrotnej kolejności (dla kodu „po `_next`”).

### Kolejność w Course Platform — i DLACZEGO

```csharp
app.UseMiddleware<SecurityHeadersMiddleware>();  // 1
app.UseSerilogRequestLogging();                   // 2
app.UseRateLimiter();                             // 3
app.UseCors("AllowFrontend");                     // 4
app.UseAuthentication();                          // 5
app.UseAuthorization();                           // 6
app.UseMiddleware<ExceptionHandlingMiddleware>(); // 7
app.MapControllers();                             // 8
```

| # | Middleware | Po co | Co by się stało, gdy źle |
|---|------------|-------|--------------------------|
| 1 | Security headers | `X-Content-Type-Options`, `CSP` itd. | Bez ochrony przed niektórymi atakami |
| 2 | Serilog logging | Log każdego requestu | Trudniejszy debug |
| 3 | Rate limiter | Ochrona przed floodem | Brak ochrony API |
| 4 | CORS | Zezwolenie na requesty z Vue (5173) | Przeglądarka zablokuje fetch |
| 5 | Authentication | Odczyt JWT → `HttpContext.User` | `User` pusty |
| 6 | Authorization | Sprawdzenie `[Authorize]` | Każdy endpoint otwarty |
| 7 | Exception handling | Łapie wyjątki → JSON 400/404/500 | Stack trace w response (złe!) |
| 8 | MapControllers | Routing do akcji kontrolera | Brak endpointów |

**Złota zasada:** `UseAuthentication` **przed** `UseAuthorization`. Autoryzacja bez tożsamości nie ma sensu.

**Uwaga o ExceptionHandlingMiddleware:** W tym projekcie jest **po** auth. Alternatywna szkoła stawia go **na samym początku**, żeby łapał absolutnie wszystko. Oba podejścia spotkasz w firmach — ważne, żebyś wiedział, *co* łapie.

### Wbudowane middleware (nie musisz pisać sam)

| Middleware | Metoda | Co robi |
|----------|--------|---------|
| Routing | `UseRouting()` | Dopasowuje URL do endpointu (często implicit) |
| CORS | `UseCors()` | Cross-Origin Resource Sharing |
| Authentication | `UseAuthentication()` | Ustawia `HttpContext.User` |
| Authorization | `UseAuthorization()` | Sprawdza polityki i `[Authorize]` |
| Static files | `UseStaticFiles()` | Serwuje pliki z `wwwroot` |
| HTTPS redirect | `UseHttpsRedirection()` | Przekierowuje HTTP → HTTPS |
| Exception Handler | `UseExceptionHandler()` | Wbudowany handler błędów |

### Jak napisać własny middleware — krok po kroku

**Krok 1:** Klasa z `RequestDelegate _next`.

**Krok 2:** Metoda `InvokeAsync(HttpContext context)`.

**Krok 3:** Zarejestruj: `app.UseMiddleware<TwojMiddleware>()` albo extension method.

Przykład z projektu — globalna obsługa błędów:

```csharp
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    catch (NotFoundException ex)
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (ValidationException ex)
    {
        context.Response.StatusCode = 400;
        // errors per field...
    }
}
```

Handler rzuca `NotFoundException` → middleware łapie → klient dostaje JSON, nie HTML z stack trace.

### Middleware vs Filter vs Endpoint filter

| Mechanizm | Gdzie działa | Kiedy używać |
|-----------|--------------|--------------|
| **Middleware** | Cała aplikacja, wcześnie w pipeline | Logowanie, CORS, auth, exception handling |
| **Action Filter** | Tylko kontrolery MVC/API | Walidacja przed akcją, logowanie parametrów |
| **Authorization Filter** | Przed akcją | Custom auth (rzadziej niż `[Authorize]`) |
| **Exception Filter** | Przy wyjątku w akcji | Alternatywa dla middleware |
| **Endpoint Filter** | Minimal APIs | Jak action filter, ale dla `app.MapGet` |

**Junior:** 80% potrzeb pokryjesz middleware + `[Authorize]` + global exception middleware. Filtry poznaj, gdy zobaczysz w kodzie firmy.

### `HttpContext` — pudełko na wszystko o requestcie

```csharp
context.Request.Method          // "GET", "POST"
context.Request.Path          // "/api/courses"
context.Request.Headers       // nagłówki
context.User                  // ClaimsPrincipal po auth
context.Response.StatusCode   // 200, 404...
context.RequestServices       // DI container dla tego requestu
```

`ICurrentUserService` w projekcie czyta `context.User.FindFirst("sub")` — to claim z JWT.

---

## B3. Filtry, atrybuty, Minimal APIs vs kontrolery

### Atrybuty — metadane na kodzie

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("api")]
public class CoursesController : ControllerBase
```

| Atrybut | Efekt |
|---------|-------|
| `[ApiController]` | Automatyczny 400 przy złym modelu, binding source inference |
| `[Route]` | Prefix URL |
| `[Authorize]` | Wymaga zalogowania |
| `[AllowAnonymous]` | Wyjątek od `[Authorize]` na klasie |
| `[FromQuery]` | Parametr z `?page=1` |
| `[FromBody]` | JSON z body |
| `[FromRoute]` | `{id}` z URL |

### Kontrolery (ten projekt)

```csharp
[HttpGet("{id:guid}")]
public async Task<ActionResult<CourseDetailsDto>> GetCourseDetails(Guid id, CancellationToken ct)
{
    var result = await _mediator.Send(new GetCourseDetailsQuery(id), ct);
    return Ok(result);
}
```

**Plusy:** struktura, konwencje, łatwe grupowanie, Swagger out-of-the-box.  
**Minusy:** więcej plików, czasem „ceremonialność”.

### Minimal APIs

```csharp
app.MapGet("/api/courses/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new GetCourseDetailsQuery(id), ct);
    return Results.Ok(result);
}).RequireAuthorization();
```

**Plusy:** zwięzłe, szybkie prototypy, mniej boilerplate.  
**Minusy:** duże API staje się nieczytelne bez organizacji.

**Junior:** Oba robią to samo pod spodem. W firmie spotkasz głównie kontrolery w większych projektach; Minimal APIs w mikroserwisach i małych usługach.

### MVC (Model-View-Controller) — klasyczny web

- **Model** — dane
- **View** — Razor (`.cshtml`) generuje HTML
- **Controller** — logika, zwraca View

Nadal używane w panelach admina, starych systemach, niektórych aplikacjach wewnętrznych. **Nie** jest to to samo co Web API — MVC zwraca HTML, API zwraca JSON.

---

## B4. Konfiguracja (`appsettings`, env, secrets)

### Hierarchia konfiguracji

```
appsettings.json                    (baza)
  └── appsettings.Development.json  (nadpisuje w dev)
        └── zmienne środowiskowe     (nadpisują wszystko)
              └── User Secrets        (dev, nie commitowane)
```

### Przykład

`appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db;Database=courseplatform;..."
  },
  "Jwt": {
    "Issuer": "CoursePlatform",
    "Audience": "CoursePlatform",
    "ExpiryMinutes": 120
  }
}
```

Zmienna env: `Jwt__Key=moj-tajny-klucz` → `configuration["Jwt:Key"]`.

### Options pattern — poprawny sposób

Zamiast `configuration["Jwt:Key"]` wszędzie:

```csharp
public class JwtSettings
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "";
}

// Program.cs
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// W klasie
public class JwtTokenGenerator
{
    public JwtTokenGenerator(IOptions<JwtSettings> options)
    {
        var settings = options.Value;
    }
}
```

**Dlaczego:** typowanie, walidacja przy starcie (`ValidateOnStart`), testowalność.

### Sekrety

- **Nigdy** nie commituj haseł, kluczy JWT, connection stringów produkcyjnych.
- Dev: User Secrets (`dotnet user-secrets set "Jwt:Key" "..."`) lub `.env` (jak w Course Platform).
- Prod: Azure Key Vault, AWS Secrets Manager, zmienne w CI/CD.

---

## B5. MVC, Razor Pages, Blazor — kiedy co

| Technologia | Co renderuje UI | Kiedy |
|-------------|-----------------|-------|
| **Web API + SPA (Vue)** | Vue w przeglądarce | Nowoczesne produkty, zespoły front/back |
| **MVC + Razor** | Serwer generuje HTML | Proste CRUD, legacy, SEO bez SSR frameworka |
| **Razor Pages** | Jak MVC, ale page-centric | Mniejsze appki z formularzami |
| **Blazor Server** | C# w przeglądarce via SignalR | Zespoły tylko .NET, akceptują latency |
| **Blazor WebAssembly** | C# działa w WASM w przeglądarce | Offline-capable apps .NET |

Course Platform = **API + Vue**. Znasz już front — backend uczy się jako dostawca JSON.

---

# Część C — Dane

## C1. Entity Framework Core — od podstaw do Change Trackera

### Czym jest ORM?

**ORM (Object-Relational Mapper)** mapuje tabele SQL na obiekty C#.

Bez ORM:
```sql
SELECT Id, Title FROM Courses WHERE Status = 1
```
→ ręcznie czytasz `SqlDataReader`, tworzysz `new Course { ... }`.

Z EF:
```csharp
var courses = await _context.Courses.Where(c => c.Status == CourseStatus.Published).ToListAsync();
```

### Code First — jak w projekcie

1. Piszesz klasy C# (`Course`, `Module`).
2. Piszesz konfigurację Fluent API (`CourseConfiguration`).
3. `dotnet ef migrations add Nazwa` — EF generuje plik SQL-ish migracji.
4. `dotnet ef database update` — aplikuje na bazę.

**Migracje są w git** — każdy developer i CI dostaje ten sam schemat.

### DbContext — „sesja” z bazą

```csharp
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
}
```

Jeden `DbContext` na **jeden request** (Scoped) — śledzi zmiany, commituje transakcję przy `SaveChangesAsync`.

### Fluent API — przykład relacji i constraintu

Z Course Platform — enrollment unikalny per user+kurs:

```csharp
builder.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
```

To tworzy w SQL:
```sql
CREATE UNIQUE INDEX IX_Enrollments_UserId_CourseId ON Enrollments (UserId, CourseId);
```

### Ładowanie danych — Lazy, Eager, Explicit

#### Eager loading (`.Include`) — **używaj świadomie**

```csharp
var course = await _context.Courses
    .Include(c => c.Modules)
    .ThenInclude(m => m.Lessons)
    .FirstOrDefaultAsync(c => c.Id == id);
```

Jeden (lub kilka) zapytania SQL z JOINami. **Plus:** wszystko od razu. **Minus:** możesz pobrać za dużo danych.

#### Lazy loading — **domyślnie WYŁĄCZONE w EF Core**

```csharp
// Wymaga virtual + proxy + .UseLazyLoadingProxies()
var course = await _context.Courses.FindAsync(id);
var modules = course.Modules; // DODATKOWE zapytanie SQL „w locie”
```

**Problem N+1:** pętla po kursach, każdy odpala osobne zapytanie o moduły. Junior często o tym nie wie i nagle API jest wolne.

#### Explicit loading

```csharp
var course = await _context.Courses.FindAsync(id);
await _context.Entry(course).Collection(c => c.Modules).LoadAsync();
```

Ręcznie mówisz „teraz doładuj moduły”.

#### `AsNoTracking()` — read-only

```csharp
var list = await _context.Courses.AsNoTracking().ToListAsync();
```

EF **nie śledzi** zmian — szybsze, mniej RAM. Używaj w query, które tylko czytają.

### Change Tracker — serce EF

Gdy pobierasz encję **ze śledzeniem** (bez `AsNoTracking`):

```csharp
var course = await _context.Courses.FindAsync(id);
course.Title = "Nowy tytuł";
await _context.SaveChangesAsync(); // EF wie, że Title się zmienił → UPDATE
```

**Change Tracker** trzyma snapshot i porównuje. Stany encji:

| Stan | Znaczenie |
|------|-----------|
| `Detached` | EF nie śledzi |
| `Unchanged` | Bez zmian |
| `Added` | Nowa, INSERT przy Save |
| `Modified` | Zmieniona, UPDATE |
| `Deleted` | Usunięta, DELETE przy Save |

Debug:
```csharp
var state = _context.Entry(course).State; // Modified
```

### Transakcje

```csharp
await using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    _context.Courses.Add(course);
    _context.Enrollments.Add(enrollment);
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

Albo `SaveChangesAsync` samo w jednej transakcji — wiele operacji na jednym context = jedna transakcja domyślnie.

### Pułapki EF — musisz je znać

1. **N+1 queries** — pętla + lazy load lub brak Include.
2. **Pobranie całej tabeli** — `ToList()` bez `Where` na dużej tabeli.
3. **Concurrency** — dwóch userów edytuje ten sam rekord → `DbUpdateConcurrencyException` (rozwiązanie: token wersji `[Timestamp]` / `rowversion`).
4. **Filtry w pamięci zamiast SQL** — `.ToList()` przed `.Where()` = katastrofa.

```csharp
// ŹLE — cała tabela do RAM, potem filtr w C#
var x = _context.Courses.ToList().Where(c => c.Title.Contains("Vue"));

// DOBRZE — filtr w SQL
var x = await _context.Courses.Where(c => c.Title.Contains("Vue")).ToListAsync();
```

---

## C2. SQL Server w świecie .NET

Znasz bazy — tu **specyfika SQL Server + .NET**, nie SELECT od zera.

### Connection string

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=MyApp;User Id=sa;Password=...;TrustServerCertificate=True"
}
```

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
```

Course Platform używa **PostgreSQL** (`UseNpgsql`) — API EF jest identyczne, zmienia się provider i connection string.

### Typy SQL Server istotne dla EF

| SQL Server | C# / EF | Uwagi |
|------------|---------|-------|
| `uniqueidentifier` | `Guid` | PK w projekcie |
| `nvarchar(max)` | `string` | Unicode |
| `varchar(n)` | `string` + konfiguracja | Non-unicode, mniejszy |
| `decimal(18,2)` | `decimal` | Pieniądze — **nigdy** `float` |
| `datetime2` | `DateTime` | UTC w kodzie (`DateTime.UtcNow`) |
| `rowversion` | `byte[]` concurrency token | Optimistic concurrency |
| `bit` | `bool` | |

### Migracje EF a SQL Server

EF generuje migracje specyficzne dla providera. `AddColumn` w SQL Server to inny DDL niż w PostgreSQL. **Nie mieszaj** providerów na tej samej bazie migracji bez wiedzy.

### Stored procedures — kiedy w .NET

EF pozwala wołać SP:

```csharp
var results = await _context.Courses
    .FromSqlRaw("EXEC GetPublishedCourses @MinPrice", minPriceParam)
    .ToListAsync();
```

**Kiedy:** raporty złożone, legacy baza, optymalizacja przez DBA. **Domyślnie w nowym kodzie:** LINQ + ewentualnie raw SQL.

### Indeksy — co junior powinien wiedzieć

EF tworzy indeksy z Fluent API (`HasIndex`). Na produkcji DBA może dodać więcej. Wolne query → **Execution Plan** w SSMS, nie zgaduj.

### SQL Server vs PostgreSQL w projektach .NET

| | SQL Server | PostgreSQL |
|--|------------|------------|
| Typowy klient | Korporacje, Azure | Startupy, open source |
| EF provider | `UseSqlServer` | `UseNpgsql` |
| Hosting | Azure SQL | RDS, Supabase, Docker |

Umiejętność EF przenosi się — zmieniasz provider i connection string.

---

## C3. PostgreSQL, NoSQL, Cosmos — panorama

### PostgreSQL (Course Platform)

- Open source, JSONB, dobre typy, Docker w dev.
- Case-insensitive search: projekt ma migrację `AddCaseInsensitiveTextCollation`.

### MongoDB / dokumenty

Dane jako JSON dokumenty. Kiedy: schemat bardzo elastyczny, logi, katalogi produktów. W .NET: **MongoDB.Driver**. Nie zastępuje SQL tam, gdzie relacje i transakcje są kluczowe (enrollment, płatności).

### Cosmos DB (Azure)

Globalnie rozproszona baza dokumentów/klucz-wartość. SDK: `Microsoft.Azure.Cosmos`. Typowe w mikroserwisach Azure, nie w małym monolicie.

**Junior:** Wiedz, że istnieją. Na rozmowie: „domyślnie relacyjna + EF, NoSQL gdy model dokumentowy i skala/geografia tego wymagają”.

---

## C4. Dapper i alternatywy dla EF

### Dapper — micro-ORM

```csharp
var courses = await connection.QueryAsync<Course>(
    "SELECT Id, Title FROM Courses WHERE Status = @Status",
    new { Status = 1 });
```

**Plusy:** szybki, pełna kontrola SQL, zero change trackera.  
**Minusy:** ręczne mapowanie, brak migracji, więcej boilerplate przy CRUD.

### Kiedy EF vs Dapper

| Sytuacja | Wybór |
|----------|-------|
| CRUD, relacje, migracje, szybki dev | **EF Core** |
| Raport, złożony JOIN, optymalizacja | **Dapper** lub raw SQL w EF |
| Oba w jednym projekcie | Normalne — EF do 90%, Dapper do krytycznych query |

### RepoDB, NHibernate

Starsze/alternatywne ORM. Na rozmowie wystarczy: „znam EF Core, słyszałem o Dapper i NHibernate”.

---

# Część D — Wydajność i skala

## D1. Cache — Memory Cache

### Po co cache?

Baza i sieć są wolne w porównaniu z RAM. Jeśli **często** czytasz te same dane (lista kategorii, config), trzymaj kopię w pamięci.

### `IMemoryCache` — wbudowane w ASP.NET

```csharp
// Rejestracja
builder.Services.AddMemoryCache();

// Użycie
public class CategoryService
{
    private readonly IMemoryCache _cache;
    private readonly IApplicationDbContext _context;

    public async Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken ct)
    {
        return await _cache.GetOrCreateAsync("categories", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _context.Categories
                .AsNoTracking()
                .Select(c => new CategoryDto(c.Id, c.Name))
                .ToListAsync(ct);
        }) ?? [];
    }
}
```

| Pojęcie | Znaczenie |
|---------|-----------|
| **Absolute expiration** | Wygasa po X czasie od zapisu |
| **Sliding expiration** | Przedłuża się przy każdym odczycie |
| **Cache key** | Unikalny string — `"categories"`, `"course:{id}"` |

### Ograniczenia Memory Cache

- **Tylko jedna instancja** aplikacji. Masz 3 repliki API na Kubernetes — każda ma **inny** cache.
- **Znika po restarcie** procesu.
- **Nie shared** między serwerami.

**Course Platform:** `GetCategories` jest dobrym kandydatem na cache — rzadko się zmienia, często czytany. Obecnie **bez cache** — ćwiczenie na przyszłość.

### Cache invalidation — „jeden z dwóch trudnych problemów w CS”

Gdy admin zmieni kategorię, musisz usunąć klucz z cache:

```csharp
_cache.Remove("categories");
// albo
_cache.Remove($"course:{courseId}");
```

Bez tego użytkownicy widzą stare dane do wygaśnięcia TTL.

---

## D2. Distributed Cache i Redis

### Po co Distributed Cache?

Gdy masz **wiele instancji** API albo chcesz cache **przeżywał restart** (z pewnymi zastrzeżeniami) — wspólna warstwa poza procesem.

ASP.NET abstrakcja: **`IDistributedCache`**.

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "CoursePlatform:";
});
```

Użycie (API jest inne niż Memory Cache — bajty, nie obiekty):

```csharp
public async Task<string?> GetCachedCourseAsync(Guid id)
{
    var key = $"course:{id}";
    var bytes = await _distributedCache.GetAsync(key);
    if (bytes != null)
        return Encoding.UTF8.GetString(bytes);

    var course = await LoadFromDb(id);
    await _distributedCache.SetAsync(key,
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(course)),
        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

    return JsonSerializer.Serialize(course);
}
```

Albo pakiet **`Microsoft.Extensions.Caching.StackExchangeRedis`** + wrapper extension methods.

### Redis — co to jest „jak dla debila”

**Redis** = bardzo szybka baza **klucz-wartość** w RAM (z opcjonalną persystencją na dysk).

Używana jako:
1. **Distributed cache** (powyżej)
2. **Session store** (sesje użytkowników między instancjami)
3. **Rate limiting** (liczniki requestów)
4. **Pub/Sub** (proste komunikaty między serwisami)
5. **Kolejki** (listy, streams — prostsze niż Kafka)
6. **Distributed lock** (jeden worker przetwarza job)

### Redis vs Memory Cache — tabela decyzyjna

| | Memory Cache | Redis |
|--|--------------|-------|
| Szybkość | Najszybszy (RAM procesu) | Bardzo szybki (sieć + RAM serwera Redis) |
| Współdzielony | Nie | Tak |
| Skala pozioma | Każda instancja osobno | Wspólny |
| Złożoność | Zero | Redis do postawienia, monitorowania |
| Kiedy | Mała app, 1 instancja | Produkcja, wiele replik, session, rate limit |

### EF Core 2nd Level Cache

Cache **wyników query** EF między requestami (np. biblioteka EFCoreSecondLevelCacheInterceptor). Rzadziej na start — najpierw `AsNoTracking`, dobre indeksy, Redis na hot path.

### Memcached

Starszy, prostszy cache klucz-wartość. W .NET spotkasz rzadziej niż Redis. Redis ma więcej struktur danych i funkcji.

### Redis w Course Platform (plan)

`IMPLEMENTATION_PLAN.md` wspomina Redis opcjonalnie. Typowy scenariusz:

```
GET /api/courses?page=1
  → sprawdź Redis key "courses:page:1:hash=filters"
  → miss → EF → zapisz w Redis na 60s
  → hit → zwróć z Redis
```

Invalidation przy `CreateCourse` / `UpdateCourse`: `InvalidateQueries` po stronie Vue **nie wystarczy** — backend musi czyścić Redis.

---

## D3. Elasticsearch — wyszukiwanie i analiza

### Po co Elasticsearch, skoro jest SQL `LIKE`?

SQL `LIKE '%vue%'`:
- Wolne na dużych tabelach (full scan lub słaby indeks).
- Słabe rankingowanie (trafność).
- Brak fuzzy search („vu” → „Vue”), synonimów, faceted search.

**Elasticsearch (ES)** = silnik wyszukiwania i analizy oparty na Apache Lucene. Indeksuje tekst, score’uje trafność, agreguje (facety: „ile kursów per kategoria”).

### Jak to działa w architekturze

```
[API] ──write──► [PostgreSQL]  (source of truth)
  │
  └──sync──► [Elasticsearch]   (kopia do wyszukiwania)
```

Przy `CreateCourse` / `UpdateCourse`:
1. Zapis do PostgreSQL (transakcja).
2. Opublikuj event lub bezpośrednio zindeksuj dokument w ES.

Przy `GET /api/courses?search=vue`:
1. Query do ES zamiast `EF.Functions.Like`.
2. ES zwraca ID + score.
3. Opcjonalnie doładuj szczegóły z SQL po ID.

### Przykład dokumentu w ES

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Vue 3 Masterclass",
  "description": "Naucz się Composition API...",
  "categories": ["Frontend", "JavaScript"],
  "price": 99.99,
  "publishedAt": "2026-01-15"
}
```

### NEST — klient .NET dla Elasticsearch

```csharp
var response = await _elasticClient.SearchAsync<CourseDocument>(s => s
    .Index("courses")
    .Query(q => q
        .MultiMatch(m => m
            .Fields(f => f.Field(c => c.Title).Field(c => c.Description))
            .Query(searchTerm)
            .Fuzziness(Fuzziness.Auto)
        )
    )
    .From((page - 1) * pageSize)
    .Size(pageSize)
);
```

### Solr, Sphinx

Alternatywy ES. ES dominuje w ekosystemie .NET + ELK stack (Elasticsearch, Logstash, Kibana). Junior: znać różnicę SQL search vs dedicated search engine.

### Course Platform dziś

Wyszukiwanie w `GetCoursesQuery` przez EF + `EF.Functions.Like` — OK na małą skalę. Przy tysiącach kursów → ES lub PostgreSQL Full Text Search (`to_tsvector`).

---

# Część E — Komunikacja

## E1. REST API — standard, który musisz znać

### Zasoby i URL

```
GET    /api/courses           — lista
GET    /api/courses/{id}      — jeden
POST   /api/courses           — utwórz
PUT    /api/courses/{id}      — zamień całość
PATCH  /api/courses/{id}      — częściowa zmiana
DELETE /api/courses/{id}      — usuń
```

### Konwencje

- Rzeczowniki w liczbie mnogiej (`courses`, nie `getCourses`).
- Id w URL, nie w body (dla PUT).
- Filtrowanie query string: `?page=1&pageSize=10&level=2`.
- Błędy jako JSON: `{ "error": "..." }` lub `{ "errors": { "Title": ["..."] } }`.

Course Platform: `CoursesController` — wzorcowy REST + MediatR.

### Wersjonowanie API

```
/api/v1/courses
/api/v2/courses
```

Albo header `Api-Version: 2`. Ważne w długo żyjących API.

### HATEOAS (opcjonalnie na junior)

Odpowiedź zawiera linki do powiązanych akcji. Rzadkie w praktyce SPA — znasz z teorii REST.

---

## E2. OData i Gridify

### OData

Standard Microsoftu: **jeden endpoint**, klient buduje query w URL:

```
GET /api/courses?$filter=Price lt 100&$orderby=Title&$top=10&$skip=20
```

W .NET: pakiet `Microsoft.AspNetCore.OData`.

**Plusy:** elastyczne filtrowanie bez 20 parametrów w kontrolerze.  
**Minusy:** złożoność, ryzyko drogich query, bezpieczeństwo (musisz ograniczyć co wolno).

### Gridify

Lżejsza alternatywa — string filter/sort z frontu:

```
GET /api/courses?filter=Price<100,Level==2&orderBy=Title
```

Pakiet `Gridify` + `IGridifyProcessor`. Popularne w mniejszych API.

### Course Platform

Ręczne parametry w `GetCoursesQuery` — czytelne, bezpieczne. OData/Gridify to refactor „gdy kontroler ma 30 parametrów”.

---

## E3. GraphQL i HotChocolate

### REST vs GraphQL — intuicja

**REST:** wiele endpointów, stałe kształty odpowiedzi.

```
GET /api/courses/1        → cały kurs + moduły + lekcje
GET /api/courses/1/reviews → osobno
```

Frontend często potrzebuje **innego** zestawu pól → over-fetching albo 5 requestów.

**GraphQL:** **jeden endpoint** (`POST /graphql`), klient mówi **dokładnie** co chce:

```graphql
query {
  course(id: "3fa85f64-...") {
    title
    instructor { firstName }
    modules {
      title
      lessons { title duration }
    }
  }
}
```

Odpowiedź = dokładnie ten kształt JSON. **Under-fetching** — jeden round-trip.

### Podstawowe pojęcia GraphQL

| Pojęcie | Co to |
|---------|-------|
| **Schema** | Kontrakt — typy Query, Mutation, Subscription |
| **Query** | Odczyt (jak GET) |
| **Mutation** | Zmiana (jak POST/PUT) |
| **Subscription** | Real-time push (WebSocket) |
| **Resolver** | Funkcja C# zwracająca pole |
| **N+1 problem** | Resolver na każdą lekcję = osobne query — rozwiązanie: **DataLoader** |

### HotChocolate — GraphQL w .NET

Najpopularniejszy serwer GraphQL dla ASP.NET Core.

```csharp
// Program.cs
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddProjections()
    .AddFiltering()
    .AddSorting();

app.MapGraphQL(); // endpoint /graphql
```

```csharp
public class Query
{
    public async Task<Course?> GetCourse(Guid id, [Service] IApplicationDbContext context)
        => await context.Courses.FindAsync(id);
}

public class Mutation
{
    public async Task<Course> CreateCourse(CreateCourseInput input, [Service] IMediator mediator)
        => ...;
}
```

**Banana Cake Pop** — UI do testów GraphQL (jak Swagger dla REST).

### Kiedy GraphQL, kiedy REST

| GraphQL | REST |
|---------|------|
| Mobile z wolnym netem, wiele widoków | Proste CRUD, publiczne API |
| Złożone agregacje danych | Cache HTTP (CDN) na GET |
| Jeden zespół fullstack ustala schema | Standard branżowy, łatwiejszy onboarding |

**Course Platform** = REST — właściwy wybór dla nauki i typowego SPA. GraphQL dodajesz, gdy zespół frontu prosi o elastyczność query.

### Bezpieczeństwo GraphQL

- Depth limiting (zagnieżdżenie max 5 poziomów).
- Complexity scoring (drogie query blokowane).
- Auth na resolverach (`[Authorize]`).

---

## E4. gRPC

### Co to?

**gRPC** = RPC over HTTP/2, dane w **Protocol Buffers** (binarnie, nie JSON).

**Plusy:** szybki, ścisły kontrakt (`.proto`), streaming dwukierunkowy.  
**Minusy:** przeglądarka bez proxy nie woła gRPC jak REST (trzeba gRPC-Web).

### Kiedy

- Komunikacja **serwis ↔ serwis** (microservices wewnętrzne).
- Niekiedy mobile ↔ backend.

**Nie** jako główne API dla Vue — REST/GraphQL wygodniejsze.

```protobuf
service CourseService {
  rpc GetCourse (GetCourseRequest) returns (CourseResponse);
}
```

```csharp
app.MapGrpcService<CourseGrpcService>();
```

Junior: „znam z nazwy, używane między mikroserwisami, front SPA zwykle REST”.

---

## E5. SignalR i WebSockets

### Problem, który rozwiązują

REST: klient **pyta** serwer (polling: `setInterval(() => fetch(...), 1000)` — brzydkie, obciąża sieć).

**WebSocket:** **stałe połączenie** — serwer może **pushować** dane do klienta.

**SignalR** = abstrakcja Microsoftu nad WebSocket + fallback (SSE, long polling).

### Przypadki użycia

- Czat na żywo
- Powiadomienia („nowy student zapisał się na kurs”)
- Live dashboard (statystyki)
- Współedycja dokumentu
- **Blazor Server** (cały UI przez SignalR)

### Przykład — hub

```csharp
public class NotificationHub : Hub
{
    public async Task JoinCourseGroup(string courseId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"course-{courseId}");

    public async Task SendProgressUpdate(string courseId, string message)
        => await Clients.Group($"course-{courseId}").SendAsync("ProgressUpdated", message);
}
```

```csharp
builder.Services.AddSignalR();
app.MapHub<NotificationHub>("/hubs/notifications");
```

### Klient Vue

```bash
npm install @microsoft/signalr
```

```typescript
import * as signalR from '@microsoft/signalr'

const connection = new signalR.HubConnectionBuilder()
  .withUrl('http://localhost:8080/hubs/notifications', {
    accessTokenFactory: () => authStore.token ?? ''
  })
  .build()

await connection.start()
await connection.invoke('JoinCourseGroup', courseId)
connection.on('ProgressUpdated', (msg) => { ... })
```

### Auth w SignalR

Token JWT często w query string (`?access_token=`) albo `accessTokenFactory` — bo WebSocket nie zawsze przenosi nagłówki jak fetch.

### Course Platform — gdzie by pasowało

- Live licznik studentów na kursie instruktora.
- Powiadomienie instruktora o nowej recenzji.
- Obecnie: front **odświeża przez Vue Query** (pull), nie push.

---

# Część F — Operacje w tle

## F1. `BackgroundService` i Hosted Services

### Po co?

Nie każda operacja mieści się w request HTTP:
- Wysyłka emaili
- Generowanie raportów PDF
- Synchronizacja z Elasticsearch
- Czyszczenie starych plików

### `BackgroundService` — wbudowane w ASP.NET

```csharp
public class EmailBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;

    public EmailBackgroundService(IServiceProvider services) => _services = services;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = _services.CreateAsyncScope();
            var queue = scope.ServiceProvider.GetRequiredService<IEmailQueue>();
            var job = await queue.DequeueAsync(stoppingToken);
            if (job != null)
                await ProcessEmailAsync(job);

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
```

```csharp
builder.Services.AddHostedService<EmailBackgroundService>();
```

### WAŻNE: Scoped w tle

`DbContext` jest **Scoped** (per request). W `BackgroundService` **nie masz** requestu — tworzysz **scope ręcznie**:

```csharp
await using var scope = _services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
```

Bez scope = używasz Singleton DbContext = **bug**.

### `IHostedService` vs `BackgroundService`

`BackgroundService` to wygodna baza z pętlą `ExecuteAsync`. Niższy poziom: `IHostedService` z `StartAsync` / `StopAsync`.

---

## F2. Hangfire, Quartz, Coravel

### Kiedy BackgroundService nie wystarcza

- Harmonogram **cron** („co noc o 3:00”)
- **Dashboard** do monitorowania jobów
- **Retry** po błędzie
- **Kolejka** z wieloma workerami
- Persystencja jobów (przeżyją restart)

### Hangfire

```csharp
builder.Services.AddHangfire(c => c.UsePostgreSqlStorage(connectionString));
builder.Services.AddHangfireServer();

// W kodzie
BackgroundJob.Enqueue(() => _emailService.SendWelcome(userId));
RecurringJob.AddOrUpdate("sync-es", () => _searchService.ReindexAll(), Cron.Daily);
```

**Dashboard** pod `/hangfire` — widzisz joby, failed, retry. Bardzo popularny w .NET.

### Quartz.NET

Enterprise scheduler — cron, triggery, klaster. Bardziej konfigurowalny, mniej „batteries included” niż Hangfire UI.

### Coravel

Lżejszy, „laravel-style” dla .NET:

```csharp
services.AddScheduler();
// ...
host.Services.UseScheduler(scheduler =>
{
    scheduler.Schedule<ReindexSearchJob>().Daily();
});
```

### Porównanie

| | BackgroundService | Hangfire | Quartz |
|--|-------------------|----------|--------|
| Prosty loop | Tak | Overkill | Overkill |
| Cron | Ręcznie | Tak | Tak |
| UI | Nie | Tak | Plugin |
| Kolejka jobów | Sam piszesz | Tak | Tak |
| Złożoność | Niska | Średnia | Wyższa |

### Course Platform — przykłady zadań w tle

| Zadanie | Etap | Narzędzie |
|---------|------|-----------|
| Seed bazy przy starcie | 1 | Już jest w `Program.cs` |
| Reindeks ES po zmianie kursu | 2+ | Hangfire Enqueue |
| Email po enroll | 3 | Hangfire + szablon |
| Czyszczenie wygasłych tokenów | 2+ | Coravel / Quartz cron |

---

# Część G — Jakość i architektura

## G1. Dependency Injection — dogłębnie

### Co robi kontener DI?

1. Przy starcie rejestrujesz: „gdy ktoś chce `ICourseRepository`, daj `EfCourseRepository`”.
2. Przy requestcie tworzy łańcuch zależności automatycznie.

### Cykle życia — musisz umieć wytłumaczyć na rozmowie

```csharp
services.AddTransient<IEmailSender, EmailSender>();     // nowy przy KAŻDYM inject
services.AddScoped<IApplicationDbContext, ApplicationDbContext>(); // 1 na HTTP request
services.AddSingleton<ICacheService, CacheService>();   // 1 na całą aplikację
```

| Lifetime | Analogia | Pułapka |
|----------|----------|---------|
| **Transient** | Nowa kartka na każde zapytanie | Nie injectuj DbContext do Singleton |
| **Scoped** | Jedna teczka na spotkanie (request) | Nie trzymaj Scoped w polu Singleton |
| **Singleton** | Jedna książka w biurze | Musi być thread-safe |

### Constructor injection — standard

```csharp
public class CreateCourseCommandHandler
{
    public CreateCourseCommandHandler(IApplicationDbContext context, ICurrentUserService user) { }
}
```

Nie używaj `new ApplicationDbContext()` w handlerze — psujesz testy i transakcje.

### Scrutor / assembly scanning

Ręczna rejestracja setek handlerów jest męcząca. MediatR robi:

```csharp
cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
```

Scrutor potrafi `services.Scan(scan => scan.FromAssembly...AddClasses...AsImplementedInterfaces)`).

### Keyed services (.NET 8+)

```csharp
services.AddKeyedScoped<IPaymentProvider, StripeProvider>("stripe");
services.AddKeyedScoped<IPaymentProvider, MockProvider>("mock");

// inject
public Handler([FromKeyedServices("stripe")] IPaymentProvider stripe) { }
```

---

## G2. Logowanie: Serilog, NLog

### Po co strukturalne logi?

`Console.WriteLine("Error: " + ex)` — nie da się przeszukać w produkcji.

**Serilog:**

```csharp
Log.Information("User {UserId} enrolled in course {CourseId}", userId, courseId);
```

JSON w pliku / Seq / Elasticsearch → filtry: `CourseId = xyz`.

### Course Platform

```csharp
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
```

`appsettings.json`:
```json
"Serilog": {
  "MinimumLevel": "Information",
  "WriteTo": [{ "Name": "Console" }]
}
```

### Poziomy

`Verbose` < `Debug` < `Information` < `Warning` < `Error` < `Fatal`

Produkcja: `Information` lub `Warning`. Dev: `Debug`.

### NLog

Alternatywa dla Serilog. Podobne możliwości. W nowych projektach częściej Serilog.

### Nie loguj sekretów

Hasła, pełne tokeny JWT, numery kart — **nigdy**.

---

## G3. Architektura oprogramowania

### Warstwy vs mikroserwisy

**Monolit warstwowy** (Course Platform):

```
API → Application → Domain
         ↑
   Infrastructure
```

Jeden deploy, jedna baza, prostszy dev. **Większość firm zaczyna tak.**

**Mikroserwisy:** osobne deploye (Courses API, Payments API, Notifications). Osobne bazy. Komunikacja HTTP/gRPC/Kafka. Koszt operacyjny ogromny — nie na portfolio juniora.

### Clean Architecture — zasady

1. **Domain** nie zna EF, HTTP, JWT.
2. **Application** definiuje use case’y i interfejsy (`IApplicationDbContext`).
3. **Infrastructure** implementuje (EF, email, MinIO).
4. **API** tylko transport.

**Dlaczego:** testujesz logikę bez bazy; wymieniasz PostgreSQL na SQL Server zmianą Infrastructure.

### Inne wzorce — panorama

| Wzorzec | Idea |
|---------|------|
| **Repository** | Abstrakcja nad `DbSet` — w CQRS często zbędny, bo `DbContext` już jest UoW |
| **Unit of Work** | `DbContext` = UoW w EF |
| **CQRS** | Osobne modele/commands do zapisu i odczytu |
| **Mediator** | MediatR — rozłącza „kto woła” od „kto wykonuje” |
| **Outbox** | Zapisz event do tabeli w tej samej transakcji co dane → worker wysyła do Kafki |
| **Saga** | Rozproszona transakcja przez wiele serwisów |

### Vertical Slice Architecture

Organizacja **po funkcji**, nie po warstwie:

```
Features/
  CreateCourse/
    CreateCourseCommand.cs
    CreateCourseHandler.cs
    CreateCourseValidator.cs
```

Course Platform używa tego w `Application/Features/` — połączenie Clean Architecture + vertical slices.

### Polly — odporność

Retry, circuit breaker przy wołaniu zewnętrznych API:

```csharp
var policy = Policy.Handle<HttpRequestException>().WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(i));
await policy.ExecuteAsync(() => _httpClient.GetAsync(url));
```

---

## G4. CQRS, MediatR, FluentValidation

### CQRS w skrócie

- **Command** zmienia stan → `CreateCourseCommand` → `Guid`
- **Query** czyta → `GetCoursesQuery` → `CoursesVm`

Osobne handlery = czytelność, łatwe testy, różne modele odczytu/zapisu.

### MediatR pipeline

```
Request → TrimmingBehaviour → ValidationBehaviour → Handler → Response
```

`ValidationBehaviour` — automatyczna walidacja FluentValidation przed handlerem.

### FluentValidation

```csharp
public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
```

### JWT + Identity (Course Platform)

Login → `UserManager` + `SignInManager` → `JwtTokenGenerator` → token w Vue → `Authorization: Bearer` → `UseAuthentication` → `CurrentUserService.UserId`.

### Resource-based authorization

Role `Instructor` ≠ dostęp do **cudzego** kursu. Sprawdzenie w `CourseAccessHelper.CanAccessCourseContentAsync` — enrollment lub właściciel lub admin.

---

## G5. Testowanie

### Piramida testów

```
        /\
       /E2E\        mało, wolne (Playwright)
      /------\
     /Integr.\     średnio (WebApplicationFactory)
    /----------\
   /  Unit      \  dużo, szybkie (handlery, validatory)
  /--------------\
```

### Unit test — handler

```csharp
[Fact]
public async Task GetCourses_NonAdmin_SeesOnlyPublished()
{
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
    await using var context = new ApplicationDbContext(options);
    // seed Draft + Published courses
    var handler = new GetCoursesQueryHandler(context, MockNonAdminUser());
    var result = await handler.Handle(new GetCoursesQuery(...), CancellationToken.None);
    result.Items.Should().OnlyContain(c => c.Status == CourseStatus.Published);
}
```

### Integration test — HTTP

```csharp
public class CoursesApiTests : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetCourses_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/courses?pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

`WebApplicationFactory<Program>` — prawdziwa app w pamięci, środowisko `Testing`, InMemory DB.

### Mocking — Moq / NSubstitute

```csharp
var mockUser = new Mock<ICurrentUserService>();
mockUser.Setup(u => u.UserId).Returns(Guid.NewGuid());
```

### Testcontainers

Prawdziwy PostgreSQL w Dockerze na czas testu — bliżej produkcji niż InMemory.

```csharp
var container = new PostgreSqlBuilder().Build();
await container.StartAsync();
```

### Co testować jako junior

- Validatory (reguły biznesowe w input)
- Handlery (logika z InMemory DB)
- 1–2 integration testy na kontroler
- **Nie** testuj frameworka (czy ASP.NET routuje — to robi Microsoft)

### AutoMapper / Mapperly

Mapowanie encja → DTO. Course Platform mapuje **ręcznie** w LINQ `Select` — lepsze dla nauki i kontroli. AutoMapper:

```csharp
CreateMap<Course, CourseDto>();
```

Mapperly — source generator, szybszy, compile-time. Znasz z roadmapy — nie musisz używać w każdym projekcie.

---

# Część H — Produkcja

## H1. Docker i konteneryzacja

### Po co Docker w .NET

„U mnie działa” → ten sam obraz na dev/stage/prod.

Course Platform:
- `cp_api` — .NET SDK, `dotnet watch run`, port 8080
- `cp_db` — PostgreSQL 16
- `cp_frontend` — Node, Vite, port 5173

### Connection string w Docker

Z kontenera API host bazy to **`db`** (nazwa serwisu Compose), nie `localhost`.

### Multi-stage Dockerfile (produkcja)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "CoursePlatform.API.dll"]
```

Obraz runtime bez SDK — mniejszy, bezpieczniejszy.

### Kubernetes (panorama)

Orkiestracja wielu kontenerów — repliki, load balancing, secrets. Junior: wiedzieć, że K8s uruchamia pody z obrazów Docker; szczegóły na później.

---

## H2. Microservices, message brokers — panorama

### Mikroserwisy — kiedy NIE

Na start kariery i portfolio: **monolit warstwowy wystarczy**. Mikroserwisy gdy zespół i skala rosną, niezależne deploye są konieczne.

### Message brokers — po co

Serwis A zrobił coś ważnego → Serwis B ma zareagować **asynchronicznie** bez czekania w HTTP.

```
[Courses API] --publish--> [RabbitMQ/Kafka] --consume--> [Search Service]
                                                      --> [Email Service]
```

### Technologie z roadmapy

| Broker | Charakterystyka |
|--------|-----------------|
| **RabbitMQ** | Klasyczny queue, łatwy start |
| **Kafka** | Log eventów, wysoka przepustowość, stream processing |
| **Azure Service Bus** | Managed w Azure |
| **MassTransit** | Abstrakcja .NET nad brokerami |

### API Gateway (Ocelot, YARP)

Jeden punkt wejścia `api.mojafirma.pl` → routuje do wewnętrznych serwisów, SSL termination, rate limit.

Junior: „monolit na start; brokery gdy rozdzielam odpowiedzialności i potrzebuję async”.

---

## H3. CI/CD — co junior powinien wiedzieć

Pipeline typowy:

```yaml
# GitHub Actions (uproszczone)
- dotnet restore
- dotnet build
- dotnet test
- docker build & push
- deploy to Azure / AWS
```

**PR check:** build + test muszą przejść. Course Platform: `docker compose exec api dotnet test`.

---

# Część I — Integracja z frontendem

## I1. ASP.NET Core + Vue (lub inny SPA)

### Model mentalny

```
[Przeglądarka]
   Vue SPA (5173)  ----HTTP JSON---->  ASP.NET API (8080)
                                              |
                                         PostgreSQL
```

Vue **nie jest** częścią ASP.NET. To dwa programy. Łączy je:
1. **HTTP** (REST)
2. **CORS** (zezwolenie cross-origin)
3. **JWT** (auth)

### Warstwy frontu (lustro backendu)

```
.vue komponent
  → composable (useCourses) — Vue Query, stan UI
    → courses.api.ts — czyste HTTP
      → client.ts — axios, baseURL, Bearer token
```

### Pinia vs Vue Query

| | Pinia | Vue Query |
|--|-------|-----------|
| Token, user | Tak | Nie |
| Lista kursów z API | Nie | Tak |
| Cache serwera | Nie | Tak |

### Serializacja JSON

C# `Title` → JSON `title` (camelCase domyślnie w ASP.NET Core). Vue wysyła `{ "title": "..." }` → binduje na `CreateCourseCommand.Title`.

### Obsługa błędów

Backend `ExceptionHandlingMiddleware` → `{ "errors": { "Title": ["..."] } }`. Front `getApiErrorMessage` → toast.

### SignalR + Vue

Patrz [E5](#e5-signalr-i-websockets) — `@microsoft/signalr` gdy potrzebujesz push zamiast pollingu.

---

# Część J — Praktyka

## J1. Course Platform — mapa przykładów

| Temat z kursu | Gdzie w projekcie | Status |
|---------------|-------------------|--------|
| Middleware pipeline | `Program.cs`, `ExceptionHandlingMiddleware` | ✅ |
| REST kontrolery | `API/Controllers/*` | ✅ |
| EF Code First | `Infrastructure/Persistence`, `Migrations/` | ✅ |
| Fluent API, unique index | `EnrollmentConfiguration` | ✅ |
| DI Scoped/Singleton | `Infrastructure/DependencyInjection.cs` | ✅ |
| CQRS + MediatR | `Application/Features/**` | ✅ |
| FluentValidation | `*Validator.cs`, `ValidationBehaviour` | ✅ |
| JWT + Identity | `Program.cs`, `JwtTokenGenerator`, `AuthController` | ✅ |
| Resource auth | `CourseAccessHelper`, `GetLessonQuery` | ✅ |
| Serilog | `Program.cs` | ✅ |
| Rate limiting | `Program.cs` | ✅ |
| CORS | `Program.cs` | ✅ |
| Unit + integration tests | `tests/` | ✅ |
| Docker Compose | `docker-compose.yml` | ✅ |
| Vue + axios + JWT | `frontend/src/shared/api/client.ts` | ✅ |
| PostgreSQL (nie SQL Server) | `UseNpgsql` | ✅ |
| Memory / Redis cache | — | ❌ plan |
| Elasticsearch | — | ❌ plan (dziś EF Like) |
| GraphQL / HotChocolate | — | ❌ |
| SignalR | — | ❌ |
| Hangfire / Quartz | — | ❌ |
| MinIO storage | `IFileStorageService` | ⚠️ interfejs |
| Message brokers | — | ❌ |

### Mini-przykłady „otwórz i śledź”

1. **Middleware:** `GET /api/courses` → log Serilog → CORS → auth (anonymous OK) → kontroler.
2. **Command:** `POST /api/courses` + Bearer Instructor → `ValidationBehaviour` → `CreateCourseCommandHandler` → INSERT.
3. **Forbidden:** student bez enroll → `GET lesson` → `CourseAccessHelper` → 403 JSON.
4. **Front:** `LoginForm` → `useAuth` → token w Pinia → kolejny request z `Authorization`.

---

## J2. Plan nauki na 12 tygodni (wg priorytetu niebieskich z roadmapy)

Każdy tydzień kończy się: *„czy potrafię wytłumaczyć to komuś przy tablicy?”*

### Tygodnie 1–2 — Fundamenty (niebieskie: C#, .NET, CLI, HTTP, Git)
- A1–A4 + **K1** (Git).
- **Ćwiczenie:** 5 commitów z feature branch + PR na GitHubie.

### Tygodnie 3–4 — ASP.NET Core (niebieskie: Middleware, Filters, REST, Minimal APIs, App Settings)
- B1–B5 + **K2** (StyleCop), **K3** (Microsoft.Extensions).
- **Ćwiczenie:** middleware mierzący czas + własny filter logujący akcję.

### Tygodnie 5–7 — Dane (niebieskie: EF Core, Change Tracker, SQL Server, Constraints)
- C1–C2 + **K9**, **K10**.
- Prześledź migracje Course Platform.
- **Ćwiczenie:** nowa encja + migracja + unique constraint.

### Tygodnie 8–9 — DI, cache, architektura (niebieskie: DI lifecycles, Memory Cache, Redis, MediatR, FluentValidation)
- G1, G3, G4 + D1, D2.
- **Ćwiczenie:** Redis w Dockerze + cache kategorii.

### Tygodnie 10 — Komunikacja (niebieskie: REST, HotChocolate, SignalR, Elasticsearch, Gridlify)
- E1–E3, E5, D3, E2 — teoria + mini demo poza projektem.
- **Nie** ucz GraphQL node (alternative) — ucz **HotChocolate** (niebieskie).

### Tygodnie 11 — Tło i integracje (niebieskie: BackgroundService, Hangfire, RabbitMQ, MassTransit)
- F1–F2 + **K5**, **K6**.

### Tydzień 12 — Testy i produkcja (niebieskie: xUnit, Shouldly, Moq, AutoFixture, WebApplicationFactory, Testcontainers, Playwright, Docker, GitHub Actions)
- G5, H1, H3 + **K7**, **K8**.
- `docker compose exec api dotnet test`.

### Po 12 tygodniach — alternative/optional tylko „on demand”
Dapper, gRPC, Quartz, NLog, OData — gdy pojawią się w pracy.

---

## J3. Checklist juniora .NET

Po tym kursie powinieneś umieć wytłumaczyć:

- [ ] Co robi `Program.cs` od `CreateBuilder` do `Run`
- [ ] Kolejność middleware i dlaczego auth przed authorization
- [ ] Różnica Transient / Scoped / Singleton z przykładem DbContext
- [ ] Co to `AsNoTracking`, `Include`, N+1
- [ ] Jak działa migracja EF Code First
- [ ] Connection string SQL Server vs Npgsql
- [ ] Kiedy Memory Cache vs Redis
- [ ] Po co Elasticsearch obok SQL
- [ ] REST vs GraphQL — trade-offy
- [ ] Po co SignalR zamiast pollingu
- [ ] BackgroundService vs Hangfire
- [ ] CQRS + po co MediatR
- [ ] JWT flow od loginu do `[Authorize]`
- [ ] Unit vs integration test
- [ ] CORS i dlaczego Vue potrzebuje go do API
- [ ] Clean Architecture — co w której warstwie
- [ ] **Git:** branch, merge/rebase, PR, code review
- [ ] **StyleCop / analyzers** — po co w zespole
- [ ] **Scalar vs Swagger** — dokumentacja API
- [ ] **MassTransit + RabbitMQ** — publish/consume w 3 zdaniach
- [ ] **YARP vs Ocelot** — API gateway
- [ ] **Shouldly** zamiast FluentAssertions (licencja 2025+)
- [ ] **Playwright** — jeden test E2E login flow

---

# Część K — Uzupełnienia z audytu

## K1. Git i współpraca (rekomendowane)

Roadmapa traktuje Git jako **niezbędny** — nie „miły dodatek”.

### Minimum dla juniora .NET

| Operacja | Po co |
|----------|-------|
| `git clone`, `pull`, `push` | Codzienna praca |
| Branch `feature/xyz` | Izolacja zmian |
| Commit message po angielsku | Historia czytelna (jak w Course Platform) |
| Pull Request | Code review przed merge |
| Rozwiązywanie konfliktów | Dwa równoległe feature |

### GitHub vs GitLab vs BitBucket

| Platforma | Typowy kontekst |
|-----------|------------------|
| **GitHub** | Open source, startupy, Actions CI |
| **GitLab** | Self-hosted, cały DevOps w jednym |
| **BitBucket** | Firmy Atlassian (Jira obok) |

**Workflow identyczny** — uczysz się Gita, platforma to UI + CI.

### Co łączy się z ASP.NET

- `.gitignore` — nie commituj `bin/`, `obj/`, `.env`, `appsettings.Development.json` z sekretami.
- PR check: `dotnet build` + `dotnet test` (GitHub Actions — niebieskie na mapie).

---

## K2. StyleCop i jakość kodu C#

**StyleCop** = reguły formatowania i stylu C# (nawiasy, kolejność usingów, nazewnictwo).

### Jak to działa w praktyce 2026

Zamiast starego StyleCop.Analyzers często:

```xml
<PropertyGroup>
  <EnableNETAnalyzers>true</EnableNETAnalyzers>
  <AnalysisLevel>latest</AnalysisLevel>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>
```

+ plik `.editorconfig` w root repozytorium — **jeden standard dla całego zespołu**.

### Po co juniorowi

- PR nie odrzucony za „dodaj spację”.
- Te same reguły w Rider / VS / VS Code.
- Roadmapa wymienia StyleCop — na rozmowie: *„używamy analyzerów + EditorConfig”*.

---

## K3. Microsoft.Extensions — co to jest

**Microsoft.Extensions.*** to rodzina pakietów NuGet, na której stoi ASP.NET Core:

| Pakiet | Co daje |
|--------|---------|
| `Microsoft.Extensions.DependencyInjection` | Kontener DI |
| `Microsoft.Extensions.Configuration` | appsettings + env |
| `Microsoft.Extensions.Logging` | Abstrakcja logów (Serilog pod spodem) |
| `Microsoft.Extensions.Hosting` | `IHost`, `BackgroundService` |
| `Microsoft.Extensions.Caching.Memory` | `IMemoryCache` |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | Redis cache |
| `Microsoft.Extensions.Http` | `IHttpClientFactory` |

Gdy w `Program.cs` piszesz `builder.Services.Add...` — to **Microsoft.Extensions** + ASP.NET.

### IHttpClientFactory — must know

```csharp
builder.Services.AddHttpClient<IPaymentClient, StripePaymentClient>();

public class StripePaymentClient(HttpClient http) : IPaymentClient
{
    public Task<HttpResponseMessage> ChargeAsync(...) =>
        http.PostAsJsonAsync("/charge", ...);
}
```

**Dlaczego nie `new HttpClient()`:** socket exhaustion, problemy z DNS. Factory zarządza pulą.

---

## K4. Scalar — dokumentacja API w 2026

Course Platform używa **Swagger** (`AddSwaggerGen`) — wciąż OK.

**Scalar** (niebieskie na mapie) — nowoczesny UI OpenAPI:

```csharp
// NuGet: Scalar.AspNetCore
app.MapOpenApi();
app.MapScalarApiReference();
```

Zalety: ładniejszy UI, lepsze UX niż Swagger UI, rośnie w projektach .NET 8/9.

**Junior:** umiesz otworzyć dokumentację API i wywołać endpoint z tokenem — narzędzie drugorzędne.

---

## K5. RabbitMQ + MassTransit (rekomendowane)

Oba są **niebieskie** — roadmapa mówi: ucz się kolejek przez **MassTransit** (abstrakcja .NET), broker często **RabbitMQ**.

### Scenariusz

Po `EnrollCommand` chcesz wysłać email — **nie** w handlerze synchronicznie (wolne, pada przy błędzie SMTP).

```
[API] --publish EnrolledEvent--> [RabbitMQ] --consume--> [Email Worker]
```

### MassTransit — minimalny przykład

```csharp
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<EnrolledEmailConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h => { h.Username("guest"); h.Password("guest"); });
        cfg.ConfigureEndpoints(context);
    });
});

public record EnrolledEvent(Guid UserId, Guid CourseId);

public class EnrolledEmailConsumer : IConsumer<EnrolledEvent>
{
    public async Task Consume(ConsumeContext<EnrolledEvent> context)
    {
        // wyślij email
    }
}
```

W handlerze: `await _publishEndpoint.Publish(new EnrolledEvent(...))`.

**Course Platform dziś:** brak brokera — enroll robi wszystko w jednej transakcji HTTP. To poprawne na Etap 1.

---

## K6. Ocelot i YARP — API Gateway

### Po co gateway w mikroserwisach

Jeden publiczny URL:

```
https://api.firma.pl/courses/*  → Courses Service
https://api.firma.pl/payments/* → Payments Service
```

+ auth na bramie, rate limit, SSL.

### Ocelot (niebieskie)

Konfiguracja JSON:

```json
{
  "Routes": [
    {
      "UpstreamPathTemplate": "/courses/{everything}",
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamHostAndPorts": [{ "Host": "courses-service", "Port": 80 }]
    }
  ]
}
```

### YARP (niebieskie, Microsoft)

Reverse proxy w kodzie:

```csharp
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
app.MapReverseProxy();
```

**2026:** YARP częściej w stacku Microsoft/Azure. Ocelot nadal spotykany — **oba** warto znać z roadmapy.

**Monolit (Course Platform):** gateway **niepotrzebny**.

---

## K7. Playwright + SpecFlow — testy E2E i BDD

### Playwright (niebieskie)

Automatyzacja przeglądarki — test **całego systemu** (Vue + API):

```csharp
[Fact]
public async Task Login_Shows_Courses()
{
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync();
    var page = await browser.NewPageAsync();
    await page.GotoAsync("http://localhost:5173/login");
    await page.FillAsync("[data-testid=email]", "student@test.com");
    await page.FillAsync("[data-testid=password]", "Test123!");
    await page.ClickAsync("[data-testid=submit]");
    await Expect(page).ToHaveURLAsync("**/courses");
}
```

**Cypress, Puppeteer** — alternative na mapie; w .NET 2026 ucz **Playwright**.

### SpecFlow (niebieskie)

BDD — scenariusze w języku naturalnym (Gherkin):

```gherkin
Scenario: Student enrolls in course
  Given I am logged in as a student
  When I enroll in course "Vue 3 Basics"
  Then I should see the course in "My Courses"
```

SpecFlow wiąże kroki z kodem C#. Używane w zespołach z analitykami / QA. **Light BDD** — alternative.

---

## K8. AutoFixture, Shouldly, Testcontainers — testy (rekomendowane)

### Shouldly (zamiast FluentAssertions)

Roadmapa **usunęła FluentAssertions** (płatna licencja komercyjna od 2025). **Shouldly** jest niebieskie:

```csharp
result.Items.Count.ShouldBe(3);
result.Items.ShouldContain(c => c.Title == "Vue 3");
```

Course Platform już używa Shouldly w testach integracyjnych.

### Moq (niebieskie)

```csharp
var mock = new Mock<ICurrentUserService>();
mock.Setup(x => x.UserId).Returns(userId);
mock.Setup(x => x.IsAdmin).Returns(false);
```

### AutoFixture (niebieskie)

Generuje losowe, sensowne dane testowe:

```csharp
var fixture = new Fixture();
var command = fixture.Create<CreateCourseCommand>();
// szybkie testy bez ręcznego wypełniania 10 pól
```

### Testcontainers (niebieskie)

Prawdziwy PostgreSQL w Dockerze na czas testu — lepsze niż InMemory (InMemory nie sprawdza constraintów SQL):

```csharp
await using var db = new PostgreSqlBuilder().Build();
await db.StartAsync();
// podłącz connection string do WebApplicationFactory
```

**Priorytet testów w Course Platform:** xUnit + Shouldly + WebApplicationFactory ✅. Dołożyć: Moq w unit, Testcontainers gdy InMemory za mało.

---

## K9. DSA i Database Design — minimum dla juniora .NET

Ogarniasz bazy — tu **tylko to, co łączy się z backendem .NET**.

### Database Design Basics (niebieskie)

- Normalizacja (1NF–3NF) — unikasz duplikacji danych.
- Klucze obce i **constraints** — w EF: Fluent API (`HasIndex`, `IsUnique`, `OnDelete`).
- Indeksy pod query z handlerów (`WHERE Status = Published`, `ORDER BY CreatedAt`).

### DSA (niebieskie) — minimum

Nie musisz być na LeetCode Hard. Musisz:

| Struktura | Gdzie w .NET |
|-----------|--------------|
| `List<T>`, `Dictionary<K,V>` | Cache, mapowania |
| `HashSet<T>` | `completedLessonIds` w `GetCourseDetailsQuery` |
| `Queue<T>` | Kolejki w pamięci, MassTransit |
| Złożoność O(n) | Nie rób `ToList()` + pętla zamiast jednego SQL |

**Algorytmy na rozmowie junior .NET:** raczej sortowanie/filtrowanie LINQ niż drzewa B+.

---

## K10. EF „Framework Basics” — co oznacza ten węzeł

Na mapie to podgałąź pod **Entity Framework Core**. Oznacza:

1. **DbContext** — czym jest, lifetime Scoped.
2. **DbSet<T>** — repozytorium tabeli.
3. **LINQ to Entities** — zapytania tłumaczone na SQL.
4. **Migrations** — ewolucja schematu.
5. **Konwencje** — nazwy tabel, klucze, relacje domyślne.
6. **Fluent API** — nadpisanie konwencji (jak `EnrollmentConfiguration`).

To **nie** jest osobna biblioteka — to fundament EF Core z sekcji C1.

---

## J4. Słownik

| Termin | Definicja |
|--------|-----------|
| **ASP.NET Core** | Framework webowy Microsoftu dla .NET |
| **Kestrel** | Serwer HTTP |
| **Middleware** | Komponent pipeline HTTP |
| **Endpoint** | URL + metoda HTTP obsługiwana przez aplikację |
| **ORM** | Mapowanie obiekt ↔ tabele SQL |
| **DbContext** | Sesja EF z bazą |
| **Migration** | Wersjonowana zmiana schematu bazy |
| **DI** | Wstrzykiwanie zależności |
| **CQRS** | Rozdzielenie zapisu i odczytu |
| **DTO** | Obiekt pod transfer przez API |
| **JWT** | Token JSON podpisany kryptograficznie |
| **Claim** | Para klucz-wartość w tokenie |
| **CORS** | Zgoda przeglądarki na cross-origin HTTP |
| **Cache TTL** | Czas życia wpisu w cache |
| **Redis** | Baza klucz-wartość w pamięci, cache rozproszony |
| **Elasticsearch** | Silnik wyszukiwania pełnotekstowego |
| **GraphQL** | Język zapytań, jeden endpoint |
| **Resolver** | Funkcja zwracająca pole GraphQL |
| **SignalR** | Real-time push nad WebSocket |
| **Hub** | Punkt końcowy SignalR dla grup klientów |
| **gRPC** | RPC binarny over HTTP/2 |
| **Hangfire** | Kolejka i harmonogram jobów .NET |
| **WebApplicationFactory** | Testowanie ASP.NET in-memory |
| **Outbox pattern** | Niezawodne publikowanie eventów z transakcji DB |

---

## Materiały zewnętrzne (oficjalne)

- [Microsoft Learn — ASP.NET Core](https://learn.microsoft.com/aspnet/core)
- [EF Core dokumentacja](https://learn.microsoft.com/ef/core)
- [HotChocolate](https://chillicream.com/docs/hotchocolate)
- [SignalR](https://learn.microsoft.com/aspnet/core/signalr/introduction)
- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)
- [roadmap.sh ASP.NET Core](https://roadmap.sh/aspnet-core)

---

*Kurs żywy — rozszerzaj notatki własnymi przykładami z pracy i z Course Platform. Projekt jest laboratorium; roadmapa jest mapą całego ekosystemu, który poznajesz przez lata kariery.*

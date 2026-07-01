# 03 — Pierwsza aplikacja: host, `Program.cs`, konfiguracja

> **Poziom:** 🟢 podstawy · **Czas:** ~75 min · **Wymaga:** [01](./01-dotnet-i-csharp-od-zera.md), [02](./02-http-rest-i-jak-dziala-web-api.md)

## Po co ci to

Każda aplikacja ASP.NET Core zaczyna się w jednym pliku: **`Program.cs`**. To tam rejestrujesz usługi, budujesz
aplikację, ustawiasz pipeline i startujesz serwer. Zrozumienie tego pliku to zrozumienie „szkieletu" każdego
backendu w .NET. Dodatkowo poznasz **konfigurację** (appsettings/env/secrets) — bo prędzej czy później będziesz
musiał dołożyć klucz API albo connection string i nie chcesz go wkleić na sztywno w kod.

## Mostek z tego, co już znasz

- W Node/Express masz `const app = express(); app.use(...); app.listen(3000)`. W ASP.NET Core to samo:
  `builder` → `app` → `app.Run()`.
- W Laravelu masz `config/` + `.env` + Service Provider (rejestracja usług). W ASP.NET: `appsettings.json` +
  zmienne środowiskowe + `builder.Services.Add...`.

Pojęciowo nic nowego: **konfiguruję → rejestruję usługi → ustawiam middleware → startuję**.

---

## .NET CLI — komendy, które musisz znać

W Course Platform odpalasz je **wewnątrz kontenera** (`docker compose exec api ...`), ale to zwykłe komendy .NET:

```bash
dotnet new webapi -n MyApi   # nowy projekt Web API (szkielet)
dotnet build                 # kompilacja
dotnet run                   # uruchomienie
dotnet test                  # testy
dotnet add package Serilog   # dodaj paczkę z NuGet
dotnet ef migrations add X   # migracja EF (wymaga narzędzia dotnet-ef)
```

**NuGet** to menedżer paczek .NET (odpowiednik npm/composer). `MediatR`, `Serilog`, `FluentValidation` — wszystko
stamtąd. Paczki lądują w pliku projektu `.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>          <!-- null-safety włączone -->
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="MediatR" Version="14.1.0" />
  </ItemGroup>
</Project>
```

- `Sdk="...Web"` — to projekt webowy (ASP.NET Core). Biblioteki (Domain, Application) mają zwykły `Microsoft.NET.Sdk`.
- `TargetFramework` — wersja .NET (`net9.0`).
- `Nullable enable` — włącza ostrzeżenia o `null` (patrz [01](./01-dotnet-i-csharp-od-zera.md)).

---

## Cykl życia aplikacji

```
1. dotnet run
2. WebApplication.CreateBuilder(args)  → tworzy "builder"
3. builder.Services.Add...             → REJESTRUJESZ usługi (DI)
4. var app = builder.Build()           → budujesz aplikację
5. app.Use... / app.Map...             → USTAWIASZ pipeline (middleware + routing)
6. app.Run()                           → Kestrel nasłuchuje na porcie, aplikacja żyje
```

Dwie fazy, które musisz rozróżniać:

- **Faza rejestracji** (przed `Build()`): mówisz „jakie usługi istnieją" — `builder.Services.Add...`.
- **Faza pipeline** (po `Build()`): mówisz „jak obsłużyć żądanie" — `app.Use...`, `app.Map...`.

Kolejność w drugiej fazie **ma znaczenie** (o tym cały rozdział [04](./04-middleware-filtry-atrybuty.md)).

**Kestrel** to wbudowany, szybki serwer HTTP .NET — to on faktycznie nasłuchuje na porcie (8080 w Course
Platform). Nie mylić z nginx (który w produkcji może stać przed Kestrelem jako reverse proxy).

---

## `Program.cs` w Course Platform (uproszczony)

Prawdziwy plik to [src/CoursePlatform.API/Program.cs](../src/CoursePlatform.API/Program.cs). Szkielet:

```csharp
var builder = WebApplication.CreateBuilder(args);

// --- FAZA REJESTRACJI (DI) ---
builder.Services.AddApplication();                              // MediatR, FluentValidation, behaviors
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment); // EF, JWT, serwisy
builder.Services.AddControllers();                             // kontrolery MVC/API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();                             // dokumentacja API
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(...)...;  // ASP.NET Identity
builder.Services.AddAuthentication(...).AddJwtBearer(...);     // weryfikacja tokenów JWT
builder.Services.AddAuthorization();
builder.Services.AddCors(...);                                // zgoda na żądania z Vue (5173)
builder.Services.AddRateLimiter(...);                         // limity żądań

var app = builder.Build();

// --- FAZA PIPELINE (kolejność się liczy!) ---
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

// migracje + seed ról przy starcie
using (var scope = app.Services.CreateScope()) { /* MigrateAsync + SeedRolesAsync */ }

app.Run();
```

Zwróć uwagę na dwie własne metody rozszerzające: `AddApplication()` i `AddInfrastructure()`. To **extension
methods** grupujące rejestracje per warstwa (żeby `Program.cs` nie miał 300 linii). Zobaczysz ich środek w
[08](./08-dependency-injection.md) i [09](./09-architektura-clean-cqrs-mediatr.md).

> `WebApplication.CreateBuilder` przy okazji ładuje konfigurację: `appsettings.json`, `appsettings.{Env}.json`,
> zmienne środowiskowe i argumenty CLI — z priorytetami (o tym niżej).

---

## Konfiguracja — appsettings, env, secrets

### Hierarchia (co nadpisuje co)

```
appsettings.json                    (baza, commitowana)
  └── appsettings.Development.json  (nadpisuje w dev)
        └── zmienne środowiskowe    (nadpisują — tu wchodzi .env / Docker)
              └── argumenty CLI      (najwyższy priorytet)
```

Późniejsze źródło **wygrywa**. Dzięki temu ten sam `appsettings.json` działa wszędzie, a różnice
(hasła, connection string) wstrzykujesz środowiskiem.

### Jak czytać zmienną zagnieżdżoną

W JSON:

```json
{ "Jwt": { "Issuer": "CoursePlatform", "ExpiryMinutes": 120 } }
```

W kodzie czytasz przez `:` (dwukropek jako separator poziomów):

```csharp
var issuer = builder.Configuration["Jwt:Issuer"];
var minutes = builder.Configuration.GetValue<int>("Jwt:ExpiryMinutes", 120); // z wartością domyślną
```

W zmiennej środowiskowej ten sam klucz zapisujesz z **podwójnym podkreśleniem** (bo w wielu shellach nie ma `:`):

```
Jwt__Key=super-tajny-klucz-min-32-znaki
Jwt__Issuer=CoursePlatform
```

> W Course Platform sekrety (`Jwt:Key`, connection string) **nie są** w `appsettings.json` — wchodzą przez `.env`
> / zmienne środowiskowe kontenera. `appsettings.json` trzyma tylko rzeczy niewrażliwe (poziomy logów, CSP).

### Options pattern — poprawny sposób na konfigurację

Zamiast wołać `Configuration["Jwt:Key"]` po całym kodzie (literówki, brak typów), mapujesz sekcję na klasę:

```csharp
public class JwtSettings
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "";
    public int ExpiryMinutes { get; set; } = 120;
}

// Program.cs — rejestracja
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// użycie — wstrzykujesz IOptions<JwtSettings>
public class JwtTokenGenerator(IOptions<JwtSettings> options)
{
    private readonly JwtSettings _settings = options.Value;
}
```

Zalety: **typowanie**, jedno miejsce prawdy, walidacja przy starcie (`ValidateOnStart`), łatwe testy.

> Course Platform czyta JWT bezpośrednio z `IConfiguration` w `JwtTokenGenerator` — działa, ale Options pattern
> to wersja „bardziej pro". To dobry temat na ćwiczenie refaktoryzacyjne (patrz niżej).

### Walidacja konfiguracji przy starcie

Course Platform **nie ufa**, że sekrety są poprawne — sprawdza je zanim aplikacja wstanie (fragment `Program.cs`):

```csharp
static void ValidateJwtConfiguration(IConfiguration configuration)
{
    var jwtKey = configuration["Jwt:Key"];
    if (string.IsNullOrWhiteSpace(jwtKey))
        throw new InvalidOperationException("Jwt:Key is not configured.");
    if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
        throw new InvalidOperationException("Jwt:Key must be at least 32 bytes for HMAC-SHA256.");
    // + sprawdzenie placeholdera, Issuer, Audience
}
```

To dobry wzorzec: **fail fast** — lepiej, żeby aplikacja nie wstała z komunikatem „brak klucza", niż wstała i
sypała 500 przy pierwszym logowaniu.

### Sekrety — czego nigdy nie robić

- **Nigdy** nie commituj haseł, kluczy JWT, produkcyjnych connection stringów.
- **Dev:** User Secrets (`dotnet user-secrets set "Jwt:Key" "..."`) albo `.env` (jak w Course Platform).
- **Prod:** Azure Key Vault / AWS Secrets Manager / zmienne w CI/CD.
- Pilnuj `.gitignore` — `.env` i `appsettings.Development.json` z sekretami **poza** repo.

---

## Środowiska (Development / Production / Testing)

`app.Environment` mówi, w jakim środowisku działasz (z `ASPNETCORE_ENVIRONMENT`). Course Platform robi na tym
decyzje:

```csharp
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }  // Swagger tylko w dev
if (!app.Environment.IsEnvironment("Testing")) { await context.Database.MigrateAsync(); } // migracje poza testami
```

Dzięki temu ta sama aplikacja zachowuje się inaczej lokalnie, na produkcji i w testach — bez zmiany kodu, tylko
przez zmienną środowiskową.

---

## Pułapki

1. **Sekret w `appsettings.json` + commit.** Klasyka wycieków. Sekrety → env/secrets, nie repo.
2. **Mylenie fazy rejestracji z fazą pipeline.** `builder.Services.Add...` (przed `Build`) vs `app.Use...` (po).
   Wstawienie `app.Use...` przed `Build()` się nie skompiluje; odwrotnie — nie zadziała.
3. **Zły host bazy w Dockerze.** Z kontenera `api` to `db`, nie `localhost` (patrz [00](./00-jak-korzystac-z-kursu-i-mapa.md)).
4. **Zapominanie o priorytetach konfiguracji.** „Ustawiłem w appsettings, a bierze inną wartość" — bo env
   nadpisuje appsettings.
5. **Brak walidacji konfiguracji.** Aplikacja wstaje, a pada dopiero przy pierwszym użyciu klucza. Waliduj na starcie.

## Ćwiczenia

1. 🟢 **Czytanie `Program.cs`.** Otwórz [src/CoursePlatform.API/Program.cs](../src/CoursePlatform.API/Program.cs).
   Zaznacz, gdzie kończy się faza rejestracji, a gdzie zaczyna faza pipeline (`var app = builder.Build();`).
2. 🟢 **Konfiguracja.** Znajdź, skąd bierze się `Jwt:ExpiryMinutes` i jaka jest wartość domyślna, gdy nie ma jej
   w konfiguracji. (Podpowiedź: `JwtTokenGenerator`.)
3. 🟡 **Nowa opcja.** Zaprojektuj (na papierze lub w kodzie) dodanie ustawienia `Courses:MaxPageSize` czytanego
   przez Options pattern. Gdzie zarejestrujesz, jaką klasę stworzysz, jak wstrzykniesz?
4. 🔴 **Refaktor.** Przerób odczyt JWT w `JwtTokenGenerator` z `IConfiguration` na `IOptions<JwtSettings>`.
   *Done, gdy* projekt się buduje (`docker compose exec api dotnet build`) i logowanie nadal działa.

## Pytania kontrolne

1. Czym różni się faza rejestracji usług od fazy konfiguracji pipeline w `Program.cs`?
2. Co robi Kestrel?
3. Jak zapiszesz `Jwt:Key` jako zmienną środowiskową i dlaczego z `__`?
4. Które źródło konfiguracji wygrywa: `appsettings.json` czy zmienna środowiskowa?
5. Po co Options pattern, skoro można czytać `Configuration["..."]`?
6. Dlaczego Swagger jest włączany tylko w Development?

<details>
<summary>Rozwiązania</summary>

1. Rejestracja (`builder.Services.Add...`, przed `Build()`) mówi, **jakie usługi istnieją**. Pipeline
   (`app.Use...`, po `Build()`) mówi, **jak obsłużyć żądanie** — i kolejność tu się liczy.
2. Kestrel to wbudowany serwer HTTP .NET — nasłuchuje na porcie i przekazuje żądania do pipeline aplikacji.
3. `Jwt__Key=...`. Podwójne podkreślenie, bo `:` (separator poziomów w .NET) nie jest dozwolony w nazwach
   zmiennych środowiskowych w wielu shellach; .NET tłumaczy `__` na `:`.
4. Zmienna środowiskowa — źródła później dodane nadpisują wcześniejsze (env > appsettings.{Env} > appsettings).
5. Bo daje typowanie, jedno miejsce prawdy, walidację przy starcie i testowalność; unika literówek w stringach
   rozsianych po kodzie.
6. Bo dokumentacja/UI API nie powinny być publicznie dostępne na produkcji (powierzchnia ataku, wyciek struktury).

</details>

## Idź dalej

➡️ **[04 — Middleware, filtry, atrybuty](./04-middleware-filtry-atrybuty.md)** — wchodzimy w serce pipeline:
jak żądanie przechodzi przez łańcuch warstw i dlaczego kolejność decyduje o bezpieczeństwie.

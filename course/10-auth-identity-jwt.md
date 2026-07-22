# 10 — Auth: Identity, JWT, resource authorization

> **Poziom:** 🟡 rdzeń juniora · **Czas:** ~120 min · **Wymaga:** [02](./02-http-rest-i-jak-dziala-web-api.md), [04](./04-middleware-filtry-atrybuty.md), [09](./09-architektura-clean-cqrs-mediatr.md)

## Po co ci to

Prawie każdy backend ma logowanie i uprawnienia. Musisz umieć wyjaśnić **cały przepływ**: od rejestracji, przez
logowanie i token, po sprawdzanie „czy wolno". To temat, który pojawia się na każdej rozmowie i który łatwo
zrobić **niebezpiecznie**. Course Platform ma solidny, realny system auth — prześledzimy go od A do Z.

## Mostek z tego, co już znasz

- W Laravelu miałeś Sanctum/Passport + guardy/polityki. ASP.NET Core ma **ASP.NET Identity** (zarządzanie
  userami/rolami/hasłami) + **JWT** (token) + **autoryzację** (`[Authorize]`, polityki).
- We froncie (Vue) dokładałeś `Authorization: Bearer <token>` w axios — teraz zobaczysz drugą stronę: jak backend
  ten token wystawia i weryfikuje.

---

## Dwa pojęcia, których nie wolno mylić

| Pojęcie | Pytanie | HTTP przy braku |
|---------|---------|-----------------|
| **Authentication** (uwierzytelnianie) | „Kim jesteś?" | 401 |
| **Authorization** (autoryzacja) | „Czy wolno ci to zrobić?" | 403 |

Najpierw ustalamy tożsamość (authentication), potem sprawdzamy uprawnienia (authorization). Dlatego w pipeline
`UseAuthentication()` jest przed `UseAuthorization()` ([04](./04-middleware-filtry-atrybuty.md)).

---

## ASP.NET Identity — zarządzanie użytkownikami

**Identity** to gotowy system: tabele użytkowników i ról, hashowanie haseł, blokady po nieudanych próbach,
walidacja siły hasła. Nie piszesz tego sam. Course Platform konfiguruje go w `Program.cs`:

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;                 // blokada po 5 próbach
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

- `ApplicationUser` ([Domain/Entities/ApplicationUser.cs](../src/CoursePlatform.Domain/Entities/ApplicationUser.cs))
  to encja użytkownika (dziedziczy po `IdentityUser<Guid>`, dokłada `FirstName`, `LastName`).
- `UserManager<ApplicationUser>` i `SignInManager<ApplicationUser>` to serwisy Identity, których używasz w
  handlerach logowania/rejestracji.
- Role: `Student`, `Instructor`, `Admin` (seedowane przy starcie, patrz niżej).

---

## JWT — token, którym się przedstawiasz

**JWT (JSON Web Token)** to podpisany token, który klient nosi w nagłówku `Authorization: Bearer <token>`. Zawiera
**claims** (informacje o userze) i **podpis**. Serwer nie pamięta sesji — tylko **weryfikuje podpis** (stateless,
[02](./02-http-rest-i-jak-dziala-web-api.md)).

Struktura tokenu (3 części oddzielone kropką): `header.payload.signature`. Payload to claims:

```json
{ "sub": "3fa85f64-...", "email": "jan@x.pl", "role": "Student", "exp": 1712345678 }
```

- `sub` — id użytkownika (Course Platform czyta go w `CurrentUserService`).
- `role` — rola (użyta przez `[Authorize(Roles=...)]`).
- `exp` — czas wygaśnięcia.
- **Podpis** — HMAC-SHA256 kluczem `Jwt:Key`. Zmiana choćby jednego znaku payloadu unieważnia podpis.

> **Ważne:** JWT jest **podpisany**, nie **zaszyfrowany**. Payload da się odczytać (base64) — nie wkładaj do
> niego sekretów. Podpis gwarantuje tylko, że nikt go nie **zmienił**.

### Generowanie tokenu

Course Platform ma [JwtTokenGenerator](../src/CoursePlatform.Infrastructure/Identity/JwtTokenGenerator.cs):

```csharp
public string GenerateToken(ApplicationUser user, IEnumerable<string> roles)
{
    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  // unikalny id tokenu
    };
    foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(issuer, audience, claims,
        expires: DateTime.UtcNow.AddMinutes(expiryMinutes), signingCredentials: creds);
    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### Weryfikacja tokenu (w `Program.cs`)

```csharp
builder.Services.AddAuthentication(/* JwtBearer */).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;   // nie przemapowuj nazw claimów
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true,
        ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = ..., ValidAudience = ...,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        RoleClaimType = ClaimTypes.Role, NameClaimType = ClaimTypes.Name,
        ClockSkew = TimeSpan.Zero        // brak "tolerancji" czasu (token wygasa punktualnie)
    };
});
```

To middleware `UseAuthentication()` przy każdym żądaniu odczytuje token, weryfikuje podpis i ważność, i wypełnia
`HttpContext.User`.

---

## Pełny przepływ logowania (end-to-end)

```
1. POST /api/auth/login { email, password }
2. LoginCommandHandler:
     UserManager.FindByEmailAsync → SignInManager.CheckPasswordSignInAsync (sprawdza hash, lockout)
     UserManager.GetRolesAsync → JwtTokenGenerator.GenerateToken(user, roles)
3. odpowiedź: { id, email, firstName, lastName, token, refreshToken, roles }
4. Vue zapisuje token → dokłada go w axios: Authorization: Bearer <token>
5. Każde kolejne żądanie: UseAuthentication weryfikuje token → HttpContext.User
6. [Authorize] / handler sprawdza uprawnienia → 200 / 401 / 403
```

Rejestracja ([RegisterCommand](../src/CoursePlatform.Application/Features/Auth/Commands/Register/RegisterCommand.cs))
tworzy usera, przypisuje rolę `Student` (i sprawdza wynik!), po czym od razu wystawia token:

```csharp
var result = await _userManager.CreateAsync(user, request.Password);
if (!result.Succeeded) throw new ValidationException(/* błędy Identity */);

var roleResult = await _userManager.AddToRoleAsync(user, "Student");
if (!roleResult.Succeeded) throw new ValidationException(/* nie udało się nadać roli */);

var roles = await _userManager.GetRolesAsync(user);
var token = _jwtTokenGenerator.GenerateToken(user, roles);
```

> **Detal, który robi różnicę:** sprawdzenie wyniku `AddToRoleAsync`. Jeśli rola nie istnieje (bo seed się nie
> wykonał), user zostałby bez roli i token bez roli — RBAC by się rozsypał po cichu. Dlatego role są seedowane
> **bezwarunkowo** przy starcie (`ApplicationDbContextSeed.SeedRolesAsync` w `Program.cs`), a wynik jest sprawdzany.

---

## Autoryzacja — trzy poziomy

### 1. Czy zalogowany (`[Authorize]`)

```csharp
[HttpGet("me")]
[Authorize]                       // brak/zły token → 401
public async Task<ActionResult<CurrentUserDto?>> GetCurrentUser(CancellationToken ct) { /* ... */ }
```

### 2. Czy ma rolę lub politykę (`[Authorize(Roles=...)]`, `[Authorize(Policy=...)]`)

```csharp
[HttpPost]
[Authorize(Policy = AuthorizationPolicies.InstructorOrAdmin)]   // Instructor lub Admin
public async Task<ActionResult<Guid>> CreateCourse(CreateCourseCommand command, CancellationToken ct) { /* ... */ }

[HttpPut("{id}")]
[Authorize(Policy = AuthorizationPolicies.ManageCourse)]        // właściciel kursu lub Admin (resource-based)
public async Task<ActionResult> UpdateCourse(Guid id, UpdateCourseCommand command, CancellationToken ct) { /* ... */ }
```

Polityki `InstructorOrAdmin` i `ManageCourse` ([AuthorizationPolicies](../src/CoursePlatform.Application/Common/Authorization/AuthorizationPolicies.cs))
to warstwa „czy w ogóle wolno" — ale **dostęp do konkretnej treści** (np. lekcji) nadal weryfują handlery
(resource-based, niżej).

### 3. Resource-based — czy ma dostęp do KONKRETNEGO zasobu (najważniejsze)

Rola to za mało: instruktor nie może oglądać/edytować **cudzego** kursu, a student widzi lekcję tylko po
**zapisaniu się**. To sprawdza handler, nie atrybut. Course Platform używa
[CourseAccessHelper](../src/CoursePlatform.Application/Common/Helpers/CourseAccessHelper.cs) — np. w
`GetLessonQuery`:

```csharp
if (_currentUser.UserId == null)
    throw new ForbiddenAccessException("User not authenticated.");

var hasAccess = await CourseAccessHelper.CanAccessCourseContentAsync(
    _context, _currentUser, request.CourseId, ct);   // admin || właściciel || zapisany

if (!hasAccess)
    throw new ForbiddenAccessException("You are not enrolled in this course.");
```

`ForbiddenAccessException` → `ExceptionHandlingMiddleware` → **403**. To wzorzec, który odróżnia „umiem
`[Authorize]`" od „rozumiem autoryzację". **Nigdy nie ufaj frontowi** — front tylko chowa przyciski; prawdziwa
bramka jest tu, na backendzie.

---

## Dodatkowe zabezpieczenia w Course Platform

Auth to nie tylko token. Course Platform w `Program.cs` dokłada:

- **CORS** — tylko `http://localhost:5173` (front) może wołać API z przeglądarki.
- **Rate limiting** — osobne polityki: `auth` (ostrzejsza, przeciw brute-force logowania) i `api` (ogólna).
  Po przekroczeniu → **429**.
- **Security headers** (`SecurityHeadersMiddleware`) — CSP, `X-Content-Type-Options` itd.
- **Walidacja konfiguracji JWT przy starcie** — klucz min. 32 bajty, nie placeholder ([03](./03-pierwsza-aplikacja-host-program-cs.md)).
- **Lockout** — blokada konta po 5 nieudanych logowaniach.
- **Refresh tokens** — login/rejestracja zwracają `refreshToken`; endpoint `POST /api/auth/refresh` wymienia go na
  nową parę access+refresh (rotacja, unieważnianie przy logout/reset hasła). Front trzyma oba tokeny w
  `localStorage` — to OK lokalnie; cookie `HttpOnly` to krok produkcyjny (🔴, niżej).
- **Forgot/reset password** — `POST /api/auth/forgot-password` wysyła link resetujący (MailHog w dev); reset
  unieważnia wszystkie refresh tokeny usera.
- **Confirm email** — po rejestracji idzie mail z linkiem; endpoint `POST /api/auth/confirm-email`.
  Identity ma `SignIn.RequireConfirmedAccount = true`, więc login przed potwierdzeniem kończy się błędem
  (komunikat o konieczności potwierdzenia). Rejestracja **nie** wydaje JWT — tokeny dopiero po confirm + login.
- **HTML sanitization** — treści od userów (opisy, recenzje) są czyszczone z niebezpiecznego HTML (`IHtmlSanitizer`),
  żeby uniknąć XSS.

---

## 🔴 Refresh tokens — co jest, a co jeszcze 🔴

Course Platform **ma już** refresh tokeny: login/rejestracja zwracają parę `token` + `refreshToken`, a
`POST /api/auth/refresh` ([RefreshTokenCommand](../src/CoursePlatform.Application/Features/Auth/Commands/RefreshToken/RefreshTokenCommand.cs))
wymienia ważny refresh na nową parę (rotacja — stary token jest unieważniany). Access JWT żyje np. 120 min;
refresh dłużej (domyślnie 7 dni). Front trzyma oba w `localStorage` i woła refresh, gdy access wygaśnie.

To, co nadal jest tematem produkcyjnym (🔴):

- **Token w cookie `HttpOnly; Secure; SameSite`** zamiast `localStorage` — bo `localStorage` jest dostępny dla
  JS, więc XSS może wykraść token. `HttpOnly` cookie nie jest widoczne dla JS.

Wiedz, co już działa w projekcie, a co to świadomy kompromis dev — to pada na rozmowach o bezpieczeństwie.

---

## Pułapki

1. **Mylenie 401 i 403.** 401 = brak/zły token; 403 = zalogowany, ale bez uprawnień. (Pytają o to.)
2. **Autoryzacja tylko po roli.** Bez resource-based instruktor edytuje cudze kursy, student widzi lekcje bez
   zapisu. Sprawdzaj dostęp do konkretnego zasobu.
3. **Sekret JWT w repo / za krótki.** Klucz min. 32 bajty, poza repo (env). Waliduj przy starcie.
4. **Sekrety w payloadzie JWT.** JWT jest podpisany, nie szyfrowany — payload da się odczytać.
5. **Ufanie frontowi.** Ukrycie przycisku to UX, nie bezpieczeństwo. Bramka jest na backendzie.
6. **Brak sprawdzenia wyniku operacji Identity** (`CreateAsync`/`AddToRoleAsync`) — cichy user bez roli.

## Ćwiczenia

> Uruchom projekt z seedem. Loginy testowe w `ApplicationDbContextSeed.cs` (np. `admin@courseplatform.com`/`Admin123!`).

1. 🟢 **Zdekoduj token.** Zaloguj się przez Swagger/`curl`, skopiuj token i wklej na `jwt.io` (lub zdekoduj
   payload base64). Wypisz claims. Które pola widzisz i co znaczą?
2. 🟢 **401 vs 403.** Zawołaj chroniony endpoint bez tokenu (401). Potem zaloguj się jako Student i zawołaj
   endpoint dla Instructora/Admina (`POST /api/courses`) — jaki kod i dlaczego?
3. 🟡 **Prześledź logowanie.** W `LoginCommand`/`RegisterCommand` opisz krok po kroku, co się dzieje od żądania
   do zwrócenia tokenu.
4. 🟡 **Resource-based w akcji.** Jako Student **niezapisany** na kurs zawołaj `GET /api/courses/{id}/lessons/{id}`.
   Jaki kod? Który fragment kodu go zwrócił? Potem zapisz się (`POST /enroll`) i spróbuj ponownie.
5. 🔴 **Prześledź refresh.** Otwórz `RefreshTokenCommandHandler` i opisz flow: skąd bierze się `refreshToken` przy
   loginie, co robi `/auth/refresh`, kiedy token jest unieważniany (logout, reset hasła). Porównaj z ryzykiem
   `localStorage` vs cookie `HttpOnly`.

## Pytania kontrolne

1. Czym różni się authentication od authorization? Który middleware jest pierwszy i dlaczego?
2. Co zawiera JWT i co gwarantuje jego podpis? Czy można mu powierzyć sekret?
3. Opisz przepływ od `POST /login` do udanego żądania chronionego endpointu.
4. Czym jest resource-based authorization i dlaczego rola nie wystarcza? Podaj przykład z Course Platform.
5. Po co seedować role bezwarunkowo i sprawdzać wynik `AddToRoleAsync`?
6. Dlaczego `localStorage` na token bywa ryzykowny i jaka jest alternatywa?

<details>
<summary>Rozwiązania</summary>

1. Authentication ustala tożsamość („kim jesteś", 401 przy braku); authorization sprawdza uprawnienia („czy
   wolno", 403). `UseAuthentication` jest pierwszy — bez tożsamości nie ma czego autoryzować.
2. JWT zawiera claims (np. `sub`, `role`, `exp`) i podpis. Podpis gwarantuje **integralność** (że nikt nie
   zmienił tokenu), ale payload jest tylko zakodowany (base64) — **nie** wkładaj sekretów.
3. Login sprawdza hasło (SignInManager, z lockoutem), pobiera role, generuje podpisany JWT i zwraca go. Front
   dokłada token do nagłówka; przy kolejnym żądaniu `UseAuthentication` weryfikuje podpis/ważność i wypełnia
   `HttpContext.User`, po czym `[Authorize]`/handler decydują o dostępie.
4. To sprawdzenie dostępu do **konkretnego** zasobu (nie tylko „czy ma rolę"). Rola nie wie, że kurs należy do
   kogoś innego. `CanAccessCourseContentAsync` sprawdza admina/właściciela/enrollment i przy braku → 403.
5. Bo bez istniejących ról `AddToRoleAsync` cicho zawiedzie → user bez roli, token bez roli, RBAC się sypie.
   Seed ról przy starcie + sprawdzenie wyniku eliminuje ten cichy błąd.
6. `localStorage` jest dostępny dla JS, więc XSS może wykraść token. Alternatywa: token w cookie
   `HttpOnly; Secure; SameSite` (niewidoczne dla JS) + krótki access token z refresh tokenem.

</details>

## Idź dalej

➡️ **[11 — Logowanie: Serilog](./11-logowanie-serilog.md)** — jak widzieć, co dzieje się w aplikacji na
produkcji, i jak nie zalogować przypadkiem czyichś sekretów.

# 04 — Middleware, filtry, atrybuty

> **Poziom:** 🟢→🟡 · **Czas:** ~90 min · **Wymaga:** [03](./03-pierwsza-aplikacja-host-program-cs.md)

## Po co ci to

Każde żądanie w ASP.NET Core przechodzi przez **pipeline middleware** — łańcuch komponentów, zanim dotrze do
kontrolera, i w drodze powrotnej. Tu dzieje się uwierzytelnianie, CORS, logowanie, obsługa błędów, rate limiting.
Zrozumienie pipeline (i **kolejności**) to różnica między „działa u mnie" a „rozumiem, dlaczego działa" — i
jest to jeden z najczęstszych tematów na rozmowach .NET.

## Mostek z tego, co już znasz

To jest **dokładnie** koncept `middleware` z Express/Laravela. W Express: `app.use((req, res, next) => {...})`
i wołasz `next()`. W ASP.NET Core: klasa z `InvokeAsync(HttpContext context)` i wołasz `await _next(context)`.
Ta sama „cebula", ta sama zasada „kolejność się liczy".

---

## Co to jest middleware (jak dla laika)

Middleware to komponent, który **opakowuje** obsługę żądania. Każdy może:

1. zrobić coś **przed** przekazaniem dalej (np. sprawdzić token),
2. **przekazać** żądanie dalej (`await _next(context)`),
3. zrobić coś **po** powrocie (np. dodać nagłówek do odpowiedzi),
4. **przerwać** łańcuch (nie wołać `_next`) — np. od razu zwrócić 401.

Minimalny middleware:

```csharp
public class MyMiddleware
{
    private readonly RequestDelegate _next;
    public MyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // PRZED — żądanie idzie "w dół"
        await _next(context);
        // PO — odpowiedź wraca "w górę"
    }
}
```

### Pipeline = stos cebuli

```
Request  ─►  [SecurityHeaders] ─► [Serilog] ─► [RateLimit] ─► [CORS] ─► [Auth] ─► Kontroler
                   │                  │             │            │         │
Response ◄─  [SecurityHeaders] ◄─ [Serilog] ◄─ [RateLimit] ◄─ [CORS] ◄─ [Auth] ◄─ (wynik)
```

Żądanie wchodzi od lewej, schodzi do kontrolera, a odpowiedź wraca **tą samą drogą w odwrotnej kolejności** (dla
kodu „po `_next`"). Dlatego middleware wpisany wcześniej „obejmuje" wszystkie późniejsze.

---

## Kolejność w Course Platform — i DLACZEGO

Z [Program.cs](../src/CoursePlatform.API/Program.cs):

```csharp
app.UseMiddleware<SecurityHeadersMiddleware>();  // 1
app.UseSerilogRequestLogging();                  // 2
app.UseRateLimiter();                            // 3
app.UseCors("AllowFrontend");                    // 4
app.UseAuthentication();                         // 5
app.UseAuthorization();                          // 6
app.UseMiddleware<ExceptionHandlingMiddleware>();// 7
app.MapControllers();                            // 8
```

| # | Middleware | Po co | Co się psuje, gdy źle |
|---|------------|-------|------------------------|
| 1 | Security headers | `X-Content-Type-Options`, CSP itd. | Brak ochrony przed częścią ataków |
| 2 | Serilog logging | Log każdego żądania | Trudniejszy debug/monitoring |
| 3 | Rate limiter | Ochrona przed floodem | API podatne na przeciążenie |
| 4 | CORS | Zgoda na żądania z Vue (`localhost:5173`) | Przeglądarka blokuje `fetch`/`axios` |
| 5 | Authentication | Odczyt JWT → `HttpContext.User` | `User` pusty, nikt nie jest „zalogowany" |
| 6 | Authorization | Sprawdza `[Authorize]` i polityki | Każdy endpoint otwarty **albo** wszystko zablokowane |
| 7 | Exception handling | Łapie wyjątki → JSON 400/403/404/500 | Stack trace lądowałby w odpowiedzi (źle!) |
| 8 | MapControllers | Routing do akcji kontrolera | Brak endpointów |

**Złota zasada:** `UseAuthentication` **przed** `UseAuthorization`. Nie sprawdzisz uprawnień (autoryzacja),
zanim nie ustalisz tożsamości (uwierzytelnienie). To pytanie pada na rozmowach — zapamiętaj kolejność i „dlaczego".

> **Uwaga o `ExceptionHandlingMiddleware`:** w Course Platform stoi **po** auth (pozycja 7). Łapie więc wyjątki
> z kontrolerów i handlerów (tam, gdzie mieszka logika i walidacja). Inna szkoła stawia globalny handler
> **na samym początku**, żeby łapał absolutnie wszystko (też wyjątki z auth/CORS). Oba podejścia spotkasz —
> ważne, żebyś wiedział, **co** dany handler faktycznie obejmuje (to, co jest po nim w łańcuchu).

---

## Własny middleware — na przykładzie obsługi błędów

Course Platform ma [ExceptionHandlingMiddleware](../src/CoursePlatform.API/Middleware/ExceptionHandlingMiddleware.cs),
który zamienia wyjątki domenowe na czyste odpowiedzi JSON (zamiast stack trace):

```csharp
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    catch (NotFoundException ex)          // rzucony w handlerze, gdy zasób nie istnieje
    {
        await HandleExceptionAsync(context, HttpStatusCode.NotFound, ex.Message);       // 404
    }
    catch (ForbiddenAccessException ex)   // brak uprawnień do zasobu
    {
        await HandleExceptionAsync(context, HttpStatusCode.Forbidden, ex.Message);      // 403
    }
    catch (ValidationException ex)        // FluentValidation
    {
        var errors = ex.Errors.GroupBy(e => e.PropertyName)
                              .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToList());
        await HandleValidationExceptionAsync(context, errors);                          // 400 { errors: {...} }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unhandled exception");
        await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred."); // 500
    }
}
```

To jest **potężny wzorzec**: handler w warstwie Application rzuca `throw new NotFoundException(...)`, a middleware
tłumaczy to na kod HTTP. Dzięki temu handlery nie wiedzą nic o HTTP (Clean Architecture), a klient dostaje spójny
kształt błędu. Wrócimy do tego w [09](./09-architektura-clean-cqrs-mediatr.md) i [10](./10-auth-identity-jwt.md).

Rejestracja własnego middleware: `app.UseMiddleware<ExceptionHandlingMiddleware>();` (widziałeś w `Program.cs`).

---

## Wbudowane middleware (nie piszesz sam)

| Metoda | Co robi |
|--------|---------|
| `UseRouting()` | Dopasowuje URL do endpointu (często implicit) |
| `UseCors()` | Cross-Origin Resource Sharing (zgoda dla frontu) |
| `UseAuthentication()` | Ustawia `HttpContext.User` z tokenu |
| `UseAuthorization()` | Sprawdza polityki i `[Authorize]` |
| `UseStaticFiles()` | Serwuje pliki z `wwwroot` |
| `UseHttpsRedirection()` | Przekierowuje HTTP → HTTPS |
| `UseRateLimiter()` | Limity żądań |

---

## `HttpContext` — pudełko na wszystko o żądaniu

```csharp
context.Request.Method        // "GET", "POST"
context.Request.Path          // "/api/courses"
context.Request.Headers       // nagłówki
context.User                  // ClaimsPrincipal — kim jest zalogowany (po Authentication)
context.Response.StatusCode   // 200, 404...
context.RequestServices       // kontener DI dla TEGO żądania (scope)
```

W Course Platform `CurrentUserService` czyta z `HttpContext.User` claim `"sub"` (id użytkownika z tokenu JWT):

```csharp
// src/CoursePlatform.Infrastructure/Services/CurrentUserService.cs (fragment)
public Guid? UserId
{
    get
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
        return Guid.TryParse(userId, out var id) ? id : null;
    }
}
```

To pomost między „surowym" HTTP a logiką biznesową: handler pyta `ICurrentUserService.UserId`, nie grzebie w
`HttpContext` (Clean Architecture).

---

## Atrybuty — metadane na kodzie

Atrybuty to adnotacje w `[nawiasach]` (jak dekoratory w TS/Pythonie, adnotacje w Javie). W kontrolerach:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("api")]
public class AdminController : ControllerBase { }
```

| Atrybut | Efekt |
|---------|-------|
| `[ApiController]` | Automatyczny 400 przy złym modelu, wnioskowanie źródeł bindowania |
| `[Route]` | Prefiks URL (`[controller]` = nazwa klasy bez „Controller") |
| `[Authorize]` | Wymaga zalogowania; `Roles="Admin"` — wymaga roli |
| `[AllowAnonymous]` | Wyjątek od `[Authorize]` (endpoint publiczny) |
| `[HttpGet]/[HttpPost]/...` | Metoda HTTP akcji |
| `[FromQuery]/[FromBody]/[FromRoute]` | Skąd wziąć parametr |

Zobaczysz je w akcji w [05](./05-kontrolery-minimal-api-mvc-blazor.md) i [10](./10-auth-identity-jwt.md).

---

## Middleware vs Filtry — kiedy co (🔴 dla świadomości)

Middleware działa dla **całej** aplikacji, wcześnie w pipeline. **Filtry** działają bliżej kontrolera i wiedzą
więcej o akcji (który kontroler, jakie argumenty).

| Mechanizm | Gdzie działa | Typowe użycie |
|-----------|--------------|---------------|
| **Middleware** | Cały pipeline | Logowanie, CORS, auth, obsługa błędów, rate limiting |
| **Action Filter** | Wokół akcji kontrolera | Walidacja/log parametrów akcji |
| **Authorization Filter** | Przed akcją | Niestandardowa autoryzacja |
| **Exception Filter** | Przy wyjątku w akcji | Alternatywa dla middleware błędów |
| **Endpoint Filter** | Minimal APIs | Jak action filter, ale dla `app.MapGet` |

> **Junior:** 80% potrzeb ogarniesz middleware + `[Authorize]` + globalny middleware błędów (dokładnie jak Course
> Platform). Filtry poznaj, gdy zobaczysz je w kodzie firmy — nie ucz się ich „na zapas".

---

## Pułapki

1. **Zła kolejność auth.** `UseAuthorization` przed `UseAuthentication` = `User` pusty przy sprawdzaniu
   uprawnień → wszystko 401/403 albo dziury.
2. **CORS w złym miejscu / źle skonfigurowany.** Musi być przed endpointami; zły origin = przeglądarka blokuje
   front (a Postman/`curl` działają — myląca „to działa u mnie").
3. **Handler błędów za późno.** Wyjątki z middleware **przed** nim nie zostaną złapane. Wiedz, co obejmujesz.
4. **Zapomniany `await _next(context)`.** Bez tego łańcuch się urywa — żądanie „wisi" albo nigdy nie dociera do
   kontrolera.
5. **Grzebanie w `HttpContext` w logice biznesowej.** Od tego jest `ICurrentUserService` — nie ciągnij
   `HttpContext` do handlerów.

## Ćwiczenia

1. 🟢 **Prześledź pipeline.** W `Program.cs` wypisz kolejność middleware i przy każdym napisz jedno zdanie „po co".
2. 🟢 **Znajdź handler błędów.** Otwórz `ExceptionHandlingMiddleware` i dopasuj: który wyjątek → który kod HTTP.
3. 🟡 **Własny middleware.** Zaprojektuj middleware `RequestTimingMiddleware`, który mierzy czas obsługi żądania
   i dokłada nagłówek `X-Response-Time-ms` do odpowiedzi. Napisz szkielet klasy i linię rejestracji. Gdzie w
   kolejności go wstawisz i dlaczego? *Done, gdy* masz `InvokeAsync` ze `Stopwatch` wokół `await _next`.
4. 🔴 **Eksperyment z kolejnością.** Rozważ (na papierze): co się stanie, jeśli przeniesiesz
   `ExceptionHandlingMiddleware` na sam początek (przed `UseAuthentication`)? Co zacznie łapać, czego wcześniej nie łapał?

## Pytania kontrolne

1. Opisz „cebulę" pipeline: jak biegnie kod przed i po `_next`.
2. Dlaczego `UseAuthentication` musi być przed `UseAuthorization`?
3. Jak własny middleware może przerwać łańcuch i od razu zwrócić 401?
4. Czym różni się middleware od action filtera?
5. Jak `ExceptionHandlingMiddleware` zamienia `NotFoundException` na 404 i po co to robić w middleware, a nie w
   każdym kontrolerze?
6. Skąd `CurrentUserService` wie, jaki jest `UserId`?

<details>
<summary>Rozwiązania</summary>

1. Kod przed `await _next` wykonuje się „w dół" (od pierwszego do ostatniego middleware, potem kontroler); kod po
   `await _next` wykonuje się „w górę" w odwrotnej kolejności, gdy odpowiedź wraca.
2. Bo autoryzacja sprawdza uprawnienia zalogowanego użytkownika — a tożsamość (`HttpContext.User`) ustawia
   dopiero uwierzytelnianie. Bez niej autoryzacja nie ma na czym pracować.
3. Nie wołając `_next(context)` — zamiast tego ustawia `context.Response.StatusCode = 401` i (opcjonalnie)
   zapisuje treść. Łańcuch się kończy.
4. Middleware działa dla całego pipeline i nie zna szczegółów akcji; filtr działa wokół konkretnej akcji
   kontrolera i ma dostęp do jej kontekstu (argumenty, wynik).
5. `try/catch` wokół `await _next`; łapie `NotFoundException` i ustawia 404 + JSON. W middleware — bo to jedno
   miejsce dla całej aplikacji (DRY), a handlery/kontrolery zostają czyste i nie znają HTTP.
6. Czyta `HttpContext.User` (wypełniony przez `UseAuthentication` z tokenu JWT) i pobiera z niego claim `"sub"`
   (id użytkownika), parsując go na `Guid`.

</details>

## Idź dalej

➡️ **[05 — Kontrolery, Minimal API, MVC/Blazor](./05-kontrolery-minimal-api-mvc-blazor.md)** — piszemy endpointy,
które faktycznie obsługują żądania, i porównujemy style budowy API.

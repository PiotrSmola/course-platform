# 05 — Kontrolery, Minimal API, MVC/Blazor

> **Poziom:** 🟢→🟡 · **Czas:** ~75 min · **Wymaga:** [04](./04-middleware-filtry-atrybuty.md)

## Po co ci to

Kontroler to miejsce, gdzie żądanie HTTP zamienia się w wywołanie twojego kodu. To „drzwi wejściowe" API.
Poznasz, jak wyglądają kontrolery w Course Platform (główny nurt korporacyjny), czym są Minimal APIs
(nowocześniejsza, lżejsza forma) i co to MVC/Razor/Blazor — żebyś wiedział, gdzie jesteś w całym ekosystemie i
co wybrać na rozmowie.

## Mostek z tego, co już znasz

- Kontroler ASP.NET Core ≈ kontroler w Laravelu (`CourseController@index`) albo router w Express z metodami.
- Akcja kontrolera ≈ pojedynczy handler trasy (`router.get('/courses', handler)`).
- Model bindowania (parametry z URL/body) ≈ `req.params`, `req.query`, `req.body` — tylko automatyczny i typowany.

---

## Kontroler w Course Platform — anatomia

Course Platform używa **kontrolerów** (nie Minimal APIs). Oto realny [CoursesController](../src/CoursePlatform.API/Controllers/CoursesController.cs) (fragment):

```csharp
[ApiController]
[Route("api/[controller]")]         // → /api/courses
[EnableRateLimiting("api")]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;
    public CoursesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<CoursesVm>> GetCourses(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetCoursesQuery(searchTerm, /* ... */ pageNumber, pageSize), cancellationToken);
        return Ok(result);            // 200 + JSON
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<CourseDetailsDto>> GetCourseDetails(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new GetCourseDetailsQuery(id), ct));

    [HttpPost]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<ActionResult<Guid>> CreateCourse(CreateCourseCommand command, CancellationToken ct)
        => Ok(await _mediator.Send(command, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<ActionResult> UpdateCourse(Guid id, UpdateCourseCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();       // 400
        await _mediator.Send(command, ct);
        return NoContent();                               // 204
    }
}
```

Rozbierzmy najważniejsze rzeczy:

### Cienki kontroler (bardzo ważna zasada)

Zauważ, że kontroler **nie ma logiki biznesowej**. Każda akcja robi trzy rzeczy:
1. odbiera żądanie,
2. wysyła Command/Query przez `_mediator.Send(...)`,
3. zwraca wynik (`Ok`, `NoContent`, `BadRequest`).

To jest **celowe** i jedna z najważniejszych konwencji w tym projekcie. Cała robota (walidacja, dostęp do bazy,
reguły) dzieje się w handlerze w warstwie Application (rozdział [09](./09-architektura-clean-cqrs-mediatr.md)).
Kontroler to tylko „tłumacz" HTTP ↔ MediatR. Na rozmowie: *„kontrolery są cienkie, logika w handlerach"*.

### Zwracane typy

- `ActionResult<T>` — pozwala zwrócić albo dane (`Ok(result)` → 200 + `T`), albo inny status (`BadRequest()`,
  `NotFound()`, `NoContent()`).
- Helpery: `Ok()` (200), `NoContent()` (204), `BadRequest()` (400), `NotFound()` (404), `Unauthorized()` (401),
  `Forbid()` (403), `CreatedAtAction(...)` (201).

### Bindowanie parametrów

- `Guid id` z trasy (`{id:guid}`) — wnioskowane automatycznie dzięki `[ApiController]`.
- `[FromQuery] string? searchTerm` — z query stringu (`?searchTerm=vue`).
- `CreateCourseCommand command` (body) — deserializacja JSON → obiekt. Też wnioskowane.
- `CancellationToken` — ASP.NET wstrzykuje go sam; przekazujesz dalej do handlera.
- `{id:guid}` to **route constraint** — trasa dopasuje się tylko, gdy `id` jest poprawnym GUID-em.

### `[ApiController]` — co daje za darmo

- Automatyczny **400** przy niepoprawnym modelu (np. brakujące wymagane pole) — zanim wejdziesz w akcję.
- Wnioskowanie źródeł parametrów (`[FromBody]`/`[FromQuery]` często zbędne).
- Szczegóły błędów w formacie `ProblemDetails`.

### Autoryzacja na poziomie akcji

`[AllowAnonymous]` (publiczne), `[Authorize]` (zalogowany), `[Authorize(Roles="Instructor,Admin")]` (rola). To
tylko **pierwsza bramka** — prawdziwe sprawdzenie dostępu do konkretnego zasobu robi handler
(resource-based authorization, [10](./10-auth-identity-jwt.md)).

> **Trasy zagnieżdżone.** [LessonsController](../src/CoursePlatform.API/Controllers/LessonsController.cs) ma
> `[Route("api/courses/{courseId:guid}/lessons")]` — czyli `courseId` jest częścią adresu wszystkich jego akcji.

---

## Minimal APIs — lżejsza alternatywa

Zamiast klasy kontrolera, definiujesz endpoint jedną linią w `Program.cs`:

```csharp
app.MapGet("/api/courses/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new GetCourseDetailsQuery(id), ct);
    return Results.Ok(result);
}).RequireAuthorization();
```

| | Kontrolery | Minimal APIs |
|--|-----------|--------------|
| Styl | Klasy + atrybuty | Funkcje inline / grupy |
| Boilerplate | Więcej | Mniej |
| Duże API | Skaluje się (grupowanie, konwencje) | Wymaga dyscypliny, żeby nie zrobić bałaganu |
| Gdzie spotkasz | Duże projekty, korporacje | Mikroserwisy, małe usługi, nowe projekty |

Robią to samo pod spodem (ten sam routing, DI, filtry). Course Platform wybrał kontrolery — czytelna struktura
przy wielu endpointach. **Na rozmowie:** znaj oba, umiej powiedzieć, kiedy który.

---

## MVC, Razor Pages, Blazor — panorama (co to i kiedy)

Do tej pory mówiliśmy o **API** (zwraca JSON, front renderuje Vue). Ale ASP.NET Core umie też renderować HTML na
serwerze. Musisz wiedzieć, że te opcje istnieją:

| Technologia | Kto renderuje UI | Kiedy |
|-------------|------------------|-------|
| **Web API + SPA (Vue/React)** | Front w przeglądarce | Nowoczesne produkty, osobne zespoły front/back (**Course Platform**) |
| **MVC + Razor (`.cshtml`)** | Serwer generuje HTML | Proste CRUD, legacy, SEO bez SSR frameworka |
| **Razor Pages** | Jak MVC, ale „strona-centryczne" | Mniejsze aplikacje z formularzami |
| **Blazor Server** | C# w przeglądarce przez SignalR | Zespoły tylko-.NET, akceptują opóźnienie sieci |
| **Blazor WebAssembly** | C# w WASM w przeglądarce | Aplikacje .NET działające offline |

- **MVC** = Model-View-Controller: kontroler zwraca **View** (HTML z Razor), nie JSON. To **nie** to samo co Web
  API. Course Platform jest API — kontrolery zwracają dane, nie widoki.
- **Blazor** to sposób pisania **frontendu w C#** zamiast JS. Ciekawe, gdy zespół nie chce/nie umie JS. Ty znasz
  Vue, więc dla ciebie SPA + API to naturalny wybór — ale dobrze wiedzieć, że Blazor istnieje (jest „niebieski"
  na roadmapie).

> **Junior:** skup się na Web API (to robisz w Course Platform). MVC/Razor/Blazor — rozumiej różnicę i kiedy
> ktoś ich używa. Nie musisz umieć pisać Blazora na rozmowę na stanowisko backend/API.

---

## Pułapki

1. **Logika w kontrolerze.** Kontroler ma być cienki. Reguły, dostęp do bazy, walidacja → handler. Gruby
   kontroler = trudne testy i powielona logika.
2. **Brak `CancellationToken`.** Przyjmij go w akcji i przekaż do handlera — inaczej anulowane żądania dalej
   obciążają bazę.
3. **Ręczne 400 zamiast walidacji.** Nie sprawdzaj ręcznie „czy title niepusty" w kontrolerze — od tego jest
   FluentValidation w pipeline ([09](./09-architektura-clean-cqrs-mediatr.md)).
4. **Zwracanie encji zamiast DTO.** Nie zwracaj encji EF bezpośrednio (wyciek pól, cykle, over-fetch). Zwracaj
   DTO/VM (Course Platform zwraca np. `CourseDetailsDto`, `CoursesVm`).
5. **Mylenie MVC z Web API.** MVC zwraca HTML (View), Web API — JSON. To różne cele.

## Ćwiczenia

1. 🟢 **Mapa endpointów.** Otwórz `src/CoursePlatform.API/Controllers/` i wypisz z 3 kontrolerów: trasę, metodę
   HTTP, wymaganą autoryzację. *Done, gdy* masz tabelkę ~9 wierszy.
2. 🟢 **Cienki kontroler.** Wskaż w `CoursesController`, gdzie jest logika biznesowa. (Podpowiedź: nie ma jej —
   napisz, dokąd została przeniesiona.)
3. 🟡 **Nowa akcja.** Zaprojektuj akcję `GET /api/courses/{id}/summary` zwracającą skrócone dane kursu. Jaki
   Command/Query wyślesz, jaki typ zwrócisz, jaka autoryzacja? (Sam handler dopiszesz po [09](./09-architektura-clean-cqrs-mediatr.md).)
4. 🟡 **Minimal API.** Przepisz akcję `GetCourseDetails` z `CoursesController` na Minimal API (`app.MapGet`).
   Zwróć uwagę, skąd weźmiesz `IMediator`. *Done, gdy* masz działający szkic `MapGet` z `RequireAuthorization`/`AllowAnonymous`.

## Pytania kontrolne

1. Co znaczy „cienki kontroler" i dlaczego to dobre?
2. Do czego służy `ActionResult<T>` i jak zwrócisz 204?
3. Co daje atrybut `[ApiController]`?
4. Czym różni się Web API od MVC?
5. Kiedy wybierzesz Minimal API zamiast kontrolerów?
6. Dlaczego nie zwracać encji EF bezpośrednio z akcji?

<details>
<summary>Rozwiązania</summary>

1. Kontroler tylko odbiera żądanie, deleguje (przez MediatR) i zwraca wynik — bez logiki biznesowej. Dobre, bo
   logika jest testowalna w izolacji, nie powiela się, a kontroler pozostaje prosty.
2. `ActionResult<T>` pozwala zwrócić dane lub dowolny status. 204 zwrócisz przez `return NoContent();`.
3. Automatyczny 400 przy niepoprawnym modelu, wnioskowanie źródeł parametrów, `ProblemDetails` dla błędów.
4. Web API zwraca dane (JSON), UI renderuje klient (Vue). MVC zwraca View (HTML) renderowany na serwerze.
5. Przy małych usługach/mikroserwisach/nowych projektach, gdzie zależy na zwięzłości i mało endpointów; przy
   dużych API kontrolery lepiej się organizują.
6. Bo wycieka wewnętrzne pola/relacje, grozi cyklami serializacji i over-fetchingiem, i wiąże API ze schematem
   bazy. Zwracaj DTO/VM skrojone pod odbiorcę.

</details>

## Idź dalej

➡️ **[06 — EF Core: fundamenty](./06-ef-core-fundamenty.md)** — wchodzimy w rdzeń juniora: jak .NET rozmawia z
bazą danych przez Entity Framework Core.

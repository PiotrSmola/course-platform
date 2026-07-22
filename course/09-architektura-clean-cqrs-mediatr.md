# 09 — Architektura: Clean Architecture, CQRS, MediatR

> **Poziom:** 🟡 rdzeń juniora · **Czas:** ~120 min · **Wymaga:** [05](./05-kontrolery-minimal-api-mvc-blazor.md), [06](./06-ef-core-fundamenty.md), [08](./08-dependency-injection.md)

## Po co ci to

Do tej pory poznałeś klocki (kontrolery, EF, DI). Teraz układamy je w **architekturę**, która nie rozpadnie się
po pół roku rozwoju. Clean Architecture + CQRS + MediatR to wzorzec, który Course Platform stosuje i który
**dominuje w poważnych projektach .NET** — więc pojawi się na rozmowie i w pracy. To rozdział, który spina cały
backend w całość.

## Mostek z tego, co już znasz

- W Laravelu widziałeś warstwy (Controller → Service → Repository → Model) i pewnie „Fat controllers are bad".
  Clean Architecture to bardziej rygorystyczna wersja tej samej idei.
- CQRS (Command/Query Separation) = „inne obiekty do zapisu, inne do odczytu" — jak rozdzielenie mutacji od
  zapytań, które znasz z Vue Query (mutacje vs queries!).
- MediatR = „wyślij komunikat, ktoś go obsłuży" — jak event bus, tylko dla żądań w kodzie.

---

## Warstwy: Clean Architecture

Course Platform ma cztery projekty (warstwy). Zależności kierowane **do środka** — do Domain:

```
        ┌─────────────────────────────────────────┐
        │                  API                     │  kontrolery, Program.cs, middleware
        │   (transport HTTP, DI wiring)            │
        └───────────────┬─────────────────────────┘
                        │ zależy od
        ┌───────────────▼─────────────────────────┐
        │              Application                  │  use case'y: Commands/Queries + Handlery,
        │   (logika przypadków użycia, interfejsy)  │  walidatory, interfejsy (IApplicationDbContext)
        └───────────────┬─────────────────────────┘
                        │ zależy od
        ┌───────────────▼─────────────────────────┐
        │                Domain                     │  encje, enums — CZYSTE C#, zero zależności
        └──────────────────────────────────────────┘
                        ▲
                        │ implementuje interfejsy z Application, zależy od Domain
        ┌───────────────┴─────────────────────────┐
        │             Infrastructure               │  EF DbContext, JWT, storage, serwisy
        └──────────────────────────────────────────┘
```

Reguły (nienaruszalne w tym projekcie):

| Warstwa | Zna | NIE zna | Przykłady z projektu |
|---------|-----|---------|----------------------|
| **Domain** | tylko siebie | EF, ASP.NET, HTTP | `Course`, `Enrollment`, `CourseStatus` |
| **Application** | Domain | EF, HTTP (tylko **interfejsy**) | `CreateCourseCommand` + handler, `IApplicationDbContext` |
| **Infrastructure** | Application + Domain | — | `ApplicationDbContext`, `JwtTokenGenerator`, `CurrentUserService` |
| **API** | Application (+ DI całości) | logiki biznesowej | `CoursesController`, `Program.cs` |

**Kluczowa sztuczka — inwersja zależności:** Application definiuje **interfejs** `IApplicationDbContext` (mówi
„potrzebuję czegoś, co ma `DbSet<Course>` i `SaveChangesAsync`"), a Infrastructure go **implementuje**. Dzięki
temu Application nie zależy od EF — zależy od własnej abstrakcji. Możesz podmienić bazę/ORM, nie ruszając logiki.

```csharp
// Application (interfejs — CO potrzebuję)
public interface IApplicationDbContext
{
    DbSet<Course> Courses { get; }
    Task<int> SaveChangesAsync(CancellationToken ct);
}

// Infrastructure (implementacja — JAK to zrobione)
public class ApplicationDbContext : ..., IApplicationDbContext { /* EF Core */ }
```

**Dlaczego to ważne:** logikę testujesz bez bazy (podstawiasz atrapę `IApplicationDbContext` — [16](./16-testowanie.md)),
a wymiana PostgreSQL → SQL Server to zmiana tylko w Infrastructure.

> **Dlaczego encje są „czyste" (Domain bez EF).** Konfiguracja mapowania siedzi w Infrastructure (Fluent API,
> [06](./06-ef-core-fundamenty.md)), a nie jako atrybuty na encjach. Dzięki temu Domain nie importuje EF.

---

## CQRS — rozdziel zapis od odczytu

**CQRS (Command Query Responsibility Segregation):** operacje dzielisz na dwa rodzaje:

- **Command** — zmienia stan, zwraca minimum (np. nowe `Id` albo nic). `CreateCourseCommand`, `EnrollCommand`.
- **Query** — tylko czyta, nic nie zmienia. `GetCoursesQuery`, `GetCourseDetailsQuery`.

Każda operacja to osobny, mały obiekt (record) + jego handler. Zalety: czytelność (jedna operacja = jeden plik),
łatwe testy, różne modele do zapisu i odczytu (np. Query zwraca DTO skrojone pod ekran).

> Znasz to z frontu: w Vue Query masz `useQuery` (odczyt) i `useMutation` (zmiana). CQRS to ta sama filozofia po
> stronie backendu.

### Vertical Slice — organizacja po funkcji

Course Platform łączy Clean Architecture z **vertical slices**: kod grupuje się po **funkcji**, nie po typie.
Zamiast folderów `Commands/`, `Handlers/`, `Validators/` w oddzielnych miejscach, wszystko dla jednej operacji
leży razem:

```
Application/Features/Courses/Commands/CreateCourse/
    CreateCourseCommand.cs            (command + handler w jednym pliku)
    CreateCourseCommandValidator.cs   (walidacja)
```

Otwierasz jeden folder i masz całą operację przed oczami. To bardzo praktyczne w utrzymaniu.

---

## MediatR — spoiwo CQRS

**MediatR** to biblioteka, która łączy „kto wysyła żądanie" z „kto je obsługuje", bez bezpośredniej zależności.
Kontroler wysyła Command/Query, MediatR znajduje właściwy handler i go woła.

```csharp
// 1. Command to record implementujący IRequest<TResult>
public record CreateCourseCommand(string Title, decimal Price, /* ... */) : IRequest<Guid>;

// 2. Handler implementuje IRequestHandler<TCommand, TResult>
public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CreateCourseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    { _context = context; _currentUser = currentUser; }

    public async Task<Guid> Handle(CreateCourseCommand request, CancellationToken ct)
    {
        var course = new Course { Title = request.Title, Price = request.Price, /* ... */ };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync(ct);
        return course.Id;
    }
}

// 3. Kontroler tylko wysyła (cienki kontroler z rozdziału 05)
[HttpPost]
public async Task<ActionResult<Guid>> CreateCourse(CreateCourseCommand command, CancellationToken ct)
    => Ok(await _mediator.Send(command, ct));
```

MediatR sam znajduje handler (assembly scanning, [08](./08-dependency-injection.md)) i wstrzykuje mu zależności
przez DI. Kontroler nie wie, kto obsłuży komendę — luźne sprzężenie.

> **Licencja (ważne w 2026):** Course Platform używa **MediatR 14.1.0**. Od 2025 MediatR (i AutoMapper) są na
> **licencji komercyjnej** (LuckyPennySoftware) — darmowe dla mniejszych/otwartych projektów, ale w komercji
> sprawdź warunki. Alternatywy open source: własny prosty mediator, **Wolverine**. Wiedz o tym na rozmowie.

---

## Pipeline behaviors — logika przekrojowa

To najpiękniejsza część MediatR. **Behavior** owija **każdy** handler (jak middleware, ale dla komend/zapytań).
Dzięki temu walidację, logowanie czy trimming robisz **raz**, a nie w każdym handlerze.

Course Platform rejestruje dwa behaviory ([Application/DependencyInjection.cs](../src/CoursePlatform.Application/DependencyInjection.cs)):

```csharp
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TrimmingBehavior<,>));    // najpierw trim
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>)); // potem walidacja
```

Kolejność ma znaczenie: **trimming przed walidacją** (żeby `"  "` w polu nie „przeszedł" walidacji `NotEmpty`).

Przepływ każdej operacji:

```
Command → TrimmingBehavior (obetnij spacje) → ValidationBehaviour (sprawdź reguły) → Handler → wynik
```

`ValidationBehaviour` uruchamia walidatory FluentValidation i — jeśli są błędy — rzuca `ValidationException`
**zanim** handler w ogóle się wykona. Handler dostaje więc zawsze poprawne dane.

---

## FluentValidation — walidacja deklaratywna

Reguły wejścia definiujesz osobno od handlera:

```csharp
public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Level).IsInEnum();     // enum musi mieć poprawną wartość
    }
}
```

`ValidationBehaviour` sam znajdzie ten walidator i uruchomi. Błędy trafiają do `ExceptionHandlingMiddleware`
([04](./04-middleware-filtry-atrybuty.md)), który zwraca `400 { errors: { Title: ["..."] } }`.

> **`IsInEnum()` to nie ozdoba.** Enum w C# to pod spodem liczba; ktoś może wysłać `level: 999`, które zrzutuje
> się na nieistniejącą wartość. `IsInEnum()` to blokuje. Backend nie ufa frontowi — to reguła bezpieczeństwa.

---

## Mapowanie encja → DTO: ręcznie vs biblioteki

Nie zwracaj encji z API ([05](./05-kontrolery-minimal-api-mvc-blazor.md)) — mapuj na DTO. Course Platform mapuje
**ręcznie** w LINQ `Select` (widziałeś w [07](./07-ef-core-zapytania-i-wydajnosc.md)):

```csharp
.Select(c => new CourseListDto(c.Id, c.Title, c.Price, /* ... */))
```

To „Manual Mapping" — więcej kodu, ale pełna kontrola i widać dokładnie, co pobiera SQL. Alternatywy:
- **AutoMapper** — konfigurujesz `CreateMap<Course, CourseDto>()`, mapuje po nazwach. Mniej kodu, więcej „magii"
  (i licencja komercyjna od 2025, jak MediatR).
- **Mapperly** — source generator (kod mapujący powstaje przy kompilacji): szybki, typowany, darmowy.

Dla nauki i kontroli manual mapping jest świetny. Znaj alternatywy na rozmowę.

---

## Resource-based authorization — reguła w handlerze

Rola (`[Authorize(Roles="Instructor")]`) to za mało — instruktor nie powinien edytować **cudzego** kursu.
Prawdziwe sprawdzenie „czy TEN user ma dostęp do TEGO zasobu" robi handler. Course Platform ma helper
[CourseAccessHelper](../src/CoursePlatform.Application/Common/Helpers/CourseAccessHelper.cs):

```csharp
// dostęp do treści kursu: admin LUB właściciel LUB zapisany student
public static async Task<bool> CanAccessCourseContentAsync(
    IApplicationDbContext context, ICurrentUserService currentUser, Guid courseId, CancellationToken ct)
{
    if (currentUser.UserId == null) return false;
    if (currentUser.IsAdmin) return true;
    var userId = currentUser.UserId.Value;
    if (await context.Courses.AnyAsync(c => c.Id == courseId && c.InstructorId == userId, ct)) return true;
    return await context.Enrollments.AnyAsync(e => e.CourseId == courseId && e.UserId == userId, ct);
}
```

Handler lekcji woła to i rzuca `ForbiddenAccessException` (→ 403), jeśli brak dostępu. To wzorzec, który
**odróżnia juniora od kogoś, kto rozumie bezpieczeństwo**. Więcej w [10](./10-auth-identity-jwt.md).

---

## Inne wzorce — panorama (żebyś znał nazwy)

| Wzorzec | Idea | W Course Platform |
|---------|------|-------------------|
| **Repository** | Abstrakcja nad `DbSet` | Zbędny — `DbContext` już jest abstrakcją + UoW |
| **Unit of Work** | Grupuje zapisy w transakcję | To robi `DbContext.SaveChanges` |
| **Mediator** | Rozłącza nadawcę od handlera | MediatR ✅ |
| **Polly** | Retry / circuit breaker przy wołaniu zewn. API | ✅ outbound resilience (MinIO, Stripe, Elasticsearch) |

### 🔴 Outbox i Saga — tylko „istnieje, kiedy"

- **Outbox** — gdy musisz niezawodnie wysłać event (do Kafki/RabbitMQ) **i** zapisać dane w jednej transakcji:
  zapisujesz event do tabeli w tej samej transakcji, a osobny worker go publikuje. Rozwiązuje „zapisało się, ale
  event nie poszedł".
- **Saga** — rozproszona „transakcja" przez wiele serwisów (każdy krok ma krok kompensujący).

To tematy **mikroserwisowe/eventowe** — nie fundament juniora w monolicie. Wiedz, że istnieją i po co; wrócą, gdy
wejdziesz w architektury rozproszone ([15](./15-zadania-w-tle-i-brokery.md)).

---

## Pułapki

1. **Logika w kontrolerze albo w Infrastructure.** Logika przypadków użycia mieszka w Application (handlery).
2. **Domain zależny od EF/HTTP.** Import EF w encji łamie Clean Architecture. Konfiguracja → Infrastructure.
3. **Walidacja w handlerze zamiast w walidatorze.** Powiela się i miesza z logiką. Reguły wejścia → FluentValidation.
4. **Autoryzacja tylko po roli.** Rola nie wystarcza — sprawdzaj dostęp do konkretnego zasobu (resource-based).
5. **Gruby handler.** Handler robiący 5 rzeczy → podziel. Jedna komenda = jeden przypadek użycia.
6. **Traktowanie Outbox/Saga jako fundamentu.** To zaawansowane wzorce rozproszone; nie wciskaj ich do monolitu
   „bo brzmią pro".

## Ćwiczenia

1. 🟢 **Mapa warstw.** Dla `CreateCourse` wskaż, w której warstwie jest: command, handler, walidator, encja,
   `DbContext`, kontroler. *Done, gdy* każdy element ma przypisaną warstwę.
2. 🟢 **Prześledź request.** Opisz drogę `POST /api/courses`: kontroler → MediatR → behaviors → handler → baza →
   odpowiedź. Zaznacz, gdzie działa walidacja.
3. 🟡 **Nowy przypadek użycia.** Zaprojektuj `ArchiveCourseCommand` (ustawia status na Hidden): record, handler
   (z resource-based auth: tylko właściciel/admin), walidator. Napisz szkielet trzech plików.
4. 🟡 **Behavior.** Wyjaśnij, dlaczego `TrimmingBehavior` musi być zarejestrowany przed `ValidationBehaviour`.
   Co się psuje przy odwrotnej kolejności?
5. 🔴 **Inwersja zależności.** Wytłumacz na `IApplicationDbContext`, jak Application „nie zna" EF, a mimo to
   korzysta z bazy. Co trzeba by zmienić, żeby przełączyć projekt na inny ORM?

## Pytania kontrolne

1. Wymień 4 warstwy i regułę zależności między nimi.
2. Co robi inwersja zależności na przykładzie `IApplicationDbContext`?
3. Czym różni się Command od Query? Podaj po jednym przykładzie z projektu.
4. Po co MediatR, skoro kontroler mógłby wołać handler bezpośrednio?
5. Co to pipeline behavior i jaką logikę przekrojową obsługuje Course Platform?
6. Dlaczego autoryzacja po roli nie wystarcza i jak to rozwiązuje `CourseAccessHelper`?

<details>
<summary>Rozwiązania</summary>

1. Domain ← Application ← API; Infrastructure → Application/Domain. Zależności kierowane do środka (Domain);
   Domain nie zależy od niczego.
2. Application definiuje interfejs `IApplicationDbContext` (czego potrzebuje), a Infrastructure go implementuje
   (`ApplicationDbContext` na EF). Application zależy od własnej abstrakcji, nie od EF.
3. Command zmienia stan i zwraca minimum (`CreateCourseCommand` → `Guid`); Query tylko czyta
   (`GetCoursesQuery` → `CoursesVm`).
4. MediatR daje luźne sprzężenie (kontroler nie zna handlera), automatyczne odnajdywanie handlerów i — kluczowe —
   pipeline behaviors (walidacja/logi/trim w jednym miejscu dla wszystkich operacji).
5. To komponent owijający każdy handler (jak middleware dla komend). Course Platform ma `TrimmingBehavior`
   (obcina spacje) i `ValidationBehaviour` (FluentValidation) — w tej kolejności.
6. Rola mówi „czy w ogóle wolno robić X", ale nie „czy wolno na TYM zasobie". `CanAccessCourseContentAsync`
   sprawdza admina/właściciela/enrollment dla konkretnego kursu i przy braku dostępu → 403.

</details>

## Idź dalej

➡️ **[10 — Auth: Identity, JWT, resource authorization](./10-auth-identity-jwt.md)** — kompletny mechanizm
logowania i kontroli dostępu w Course Platform, od loginu do `[Authorize]`.

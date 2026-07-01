# 08 — Dependency Injection

> **Poziom:** 🟡 rdzeń juniora · **Czas:** ~90 min · **Wymaga:** [01](./01-dotnet-i-csharp-od-zera.md), [03](./03-pierwsza-aplikacja-host-program-cs.md)

## Po co ci to

**Dependency Injection (DI)** to fundament, na którym stoi całe ASP.NET Core. Kontrolery, handlery, `DbContext`,
serwisy — nic z tego nie tworzysz ręcznie przez `new`. Zamiast tego rejestrujesz „przepisy", a framework sam
buduje obiekty i podaje je tam, gdzie trzeba. Zrozumienie DI (a zwłaszcza **cykli życia**) to jedno z
najczęstszych i najważniejszych pytań na rozmowie .NET — i realna umiejętność, bo źle dobrany lifetime to bug.

## Mostek z tego, co już znasz

- W Laravelu masz kontener IoC i Service Providery (`app->bind(...)`, wstrzykiwanie przez konstruktor). ASP.NET
  Core ma **wbudowany** kontener DI i robi dokładnie to samo.
- W Vue masz `provide/inject` — ta sama idea „dostarcz zależność z góry zamiast tworzyć ją w środku".
- Zasada „zależ od abstrakcji, nie od konkretu" (interfejsy z [01](./01-dotnet-i-csharp-od-zera.md)) to serce DI.

---

## Problem, który DI rozwiązuje

Bez DI klasa sama tworzy swoje zależności:

```csharp
public class CreateCourseCommandHandler
{
    private readonly ApplicationDbContext _context = new ApplicationDbContext(...); // ŹLE
}
```

Problemy: nie podmienisz `_context` w teście (na atrapę), nie skonfigurujesz połączenia w jednym miejscu, nie
zapanujesz nad transakcją/lifetime. Z DI klasa **prosi** o zależność w konstruktorze, a kto ją dostarczy — nie
jej sprawa:

```csharp
public class CreateCourseCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateCourseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }
}
```

To jest **constructor injection** — standard w .NET. Kontener widzi konstruktor, wie, jak zbudować `IApplicationDbContext`
i `ICurrentUserService`, tworzy je i wstrzykuje. Ty nigdy nie wołasz `new` na tym handlerze.

---

## Jak działa kontener DI

Dwie fazy (znasz je z [03](./03-pierwsza-aplikacja-host-program-cs.md)):

1. **Rejestracja** (w `Program.cs` / metodach `Add...`): „gdy ktoś poprosi o `ICurrentUserService`, daj
   `CurrentUserService`".
2. **Rozwiązywanie (resolve)**: przy żądaniu framework buduje **łańcuch zależności** automatycznie — tworzy
   `CurrentUserService`, tworzy `DbContext`, wstrzykuje je do handlera, handler do... itd.

```csharp
// rejestracja (Infrastructure/DependencyInjection.cs)
services.AddScoped<ICurrentUserService, CurrentUserService>();
services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
services.AddSingleton<IDateTimeService, DateTimeService>();
```

To realny fragment [Infrastructure/DependencyInjection.cs](../src/CoursePlatform.Infrastructure/DependencyInjection.cs).
Zwróć uwagę: mapujemy **interfejs → implementacja**. Kod zależy od `ICurrentUserService`, a kontener podstawia
`CurrentUserService`. W teście podstawisz atrapę — bez zmiany handlera.

---

## Cykle życia (lifetimes) — MUSISZ to umieć

To jest sedno tematu i klasyczne pytanie rekrutacyjne. Rejestrując usługę, wybierasz, **jak długo żyje** jej
instancja:

```csharp
services.AddTransient<IEmailSender, EmailSender>();   // NOWA przy każdym wstrzyknięciu
services.AddScoped<IApplicationDbContext, ...>();     // JEDNA na żądanie HTTP
services.AddSingleton<IDateTimeService, ...>();       // JEDNA na całą aplikację
```

| Lifetime | Ile instancji | Analogia | Typowe użycie |
|----------|---------------|----------|---------------|
| **Transient** | Nowa za każdym razem | Kartka jednorazowa | Lekkie, bezstanowe serwisy |
| **Scoped** | Jedna na żądanie HTTP | Jedna teczka na spotkanie | `DbContext`, serwisy per-request |
| **Singleton** | Jedna na całą aplikację | Jedna książka w biurze | Cache, konfiguracja, bezstanowe helpery |

### Dlaczego `DbContext` jest Scoped

`DbContext` śledzi zmiany i reprezentuje jednostkę pracy (transakcję) dla **jednego** żądania. Chcesz, żeby
wszystkie operacje w ramach jednego żądania HTTP dzieliły ten sam kontekst (i zapisały się razem), ale żeby
kolejne żądanie dostało **świeży** kontekst. Dlatego Scoped. W Course Platform `DbContext` (przez
`IApplicationDbContext`) jest Scoped.

### Najgroźniejsza pułapka: „captive dependency"

**Nigdy nie wstrzykuj usługi o krótszym życiu do usługi o dłuższym.** Najczęstszy błąd: `DbContext` (Scoped) w
Singletonie.

```csharp
// BUG: Singleton przechwytuje Scoped DbContext na całe życie aplikacji
public class BadCache        // zarejestrowany jako Singleton
{
    public BadCache(ApplicationDbContext db) { }  // DbContext "uwięziony", współdzielony między żądaniami → chaos
}
```

Skutki: `DbContext` nie jest thread-safe i nie jest zwalniany — dostajesz błędy współbieżności, wycieki, „widmowe"
dane. Zasada:
- **Scoped nie może żyć w Singletonie.**
- **Transient wstrzyknięty do Singletona** staje się de facto Singletonem (tworzy się raz).

Jak w Singletonie użyć czegoś Scoped? Utwórz scope ręcznie (zobaczysz to w tle/`BackgroundService`, [15](./15-zadania-w-tle-i-brokery.md)):

```csharp
await using var scope = _serviceProvider.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
```

---

## Rejestracja w Course Platform — podział na warstwy

Żeby `Program.cs` nie miał 300 linii, rejestracje są zgrupowane w **metodach rozszerzających** per warstwa:

```csharp
// Program.cs
builder.Services.AddApplication();                                   // warstwa Application
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment); // warstwa Infrastructure
```

- `AddApplication()` ([Application/DependencyInjection.cs](../src/CoursePlatform.Application/DependencyInjection.cs))
  rejestruje MediatR, walidatory FluentValidation i pipeline behaviors.
- `AddInfrastructure(...)` rejestruje `DbContext`, `ICurrentUserService`, `IJwtTokenGenerator`, storage itd.

To czysty wzorzec: każda warstwa „wie", co sama rejestruje, a `Program.cs` tylko je spina. Wrócimy do warstw w
[09](./09-architektura-clean-cqrs-mediatr.md).

### Assembly scanning — rejestracja hurtem

Ręczne rejestrowanie setek handlerów byłoby męczące. MediatR skanuje assembly i rejestruje wszystkie handlery sam:

```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
```

(Biblioteka **Scrutor** robi to samo dla dowolnych twoich interfejsów — `services.Scan(...)`. Dobrze znać nazwę.)

---

## `Microsoft.Extensions.*` — co to jest

Cały ten mechanizm (DI, konfiguracja, logowanie, hosting) to rodzina paczek **`Microsoft.Extensions.*`** — to
fundament, na którym stoi ASP.NET Core:

| Paczka | Co daje |
|--------|---------|
| `Microsoft.Extensions.DependencyInjection` | Kontener DI (`AddScoped` itd.) |
| `Microsoft.Extensions.Configuration` | appsettings + env ([03](./03-pierwsza-aplikacja-host-program-cs.md)) |
| `Microsoft.Extensions.Logging` | Abstrakcja logów (Serilog podpina się pod nią, [11](./11-logowanie-serilog.md)) |
| `Microsoft.Extensions.Hosting` | `IHost`, `BackgroundService` ([15](./15-zadania-w-tle-i-brokery.md)) |
| `Microsoft.Extensions.Caching.Memory` | `IMemoryCache` ([12](./12-cache-memory-redis.md)) |
| `Microsoft.Extensions.Http` | `IHttpClientFactory` (niżej) |

Gdy piszesz `builder.Services.Add...` — używasz właśnie `Microsoft.Extensions.DependencyInjection`.

### `IHttpClientFactory` — must-know

Gdy backend woła zewnętrzne API (np. płatności), **nie twórz `new HttpClient()`** — grozi to wyczerpaniem
socketów i problemami z DNS. Zamiast tego zarejestruj klienta przez fabrykę:

```csharp
builder.Services.AddHttpClient<IPaymentClient, StripePaymentClient>();

public class StripePaymentClient(HttpClient http) : IPaymentClient
{
    public Task<HttpResponseMessage> ChargeAsync(/* ... */) => http.PostAsJsonAsync("/charge", /* ... */);
}
```

Fabryka zarządza pulą połączeń i ich cyklem życia. Course Platform (etap 1) nie woła zewnętrznych API, ale to
wiedza „na rozmowę i na płatności w etapie 3".

---

## 🔴 Keyed services (.NET 8+) — „awans"

Gdy masz **kilka** implementacji tego samego interfejsu i chcesz wybierać po kluczu:

```csharp
services.AddKeyedScoped<IPaymentProvider, StripeProvider>("stripe");
services.AddKeyedScoped<IPaymentProvider, MockProvider>("mock");

public class Handler([FromKeyedServices("stripe")] IPaymentProvider provider) { }
```

Przydatne np. przy przełączaniu providera płatności (mock w dev, Stripe na prod). Nie fundament juniora — poznaj,
gdy zobaczysz w kodzie.

---

## Pułapki

1. **`new` zamiast wstrzyknięcia.** Tworzenie `DbContext`/serwisów ręcznie psuje testy, transakcje i lifetime.
2. **Captive dependency.** Scoped (np. `DbContext`) wstrzyknięty do Singletona = poważny bug współbieżności.
3. **Zły lifetime dla stanu.** Singleton musi być **thread-safe** (obsługuje wiele żądań naraz). Trzymanie w nim
   stanu per-użytkownik to wyciek danych między userami.
4. **Rejestracja pod złym interfejsem.** Zarejestrujesz `X` jako `IA`, wstrzykujesz `IB` → runtime error „unable
   to resolve service".
5. **Za dużo w konstruktorze.** Klasa z 8 zależnościami to sygnał, że robi za dużo (narusza SRP) — kandydat do
   podziału.

## Ćwiczenia

1. 🟢 **Znajdź rejestracje.** W `Infrastructure/DependencyInjection.cs` wypisz wszystkie usługi i ich lifetime.
   Przy każdej powiedz, dlaczego akurat taki lifetime.
2. 🟢 **Prześledź łańcuch.** Weź `CreateCourseCommandHandler`: jakie zależności ma w konstruktorze i skąd kontener
   je bierze?
3. 🟡 **Zaprojektuj serwis.** Chcesz `ISlugGenerator` (bezstanowy, robi slug z tytułu). Jaki lifetime wybierzesz
   i dlaczego? Napisz interfejs, implementację i linię rejestracji.
4. 🟡 **Diagnoza buga.** Kolega zarejestrował cache jako Singleton i wstrzyknął do niego `DbContext`. Wyjaśnij, co
   pójdzie źle i jak to naprawić (dwa sposoby).
5. 🔴 **Scope ręczny.** Napisz szkielet klasy, która w kontekście Singletona/tła potrzebuje `DbContext` —
   użyj `CreateAsyncScope()`.

## Pytania kontrolne

1. Co to jest constructor injection i dlaczego lepszy niż `new` w środku klasy?
2. Wyjaśnij różnicę Transient / Scoped / Singleton z przykładem.
3. Dlaczego `DbContext` jest Scoped?
4. Co to „captive dependency" i jak jej uniknąć?
5. Po co rejestrować pod interfejsem, a nie konkretną klasą?
6. Dlaczego nie tworzyć `new HttpClient()` i czego użyć zamiast tego?

<details>
<summary>Rozwiązania</summary>

1. To wstrzykiwanie zależności przez parametry konstruktora. Lepsze, bo pozwala podmienić zależność (test/mock),
   centralizuje konfigurację, oddaje kontrolę nad lifetime kontenerowi i czyni zależności jawnymi.
2. Transient — nowa instancja przy każdym wstrzyknięciu (lekkie, bezstanowe). Scoped — jedna na żądanie HTTP
   (`DbContext`). Singleton — jedna na całą aplikację (cache, config; musi być thread-safe).
3. Bo reprezentuje jednostkę pracy/transakcję dla jednego żądania — wszystkie operacje w żądaniu dzielą kontekst
   i zapisują się razem, a kolejne żądanie dostaje świeży.
4. Wstrzyknięcie usługi krótkożyjącej (Scoped) do dłużej żyjącej (Singleton) — Scoped zostaje „uwięziony" na
   całe życie Singletona. Unikasz, nie robiąc tego; gdy musisz, tworzysz scope ręcznie (`CreateAsyncScope`).
5. Bo kod zależy wtedy od abstrakcji — można podmienić implementację (test, inny provider) bez zmiany
   konsumenta; to sedno testowalności i Clean Architecture.
6. Bo ręczne `HttpClient`-y wyczerpują sockety i mają problemy z DNS. Zamiast tego `IHttpClientFactory`
   (`AddHttpClient`), który zarządza pulą i cyklem życia.

</details>

## Idź dalej

➡️ **[09 — Architektura, Clean Architecture, CQRS, MediatR](./09-architektura-clean-cqrs-mediatr.md)** — jak
poukładać kod w warstwy i przypadki użycia, żeby projekt się skalował i dał testować.

# 11 — Logowanie: Serilog

> **Poziom:** 🟡 rdzeń juniora · **Czas:** ~45 min · **Wymaga:** [03](./03-pierwsza-aplikacja-host-program-cs.md), [08](./08-dependency-injection.md)

## Po co ci to

Na produkcji nie masz debuggera — masz **logi**. Gdy coś padnie o 3:00, logi są jedynym śladem, co się stało.
Dobre, **strukturalne** logowanie to różnica między „wiem, że User X nie mógł się zapisać na kurs Y o 14:32" a
„coś nie działa". To krótki, ale ważny rozdział — i realna umiejętność w pracy.

## Mostek z tego, co już znasz

- W Laravelu miałeś `Log::info(...)` + kanały (Monolog). ASP.NET Core ma abstrakcję `ILogger<T>` (z
  `Microsoft.Extensions.Logging`), a pod nią podpinasz konkretny „sink" — najczęściej **Serilog**.
- We froncie robiłeś `console.log("user", user)`. Structured logging to to samo, ale zapisane tak, że da się
  potem **przeszukać** po polach (`userId = X`), a nie tylko czytać oczami.

---

## Dlaczego nie `Console.WriteLine`

```csharp
Console.WriteLine("Error: " + ex);   // ŹLE na produkcji
```

Problemy: brak poziomu ważności, brak czasu/kontekstu, brak struktury (nie przefiltrujesz po `CourseId`), brak
miejsca docelowego (plik? system logów?). Do tego string-konkatenacja gubi typy.

## Structured logging — logi jako dane

Zamiast sklejać string, przekazujesz **szablon + wartości**:

```csharp
_logger.LogInformation("User {UserId} enrolled in course {CourseId}", userId, courseId);
```

To nie jest zwykłe formatowanie. Serilog zapisuje osobno:
- **komunikat** (szablon): `User {UserId} enrolled in course {CourseId}`,
- **właściwości**: `UserId = 3fa8...`, `CourseId = 7bc1...`.

Dzięki temu w systemie logów (Seq, Elasticsearch/Kibana) zrobisz zapytanie „pokaż wszystkie zdarzenia, gdzie
`CourseId = 7bc1...`" — jak filtr w bazie. To jest sedno „structured".

> **Nazwy w `{}` to nie interpolacja stringów C#.** Nie pisz `$"...{userId}..."` — stracisz strukturę. Zostaw
> `{UserId}` i przekaż `userId` jako argument.

---

## Poziomy logowania

Od najbardziej gadatliwego do najpoważniejszego:

```
Verbose < Debug < Information < Warning < Error < Fatal
```

| Poziom | Kiedy używać |
|--------|--------------|
| `Debug`/`Verbose` | Szczegóły do diagnozy (tylko dev) |
| `Information` | Normalne zdarzenia biznesowe („user się zapisał") |
| `Warning` | Coś podejrzanego, ale obsłużonego (np. walidacja odrzuciła input, brak dostępu) |
| `Error` | Błąd, który wymaga uwagi (nieobsłużony wyjątek) |
| `Fatal` | Aplikacja pada / nie może działać |

Na produkcji zwykle `Information` lub `Warning` (mniej szumu), w dev `Debug`. Ustawiasz to w konfiguracji, nie w
kodzie — więc zmieniasz poziom bez recompile.

Course Platform używa poziomów świadomie: w `ExceptionHandlingMiddleware` ([04](./04-middleware-filtry-atrybuty.md))
`NotFoundException`/`ForbiddenAccessException`/`ValidationException` → `LogWarning` (to normalne sytuacje 4xx), a
nieobsłużony wyjątek → `LogError` (to prawdziwy problem 5xx).

---

## Serilog w Course Platform

Konfiguracja w `Program.cs`:

```csharp
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
```

`ReadFrom.Configuration` oznacza, że konfiguracja Serilog (poziomy, „sinki") pochodzi z `appsettings`/env — nie
jest zaszyta w kodzie. Przykładowa sekcja (koncepcyjnie):

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [ { "Name": "Console" } ]
  }
}
```

**Sink** to miejsce docelowe logów: konsola, plik, Seq (ładny podgląd logów Serilog), Elasticsearch/Kibana,
Application Insights. Zmieniasz sink w konfiguracji, nie w kodzie.

### Request logging

Course Platform ma też `app.UseSerilogRequestLogging()` w pipeline — to loguje **każde żądanie HTTP** jednym,
zwięzłym wpisem (metoda, ścieżka, status, czas). Zamiast zaśmiecać domyślnymi logami frameworka, dostajesz
czytelną linię na request.

### Jak logować w kodzie (przez DI)

`ILogger<T>` wstrzykujesz przez konstruktor (DI z [08](./08-dependency-injection.md)) — `T` to nazwa klasy,
pojawi się w logu jako kontekst:

```csharp
public class EnrollCommandHandler
{
    private readonly ILogger<EnrollCommandHandler> _logger;
    public EnrollCommandHandler(ILogger<EnrollCommandHandler> logger) => _logger = logger;

    public async Task Handle(/* ... */)
    {
        _logger.LogInformation("User {UserId} enrolling in course {CourseId}", userId, courseId);
        // ...
    }
}
```

---

## Czego NIGDY nie logować

- **Hasła**, pełne **tokeny JWT**, klucze API, numery kart, dane wrażliwe (PII w nadmiarze).
- Logi trafiają do plików/systemów, które ktoś przegląda — traktuj je jak dane, które mogą wyciec.
- Zamiast tokenu loguj co najwyżej id użytkownika. Zamiast hasła — nic.

To nie teoria: wyciek sekretów przez logi to realna klasa incydentów bezpieczeństwa.

---

## NLog — alternatywa (dla świadomości)

**NLog** to inna popularna biblioteka logowania o podobnych możliwościach. W nowych projektach .NET częściej
zobaczysz **Serilog** (structured logging jest jego rdzeniem). Oba podpinają się pod abstrakcję `ILogger<T>`, więc
twój kod (`_logger.LogInformation(...)`) wygląda tak samo niezależnie od wybranego backendu — to zaleta abstrakcji
`Microsoft.Extensions.Logging`.

---

## Pułapki

1. **Interpolacja zamiast szablonu.** `_logger.LogInformation($"user {userId}")` gubi strukturę — użyj
   `"user {UserId}", userId`.
2. **Logowanie sekretów.** Hasła/tokeny/klucze w logach = incydent. Nigdy.
3. **Zły poziom.** Logowanie wszystkiego jako `Error` (szum, fałszywe alarmy) albo normalnych błędów 4xx jako
   `Error` zamiast `Warning`.
4. **Za dużo logów w gorącej ścieżce.** Log w pętli po 10 000 elementach zaleje system i spowolni aplikację.
5. **Brak kontekstu.** Log „coś poszło źle" bez id/parametrów jest bezużyteczny. Dołączaj właściwości.

## Ćwiczenia

1. 🟢 **Znajdź logowanie.** W `ExceptionHandlingMiddleware` wskaż, które wyjątki logują się jako `Warning`, a
   które jako `Error`. Dlaczego takie rozróżnienie?
2. 🟢 **Poziomy.** Ustaw (koncepcyjnie) `MinimumLevel` na `Warning` — które z dzisiejszych logów znikną, a które
   zostaną?
3. 🟡 **Dodaj log.** Do wybranego handlera (np. `EnrollCommandHandler`) dodaj `ILogger<T>` przez konstruktor i
   zaloguj udany zapis na `Information` ze strukturalnymi polami `UserId`, `CourseId`. *Done, gdy* projekt się
   buduje i log pojawia się w konsoli kontenera (`docker compose logs api`).
4. 🔴 **Sink.** Naszkicuj (na papierze) dodanie sinka do pliku obok konsoli i wyjaśnij, czemu to zmiana w
   konfiguracji, a nie w kodzie.

## Pytania kontrolne

1. Czym „structured logging" różni się od `Console.WriteLine` / string-konkatenacji?
2. Dlaczego `{UserId}` w szablonie, a nie `$"{userId}"`?
3. Wymień poziomy logowania i kiedy użyć `Warning` vs `Error`.
4. Co to jest „sink" i podaj dwa przykłady?
5. Czego nigdy nie wolno logować i dlaczego?
6. Jak wstrzykujesz logger do handlera?

<details>
<summary>Rozwiązania</summary>

1. Structured logging zapisuje komunikat + osobne właściwości (pola), po których można potem filtrować w systemie
   logów; `Console.WriteLine` daje płaski string bez poziomu, czasu, kontekstu i możliwości wyszukiwania.
2. Bo `{UserId}` zachowuje wartość jako osobną właściwość (przeszukiwalną); interpolacja `$"..."` sklei wszystko
   w jeden string i strukturę tracisz.
3. `Verbose < Debug < Information < Warning < Error < Fatal`. `Warning` — sytuacja podejrzana, ale obsłużona
   (np. 4xx, brak dostępu). `Error` — realny błąd wymagający uwagi (np. nieobsłużony wyjątek, 5xx).
4. Sink to miejsce docelowe logów. Przykłady: konsola, plik, Seq, Elasticsearch/Kibana, Application Insights.
5. Haseł, pełnych tokenów JWT, kluczy API, danych kart/PII — bo logi mogą wyciec i trafiają do wielu miejsc.
6. Przez konstruktor: `ILogger<NazwaKlasy>` wstrzykiwany przez DI; `T` daje kontekst (nazwę klasy) w logach.

</details>

## Idź dalej

➡️ **[12 — Cache: Memory i Redis](./12-cache-memory-redis.md)** — wchodzimy w rozszerzenia pod pracę: jak
przyspieszyć aplikację, trzymając często czytane dane w pamięci.

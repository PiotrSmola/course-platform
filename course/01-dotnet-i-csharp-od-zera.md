# 01 — .NET i C# od zera

> **Poziom:** 🟢 laik · **Czas:** ~90 min · **Wymaga:** [00](./00-jak-korzystac-z-kursu-i-mapa.md)

## Po co ci to

ASP.NET Core to framework napisany w **C#**, działający na platformie **.NET**. Zanim zrozumiesz framework,
musisz rozumieć język i platformę — inaczej każdy przykład będzie „magią". Ten rozdział daje ci C# i .NET
na tyle, żeby czytać kod Course Platform bez potykania się o składnię. Nie musisz stać się ekspertem C# —
musisz umieć **czytać** i **pisać proste rzeczy** oraz rozumieć, dlaczego wyglądają tak, a nie inaczej.

## Mostek z tego, co już znasz

Znasz JS/TS, może Javę, PHP/Laravel, Python. To ogromny fundament — C# to język **statycznie typowany,
obiektowy, kompilowany**, bardzo podobny do Javy i TypeScriptu.

| Znasz to | W C# to | Komentarz |
|----------|---------|-----------|
| Node.js runtime + npm | **.NET runtime + NuGet** | Środowisko uruchomieniowe + menedżer paczek |
| TypeScript (typy, interfejsy) | **C#** | C# to „TS na sterydach" — typy są obowiązkowe i realne w runtime |
| `class` w JS/PHP/Javie | `class` w C# | Bardzo podobnie |
| `async/await` w JS | `async/await` w C# | Prawie identyczne, ale zwraca `Task`, nie `Promise` |
| `Array.map/filter` w JS | **LINQ** (`.Select/.Where`) | To samo pojęciowo |
| `composer`/`npm install` | `dotnet add package` | Instalacja zależności |
| `interface` w TS | `interface` w C# | Kontrakt bez implementacji |

Największa różnica względem JS/Pythona: **wszystko ma typ i wszystko żyje w klasach**, a kod jest
**kompilowany** przed uruchomieniem (błędy typów łapiesz zanim odpalisz).

---

## Czym jest .NET (jak dla laika)

**.NET** to platforma: środowisko uruchomieniowe + ogromna biblioteka standardowa. Pomyśl o niej jak o JVM
(dla Javy) albo o Node.js (dla JS), tylko dla języków rodziny .NET (głównie C#).

Trzy słowa, które usłyszysz:

| Skrót | Rozwinięcie | Co robi (po ludzku) |
|-------|-------------|---------------------|
| **CLR** | Common Language Runtime | Silnik, który uruchamia twój kod, zarządza pamięcią (garbage collector), rzuca wyjątki. Odpowiednik JVM. |
| **BCL** | Base Class Library | Gotowe klasy: kolekcje, pliki, sieć, JSON, daty. Odpowiednik „standard library". |
| **IL** | Intermediate Language | Kod C# kompiluje się najpierw do IL (kod pośredni), a CLR tłumaczy IL na kod maszynowy w locie (JIT). Jak bytecode w Javie. |

**.NET 9** (którego używa Course Platform) to konkretna wersja platformy. Nazewnictwo historycznie było
zagmatwane (.NET Framework vs .NET Core vs .NET 5+), ale dziś jest prosto: **jest po prostu „.NET" + numer
wersji**, jeden, wieloplatformowy (Windows/Linux/macOS). „.NET Core" to stara nazwa tego samego nurtu.

> **Dlaczego to ważne dla ASP.NET:** ASP.NET Core to biblioteki na .NET, które dodają obsługę HTTP. Cała reszta
> (typy, kolekcje, async, DI) to czyste .NET/C#. Framework nie wymyśla języka od nowa.

---

## C# — rzeczy, które musisz umieć czytać

### Typy i zmienne

```csharp
int liczba = 42;                 // liczba całkowita
decimal cena = 99.99m;           // liczba dziesiętna do PIENIĘDZY (sufiks m)
string tytul = "Vue 3";          // tekst
bool czyOpublikowany = true;     // prawda/fałsz
Guid id = Guid.NewGuid();        // unikalny identyfikator (jak UUID)
DateTime teraz = DateTime.UtcNow; // data i czas (zawsze UTC w backendzie!)

var cos = "wykryty typ";         // var = "wywnioskuj typ" (jak w TS/Java), ale typ i tak jest statyczny
```

> **Pieniądze zawsze `decimal`, nigdy `double`/`float`.** `double` gubi grosze przy zaokrągleniach.
> W Course Platform cena kursu to `decimal` (zobacz `Course.Price`).

### Klasy — obiekty z tożsamością

Klasa to szablon obiektu. W Course Platform encja `Course` to zwykła klasa:

```csharp
// src/CoursePlatform.Domain/Entities/Course.cs (fragment)
public class Course : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public CourseStatus Status { get; set; } = CourseStatus.Draft;
    public Guid InstructorId { get; set; }
    public ICollection<Module> Modules { get; set; } = new List<Module>();
}
```

Rozbierzmy to:

- `public` — dostępne z zewnątrz (modyfikator dostępu; są też `private`, `protected`, `internal`).
- `class Course : BaseEntity` — `Course` **dziedziczy** po `BaseEntity` (dostaje jej pola, np. `Id`, `CreatedAt`).
- `public string Title { get; set; }` — to **property** (właściwość). W JS napisałbyś zwykłe pole; w C#
  property to pole z „getterem i setterem". `{ get; set; }` = można czytać i zapisywać.
- `= string.Empty` — wartość domyślna (pusty string zamiast `null`).
- `ICollection<Module>` — kolekcja obiektów `Module` (generyk, jak `Array<Module>` w TS).

### Properties (właściwości) — czym różnią się od pól

```csharp
public string Title { get; set; }          // auto-property: czytaj i zapisuj
public string Title { get; private set; }  // czytaj z zewnątrz, zapisuj tylko wewnątrz klasy
public Guid Id { get; protected set; }     // jak w BaseEntity — chroni tożsamość encji
public string Slug => Title.ToLower();      // property tylko do odczytu, wyliczana (expression-bodied)
```

To jest jak `get`/`set` w TS, tylko wbudowane w składnię języka.

### `record` vs `class` — kiedy co

```csharp
// class — obiekt z tożsamością, który ZMIENIA się w czasie (encja)
public class Course { public string Title { get; set; } }

// record — niezmienny "pakiet danych", idealny na DTO i komendy CQRS
public record CreateCourseCommand(string Title, decimal Price) : IRequest<Guid>;
```

`record` w nawiasie ma **parametry pozycyjne** — C# sam generuje z nich properties (tylko do odczytu, `init`),
konstruktor, porównywanie po wartości i `ToString()`. W Course Platform **wszystkie komendy/zapytania CQRS to
recordy** — bo to niezmienne „paczki danych" (zobacz `CreateCourseCommand`, `GetCoursesQuery`).

Reguła kciuka: **encja = `class`** (żyje, mutuje się), **DTO/Command/Query = `record`** (przenosi dane, nie mutuje).

### Null-safety (`string?`, `Guid?`)

C# ma **nullable reference types** — kompilator pilnuje `null` (jak `strictNullChecks` w TS):

```csharp
string tytul = "x";      // NIE może być null (kompilator ostrzeże)
string? opis = null;     // MOŻE być null (znak ?)
Guid? userId = null;     // nullable typ wartościowy

// bezpieczny dostęp
int? dlugosc = opis?.Length;         // null-conditional (jak ?. w JS)
string wynik = opis ?? "brak";       // null-coalescing (jak ?? w JS)
```

W Course Platform zobaczysz to np. w `ICurrentUserService.UserId` typu `Guid?` — bo użytkownik może być
niezalogowany.

### Kolekcje i LINQ

Kolekcje to `List<T>`, `Dictionary<K,V>`, `HashSet<T>`, `Queue<T>`. **LINQ** to zestaw metod do zapytań nad
kolekcjami — pojęciowo jak `.map/.filter/.reduce` w JS, ale bogatszy:

```csharp
var opublikowane = courses
    .Where(c => c.Status == CourseStatus.Published)   // filtr (jak .filter)
    .OrderBy(c => c.Title)                            // sortowanie
    .Select(c => c.Title)                             // mapowanie (jak .map)
    .Take(10)                                         // pierwsze 10
    .ToList();                                        // materializacja do listy
```

**Kluczowa rzecz na przyszłość:** ten sam LINQ EF Core **tłumaczy na SQL**, gdy piszesz go nad tabelą w bazie.
To znaczy `Where` staje się `WHERE` w SQL i wykonuje się w bazie, nie w pamięci. Wrócimy do tego w [06](./06-ef-core-fundamenty.md)/[07](./07-ef-core-zapytania-i-wydajnosc.md).

### `async` / `await` — obowiązkowe w ASP.NET

Serwer obsługuje wiele żądań naraz. Gdy czekasz na bazę lub sieć, **nie blokuj wątku** — oddaj go innym
żądaniom. Do tego służy `async/await` (jak w JS, ale zwraca `Task<T>`, nie `Promise<T>`):

```csharp
// ŹLE — blokuje wątek na czas zapytania do bazy
var courses = _context.Courses.ToList();

// DOBRZE — zwalnia wątek, gdy czeka na bazę
var courses = await _context.Courses.ToListAsync(cancellationToken);
```

- `Task` = „obietnica wyniku w przyszłości" (jak `Promise`). `Task<T>` zwraca wartość, `Task` samo nic.
- `await` = „poczekaj na wynik, ale nie blokuj wątku".
- Metoda z `await` musi być `async`.
- Metody I/O w .NET mają wersje `...Async` (`ToListAsync`, `SaveChangesAsync`, `FindAsync`) — używaj ich.

**`CancellationToken`** — gdy klient zamknie kartę/anuluje żądanie, .NET może przerwać operację. W Course
Platform ten token przewija się przez wszystkie handlery i zapytania do bazy — zawsze go przekazuj dalej.

### Interfejsy — kontrakt bez implementacji

```csharp
// src/CoursePlatform.Application/Common/Interfaces/ICurrentUserService.cs
public interface ICurrentUserService
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
```

Interfejs mówi „co", nie „jak" (dokładnie jak w TS/Javie). Ktoś inny dostarcza implementację
(`CurrentUserService` w warstwie Infrastructure). Dzięki temu kod zależy od **abstrakcji**, a nie od konkretu —
to fundament testowalności i Dependency Injection (rozdział [08](./08-dependency-injection.md)).

### Enum — zbiór nazwanych wartości

```csharp
// src/CoursePlatform.Domain/Enums/CourseStatus.cs
public enum CourseStatus
{
    Draft = 0,
    Published = 1,
    Hidden = 2
}
```

Zamiast „magicznych" liczb czy stringów masz nazwany, typowany zbiór wartości. `course.Status == CourseStatus.Published`
czyta się lepiej niż `course.Status == 1`.

---

## Struktura pliku C#

```csharp
using Microsoft.EntityFrameworkCore;   // importy (jak import w JS/TS)

namespace CoursePlatform.Domain.Entities;  // przestrzeń nazw (jak package w Javie)

public class Course : BaseEntity
{
    // ...
}
```

- `using X;` — importuje przestrzeń nazw (odpowiednik `import` / `use` w Laravel).
- `namespace X;` — logiczne pudełko na klasy; zwykle odpowiada strukturze katalogów.
- Jeden plik zwykle = jedna klasa publiczna (konwencja, nie przymus).

---

## 🔴 Rzeczy „na później" (nie blokują cię teraz)

Zobaczysz je w kodzie, ale nie musisz ich rozumieć w 100% na start:

- **Generyki** (`IRequestHandler<TRequest, TResponse>`) — typy sparametryzowane. Jak `<T>` w TS/Javie.
- **Extension methods** — dodawanie metod do istniejących typów (`builder.Services.AddControllers()` to
  metoda rozszerzająca). Wrócą w [03](./03-pierwsza-aplikacja-host-program-cs.md)/[08](./08-dependency-injection.md).
- **Pattern matching** (`switch` na typach, `is`), **tuples**, **`nameof`** — cukier składniowy, zrozumiesz w kontekście.

Nie ucz się ich „na sucho" — poznasz je naturalnie, gdy trafisz na nie w prawdziwym kodzie kolejnych rozdziałów.

---

## Pułapki (gdzie junior wpada)

1. **`double` na pieniądze.** Zawsze `decimal`. Zaokrąglenia w `double` gubią grosze.
2. **`DateTime.Now` zamiast `DateTime.UtcNow`.** W backendzie zawsze UTC — inaczej masz piekło ze strefami
   czasowymi. Course Platform używa `UtcNow` wszędzie.
3. **Blokujące wywołania (`.Result`, `.Wait()`, `.ToList()` na zapytaniu do bazy).** Zamiast tego `await ...Async`.
   Blokowanie wątku w ASP.NET obniża przepustowość i może zakleszczyć aplikację.
4. **Ignorowanie ostrzeżeń o `null`.** Kompilator ostrzega nie bez powodu — `NullReferenceException` to
   klasyczny błąd runtime, którego C# pomaga uniknąć.
5. **Mylenie `==` dla klas.** Dla `class` `==` domyślnie porównuje referencje (czy to ten sam obiekt), a dla
   `record` — wartości. To celowa różnica.

---

## Ćwiczenia

> Uruchom projekt (`docker compose up -d`) i eksploruj kod w `src/CoursePlatform.Domain/`.

1. 🟢 **Czytanie encji.** Otwórz `src/CoursePlatform.Domain/Entities/Lesson.cs`. Wypisz wszystkie properties
   i przy każdym powiedz: typ, czy nullable, czy to kolekcja. *Done, gdy* rozumiesz każdą linię.
2. 🟢 **record vs class.** Znajdź jeden `record` (np. w `Features/Courses/Queries/GetCourses/GetCoursesQuery.cs`)
   i jedną `class`-encję. Napisz w 2 zdaniach, dlaczego każde z nich jest tym, czym jest.
3. 🟡 **LINQ na papierze.** Mając listę `List<Course> courses`, napisz zapytanie LINQ zwracające tytuły kursów
   droższych niż 100 zł, posortowane malejąco po cenie. *Done, gdy* używasz `Where`, `OrderByDescending`, `Select`.
4. 🟡 **async.** Znajdź w dowolnym handlerze (`src/CoursePlatform.Application/Features/**`) trzy metody kończące
   się na `Async` i wyjaśnij, dlaczego są asynchroniczne (co robią wolnego).

## Pytania kontrolne

1. Czym różni się `.NET` od `C#`? A czym CLR od BCL?
2. Kiedy użyjesz `record`, a kiedy `class`? Podaj przykład z Course Platform.
3. Co oznacza `string?` i czym różni się od `string`?
4. Dlaczego w ASP.NET używamy `await ...Async` zamiast zwykłych wywołań? Co złego robi `.ToList()` na zapytaniu do bazy?
5. Czym jest property i czym różni się od zwykłego pola?
6. Po co interfejs `ICurrentUserService`, skoro i tak jest jedna implementacja?
7. Jakiego typu użyjesz na cenę i dlaczego nie `double`?

<details>
<summary>Rozwiązania</summary>

1. **.NET** to platforma (runtime + biblioteki); **C#** to język, który się na niej wykonuje. **CLR** to silnik
   uruchomieniowy (zarządza pamięcią, wykonuje IL); **BCL** to biblioteka gotowych klas (kolekcje, IO, JSON…).
2. `record` — niezmienna paczka danych (DTO, Command, Query); porównuje się po wartości. `class` — obiekt z
   tożsamością, który mutuje w czasie (encja). Przykład: `CreateCourseCommand` (record) vs `Course` (class).
3. `string?` może być `null`; `string` — nie (kompilator ostrzega przy próbie przypisania `null`). To ochrona
   przed `NullReferenceException`.
4. Bo serwer obsługuje wiele żądań równocześnie — `await ...Async` zwalnia wątek na czas czekania na I/O
   (baza/sieć), zamiast go blokować. `.ToList()` na zapytaniu do bazy wykonuje je **synchronicznie** i blokuje
   wątek; dodatkowo, jeśli zabraknie `Where`, ściąga całą tabelę do pamięci.
5. Property to „pole z getterem/setterem" wbudowane w składnię — pozwala kontrolować odczyt/zapis (np.
   `{ get; private set; }`), dodać logikę, wartość wyliczaną. Zwykłe pole tego nie ma.
6. Bo dzięki abstrakcji handler nie zależy od konkretnej implementacji — w testach podstawisz atrapę (mock),
   a w aplikacji prawdziwy `CurrentUserService`. To fundament DI i testowalności.
7. `decimal` — bo jest dokładny dziesiętnie. `double`/`float` to binarne liczby zmiennoprzecinkowe, które
   gubią grosze przy niektórych wartościach.

</details>

## Idź dalej

➡️ **[02 — HTTP, REST i jak działa Web API](./02-http-rest-i-jak-dziala-web-api.md)** — zanim napiszemy backend,
zrozummy, jak w ogóle rozmawiają ze sobą przeglądarka/Vue i serwer.

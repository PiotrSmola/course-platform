# 07 — EF Core: zapytania i wydajność

> **Poziom:** 🟡 rdzeń juniora · **Czas:** ~120 min · **Wymaga:** [06](./06-ef-core-fundamenty.md)

## Po co ci to

Umiesz już zmapować tabele na klasy. Teraz najważniejsze: **jak wyciągać dane, żeby było szybko i poprawnie**.
Tu junior najczęściej się wykłada (problem N+1, ładowanie za dużo, filtrowanie w pamięci) — i tu robisz różnicę
w prawdziwym projekcie. To bezpośrednia kontynuacja rdzenia z [06](./06-ef-core-fundamenty.md).

## Mostek z tego, co już znasz

W Eloquent robiłeś `Course::with('modules')->where(...)->get()` i pamiętasz „eager vs lazy loading" oraz problem
N+1. EF Core to to samo, tylko bardziej jawne: sam decydujesz `Include`, sam wybierasz `AsNoTracking`, sam widzisz,
kiedy zapytanie wykonuje się w bazie, a kiedy w pamięci.

---

## LINQ → SQL: kiedy zapytanie „idzie do bazy"

Kluczowa intuicja: `DbSet<Course>` to **zapytanie**, nie dane. Dopóki nie zawołasz metody „materializującej"
(`ToListAsync`, `FirstOrDefaultAsync`, `CountAsync`, `AnyAsync`), EF **nic nie wykonuje** — buduje SQL.

```csharp
var query = _context.Courses.Where(c => c.Status == CourseStatus.Published); // NIC się nie dzieje (IQueryable)
query = query.OrderBy(c => c.Title);                                          // wciąż nic
var list = await query.ToListAsync(ct);                                       // TU leci SQL do bazy
```

To potężne: możesz **składać** zapytanie warunkowo, a EF wygeneruje jeden SQL. Course Platform robi tak w
[GetCoursesQuery](../src/CoursePlatform.Application/Features/Courses/Queries/GetCourses/GetCoursesQuery.cs):

```csharp
var query = _context.Courses.AsNoTracking().AsQueryable();

if (!string.IsNullOrWhiteSpace(request.SearchTerm))
{
    var search = $"%{request.SearchTerm.Trim().ToLower()}%";
    query = query.Where(c =>
        EF.Functions.Like(c.Title.ToLower(), search) ||
        EF.Functions.Like(c.ShortDescription.ToLower(), search) /* ... */);
}
if (request.Level.HasValue)
    query = query.Where(c => c.Level == request.Level.Value);

var totalCount = await query.CountAsync(ct);          // jeden SQL: COUNT
var items = await query.Skip(...).Take(...).Select(...).ToListAsync(ct);  // drugi SQL: strona danych
```

Filtry doklejają się warunkowo, a do bazy idą **dwa** zapytania (liczba + strona), nie tysiąc.

> **`EF.Functions.Like` + `ToLower()`** to sposób Course Platform na wyszukiwanie **niewrażliwe na wielkość
> liter** — `ToLower()` tłumaczy się na SQL `lower(...)`, więc „vue" znajdzie „Vue 3". Szczegóły i alternatywy
> (Postgres FTS, Elasticsearch) w [13](./13-wyszukiwanie-i-elasticsearch.md).

---

## Ładowanie relacji — Eager, Lazy, Explicit

Encja `Course` ma `Modules`, a każdy `Module` ma `Lessons`. Domyślnie EF **nie** ładuje relacji — musisz
poprosić.

### Eager loading (`.Include`) — używaj świadomie

```csharp
var course = await _context.Courses
    .Include(c => c.Modules)
        .ThenInclude(m => m.Lessons)
    .FirstOrDefaultAsync(c => c.Id == id, ct);
```

Jeden (lub kilka) SQL z JOIN-ami — wszystko od razu. **Plus:** komplet danych. **Minus:** możesz pobrać za dużo
(np. wszystkie lekcje, gdy potrzebujesz tylko tytułów).

### Lazy loading — domyślnie WYŁĄCZONE w EF Core (i dobrze)

```csharp
// wymaga virtual + proxy + .UseLazyLoadingProxies()
var course = await _context.Courses.FindAsync(id);
var modules = course.Modules;  // DODATKOWE zapytanie SQL "w locie" przy dostępie
```

To wygląda niewinnie, ale kryje **problem N+1**: pętla po 100 kursach, każdy dotyka `course.Modules` → **101
zapytań** do bazy. API nagle jest wolne, a junior nie wie dlaczego. Course Platform **nie używa** lazy loadingu —
i słusznie.

### Explicit loading — ręcznie, gdy potrzebujesz

```csharp
var course = await _context.Courses.FindAsync(id);
await _context.Entry(course).Collection(c => c.Modules).LoadAsync(ct);
```

Świadomie mówisz „teraz doładuj moduły". Rzadziej używane, ale bywa przydatne.

### Najlepsze: projekcja przez `Select` (często zamiast `Include`)

Gdy potrzebujesz tylko części pól, projektuj od razu do DTO — EF pobierze **tylko** te kolumny:

```csharp
var items = await _context.Courses
    .Where(c => c.Status == CourseStatus.Published)
    .Select(c => new CourseListDto(
        c.Id, c.Title, c.Price,
        c.Modules.Count,                                  // policzy w SQL
        c.Modules.SelectMany(m => m.Lessons).Count(),     // policzy w SQL
        c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0))
    .ToListAsync(ct);
```

To realny wzorzec z `GetCoursesQuery`. Zamiast ściągać całe kursy z modułami i liczyć w pamięci, EF policzy
agregaty w bazie i zwróci gotowe DTO. **To jest „manual mapping"** ([09](./09-architektura-clean-cqrs-mediatr.md)):
mapujesz encję → DTO w `Select`, bez AutoMappera.

---

## `AsNoTracking()` — dla zapytań tylko do odczytu

Domyślnie EF **śledzi** pobrane encje (Change Tracker — niżej), żeby wiedzieć, co zmienić przy zapisie. Przy
zapytaniach, które tylko czytają (lista kursów do wyświetlenia), to zbędny narzut. `AsNoTracking()` go wyłącza:

```csharp
var list = await _context.Courses.AsNoTracking().ToListAsync(ct);  // szybciej, mniej RAM
```

Course Platform używa `AsNoTracking()` we wszystkich zapytaniach-odczytach (Queries). W komendach, które coś
zmieniają, śledzenie zostaje włączone.

---

## Change Tracker — serce zapisu

Gdy pobierzesz encję **ze śledzeniem** (bez `AsNoTracking`), EF pamięta jej stan i przy `SaveChangesAsync`
wygeneruje odpowiedni SQL:

```csharp
var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id, ct); // śledzona
course.Title = "Nowy tytuł";
course.MarkUpdated();
await _context.SaveChangesAsync(ct);   // EF widzi zmianę Title → UPDATE tylko tej kolumny
```

Nie wołasz żadnego „update" — EF sam porównuje snapshot i generuje `UPDATE`. Stany encji:

| Stan | Znaczenie | Efekt przy SaveChanges |
|------|-----------|------------------------|
| `Detached` | EF nie śledzi | nic |
| `Unchanged` | Bez zmian | nic |
| `Added` | Nowa (`_context.X.Add(...)`) | INSERT |
| `Modified` | Zmieniona | UPDATE |
| `Deleted` | Usunięta (`Remove`) | DELETE |

Podgląd stanu (debug): `_context.Entry(course).State;`

To dlatego Course Platform w komendach robi po prostu:

```csharp
_context.Enrollments.Add(enrollment);   // stan Added
await _context.SaveChangesAsync(ct);     // INSERT
```

---

## `SaveChangesAsync` i transakcje

`SaveChangesAsync` zapisuje **wszystkie** śledzone zmiany w **jednej transakcji**. Jeśli dodasz kilka encji i
zawołasz raz `SaveChangesAsync`, albo wszystko się zapisze, albo nic (atomowość) — bez ręcznej transakcji.

Ręczna transakcja przydaje się, gdy chcesz zgrupować **wiele** `SaveChanges` albo operacje niestandardowe:

```csharp
await using var tx = await _context.Database.BeginTransactionAsync(ct);
try
{
    _context.Courses.Add(course);
    await _context.SaveChangesAsync(ct);
    // ... coś jeszcze
    await tx.CommitAsync(ct);
}
catch
{
    await tx.RollbackAsync(ct);
    throw;
}
```

W praktyce większość operacji Course Platform mieści się w jednym `SaveChangesAsync`, więc jawne transakcje nie
są potrzebne.

### Obsługa naruszenia unikalnego constraintu (praktyka z projektu)

Gdy baza odrzuci zapis przez unikalny indeks (np. duplikat enrollmentu w wyścigu), EF rzuca `DbUpdateException`.
Course Platform łapie to i zamienia na sensowny błąd 400 — ale **tylko** jeśli faktycznie istnieje duplikat
(inaczej rzuca dalej, żeby nie ukryć prawdziwej awarii):

```csharp
try
{
    await _context.SaveChangesAsync(ct);
}
catch (DbUpdateException)
{
    var already = await _context.Enrollments
        .AnyAsync(e => e.UserId == userId && e.CourseId == request.CourseId, ct);
    if (!already) throw;                      // to nie duplikat — prawdziwy błąd, rzuć dalej
    throw new ValidationException(/* "Jesteś już zapisany na ten kurs." */);
}
```

To ładne połączenie [06](./06-ef-core-fundamenty.md) (unikalny constraint) i [09](./09-architektura-clean-cqrs-mediatr.md)
(walidacja/wyjątki). Zapamiętaj wzorzec: **twarda gwarancja w bazie + eleganckie tłumaczenie błędu w kodzie**.

---

## Stored procedures i surowy SQL (dla świadomości)

EF pozwala wołać procedury składowane / surowy SQL, gdy LINQ nie wystarcza (złożony raport, legacy baza,
optymalizacja przez DBA):

```csharp
var results = await _context.Courses
    .FromSqlRaw("SELECT * FROM get_published_courses({0})", minPrice)
    .ToListAsync(ct);
```

**Kiedy:** złożone raporty, mocna optymalizacja. **Domyślnie w nowym kodzie:** LINQ. Course Platform nie używa
procedur — ale wiedz, że to opcja (temat „Stored Procedures" jest na roadmapie).

---

## SQL Server vs PostgreSQL — dla juniora .NET

Course Platform używa **PostgreSQL** (`UseNpgsql`), ale na rynku .NET spotkasz też **SQL Server** (`UseSqlServer`,
ekosystem Microsoft/Azure). Dobra wiadomość: **umiejętność EF przenosi się 1:1** — zmienia się provider i
connection string, LINQ zostaje ten sam.

```csharp
options.UseNpgsql(connectionString);   // Course Platform (PostgreSQL)
options.UseSqlServer(connectionString);// alternatywa (SQL Server)
```

Drobne różnice (typy, niektóre funkcje SQL, kolacje) istnieją, ale na poziomie juniora nie blokują. Typowe
mapowania typów: `Guid`↔`uniqueidentifier`/`uuid`, `decimal`↔`decimal/numeric`, `DateTime`↔`timestamp`,
`bool`↔`boolean/bit`.

---

## Pułapki (najważniejszy rozdział pod rozmowy)

1. **N+1 queries.** Pętla + lazy/osobne zapytania. Rozwiązanie: `Include` albo (lepiej) projekcja `Select`.
2. **Filtrowanie w pamięci zamiast w SQL.** `ToList()` **przed** `Where()` ściąga całą tabelę:
   ```csharp
   var x = _context.Courses.ToList().Where(c => c.Title.Contains("Vue")); // ŹLE: cała tabela do RAM
   var y = await _context.Courses.Where(c => c.Title.Contains("Vue")).ToListAsync(ct); // DOBRZE: filtr w SQL
   ```
3. **Brak `AsNoTracking` w odczytach.** Zbędny narzut śledzenia przy zapytaniach tylko do odczytu.
4. **Over-fetching przez `Include`.** Ściągasz całe grafy, gdy potrzebujesz kilku pól. Projektuj przez `Select`.
5. **Concurrency.** Dwóch userów edytuje ten sam rekord → można stracić zmianę. Rozwiązanie: token wersji
   (`rowversion`/`[Timestamp]`) → `DbUpdateConcurrencyException`. (🔴 temat na później, ale warto znać nazwę.)
6. **Brak `CancellationToken` w zapytaniach.** Przekazuj go do każdej metody `...Async`.

## Ćwiczenia

1. 🟢 **Znajdź projekcję.** W `GetCoursesQuery` wskaż, gdzie encja jest mapowana na DTO (`Select`) i które
   agregaty liczą się w SQL.
2. 🟢 **AsNoTracking.** Policz w `Application/Features/**`, ile zapytań-odczytów używa `AsNoTracking()`. Dlaczego
   akurat one?
3. 🟡 **Napraw N+1 (na papierze).** Masz kod: `foreach (var c in await _context.Courses.ToListAsync()) { var n = c.Modules.Count; }`
   (przy lazy loadingu). Przepisz na wersję bez N+1 dwoma sposobami: `Include` i `Select`.
4. 🟡 **Nowe zapytanie.** Napisz LINQ zwracające 5 najdroższych opublikowanych kursów z liczbą lekcji, jako DTO.
   *Done, gdy* wszystko liczy się w bazie (żadnej pętli w C#).
5. 🔴 **Change Tracker.** Prześledź w handlerze aktualizacji (np. `UpdateCourseCommand`): która encja jest
   śledzona, kiedy staje się `Modified`, i dlaczego nie trzeba wołać żadnego „Update()".

## Pytania kontrolne

1. Kiedy zapytanie EF faktycznie wykonuje się na bazie?
2. Czym jest problem N+1 i jak go rozwiązać?
3. Kiedy użyjesz `Include`, a kiedy projekcji `Select`?
4. Po co `AsNoTracking()` i gdzie go NIE używać?
5. Jak działa Change Tracker — dlaczego zmiana property + `SaveChangesAsync` wystarcza do UPDATE?
6. Jak Course Platform obsługuje naruszenie unikalnego constraintu i dlaczego rzuca dalej, gdy to nie duplikat?

<details>
<summary>Rozwiązania</summary>

1. Dopiero przy metodzie materializującej: `ToListAsync`, `FirstOrDefaultAsync`, `CountAsync`, `AnyAsync` itp.
   Wcześniej `IQueryable` tylko buduje SQL.
2. N+1: jedno zapytanie o listę + po jednym zapytaniu na każdy element (np. przy lazy loadingu w pętli).
   Rozwiązanie: `Include` (JOIN) albo projekcja `Select` (pobiera tylko potrzebne pola/agregaty).
3. `Include` — gdy potrzebujesz pełnych powiązanych encji. `Select` — gdy potrzebujesz tylko części pól/agregatów
   (mniej danych, szybciej). W praktyce `Select` do odczytów jest często lepszy.
4. `AsNoTracking()` wyłącza śledzenie zmian — szybsze, mniej pamięci, dla odczytów. NIE używaj, gdy zamierzasz
   zmodyfikować i zapisać pobraną encję (bo wtedy potrzebujesz śledzenia).
5. Change Tracker trzyma snapshot pobranej encji; przy `SaveChangesAsync` porównuje go z aktualnym stanem i
   generuje UPDATE dla zmienionych kolumn. Dlatego wystarczy zmienić property.
6. Łapie `DbUpdateException`, sprawdza `AnyAsync`, czy rekord-duplikat istnieje; jeśli tak → zwraca 400 z
   komunikatem; jeśli nie → `throw;` (to była inna awaria, np. FK/DB — nie wolno jej ukryć jako „duplikat").

</details>

## Idź dalej

➡️ **[08 — Dependency Injection](./08-dependency-injection.md)** — jak .NET spina wszystkie te klasy
(DbContext, handlery, serwisy) i podaje je tam, gdzie trzeba.

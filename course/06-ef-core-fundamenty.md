# 06 — Entity Framework Core: fundamenty

> **Poziom:** 🟡 rdzeń juniora · **Czas:** ~120 min · **Wymaga:** [01](./01-dotnet-i-csharp-od-zera.md), [05](./05-kontrolery-minimal-api-mvc-blazor.md)

## Po co ci to

To jest **rdzeń** pracy backendowca w .NET. 90% czasu w prawdziwym projekcie to rozmowa z bazą: zapisz, odczytaj,
zaktualizuj, usuń. **Entity Framework Core (EF Core)** to sposób, w jaki .NET to robi — mapuje tabele SQL na
obiekty C#, generuje SQL z twojego LINQ i wersjonuje schemat bazy przez migracje. Ten i następny rozdział to
najważniejsza część kursu pod pracę.

## Mostek z tego, co już znasz

- Znasz SQL — świetnie, będziesz rozumiał, co EF generuje pod spodem (i kiedy generuje głupoty).
- W Laravelu używałeś **Eloquent** (ORM) i migracji — EF Core to ten sam pomysł: klasy ↔ tabele + migracje.
- `DbContext` ≈ „sesja z bazą" / odpowiednik połączenia + jednostki pracy. `DbSet<Course>` ≈ tabela `courses`.

Różnica względem Eloquent: EF jest **bardziej jawny** (piszesz LINQ, kontrolujesz ładowanie relacji) i mocniej
typowany.

---

## Czym jest ORM

**ORM (Object-Relational Mapper)** mapuje wiersze tabel na obiekty w kodzie. Bez ORM:

```sql
SELECT "Id", "Title" FROM "Courses" WHERE "Status" = 1;
```
→ ręcznie czytasz wynik, tworzysz `new Course { ... }`, pilnujesz typów. Nudne i podatne na błędy.

Z EF Core:

```csharp
var courses = await _context.Courses
    .Where(c => c.Status == CourseStatus.Published)
    .ToListAsync(cancellationToken);
```

EF **tłumaczy LINQ na SQL**, wykonuje, mapuje wynik na obiekty. Piszesz w jednym języku (C#), nie żonglujesz
stringami SQL.

> ORM nie zwalnia cię z myślenia o SQL. Wręcz przeciwnie — musisz wiedzieć, **jaki SQL** EF wygeneruje, żeby nie
> zrobić czegoś wolnego (o tym w [07](./07-ef-core-zapytania-i-wydajnosc.md)).

---

## Code First — jak w Course Platform

Są dwa podejścia: **Database First** (baza istnieje, generujesz z niej klasy) i **Code First** (piszesz klasy,
generujesz z nich bazę). Course Platform i większość nowych projektów to **Code First**:

```
1. Piszesz encje C#         (Course, Module, Lesson)
2. Piszesz konfigurację     (Fluent API: CourseConfiguration)
3. dotnet ef migrations add Nazwa   → EF generuje plik migracji
4. dotnet ef database update        → migracja tworzy/zmienia tabele w bazie
```

**Migracje są w git** — dzięki temu każdy developer i CI dostają ten sam schemat bazy, krok po kroku. To jak
wersjonowanie struktury bazy.

---

## Encje — klasy mapowane na tabele

Encja to zwykła klasa C# (poznałeś je w [01](./01-dotnet-i-csharp-od-zera.md)). W Course Platform bazowa klasa
[BaseEntity](../src/CoursePlatform.Domain/Common/BaseEntity.cs) daje wspólne pola:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public void MarkUpdated() => UpdatedAt = DateTime.UtcNow;
}
```

Encja `Course` dziedziczy po niej i dokłada swoje pola oraz **relacje**:

```csharp
public class Course : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public CourseStatus Status { get; set; } = CourseStatus.Draft;

    public Guid InstructorId { get; set; }              // klucz obcy (FK)
    public ApplicationUser Instructor { get; set; } = null!;  // nawigacja do właściciela

    public ICollection<Module> Modules { get; set; } = new List<Module>();  // relacja 1:wiele
    public ICollection<Category> Categories { get; set; } = new List<Category>(); // relacja wiele:wiele
}
```

- `InstructorId` (FK) + `Instructor` (property nawigacyjna) = relacja „kurs należy do instruktora".
- `ICollection<Module> Modules` = relacja „kurs ma wiele modułów".
- `ICollection<Category>` po obu stronach = relacja **wiele-do-wielu** (kurs ma kategorie, kategoria ma kursy).

---

## DbContext — „sesja" z bazą

`DbContext` to centralny obiekt EF: reprezentuje połączenie, śledzi zmiany i zapisuje je w jednej transakcji.
Course Platform ma [ApplicationDbContext](../src/CoursePlatform.Infrastructure/Persistence/ApplicationDbContext.cs):

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    // ...
}
```

- Każdy `DbSet<T>` odpowiada tabeli (i pozwala pisać na niej zapytania LINQ).
- `DbContext` jest **Scoped** — jeden na jedno żądanie HTTP (dlaczego, wyjaśnimy w [08](./08-dependency-injection.md)).
- Zapisuje zmiany przez `await _context.SaveChangesAsync(ct)` — o tym w [07](./07-ef-core-zapytania-i-wydajnosc.md).

> Zauważ interfejs `IApplicationDbContext` — to abstrakcja z warstwy Application. Handlery zależą od interfejsu,
> nie od konkretnego `ApplicationDbContext` (Clean Architecture, [09](./09-architektura-clean-cqrs-mediatr.md)).

---

## Fluent API — konfiguracja mapowania

Domyślnie EF zgaduje mapowanie z konwencji (nazwa property `Id` → klucz główny itd.). Gdy chcesz coś doprecyzować
(długość kolumny, unikalność, relacje, kaskady) — używasz **Fluent API** w osobnych klasach `IEntityTypeConfiguration<T>`.

Course Platform trzyma je w `Persistence/Configurations/`. Przykład —
[CourseConfiguration](../src/CoursePlatform.Infrastructure/Persistence/Configurations/CourseConfiguration.cs):

```csharp
public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Price).HasPrecision(18, 2);   // decimal(18,2) — pieniądze

        builder.HasOne(c => c.Instructor)                     // relacja: kurs → instruktor
            .WithMany(u => u.Courses)
            .HasForeignKey(c => c.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);               // nie kasuj kursów przy usuwaniu usera

        builder.HasMany(c => c.Categories).WithMany(cat => cat.Courses);  // wiele:wiele

        builder.HasIndex(c => c.Status);                      // indeks pod filtrowanie po statusie
    }
}
```

### Constraints jako logika biznesowa

Najciekawszy przykład to **unikalny indeks** — to reguła biznesowa wyrażona w bazie. „Nie można zapisać się na
kurs dwa razy" ([EnrollmentConfiguration](../src/CoursePlatform.Infrastructure/Persistence/Configurations/EnrollmentConfiguration.cs)):

```csharp
builder.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
```

W SQL to:

```sql
CREATE UNIQUE INDEX "IX_Enrollments_UserId_CourseId" ON "Enrollments" ("UserId", "CourseId");
```

Baza **fizycznie nie pozwoli** na duplikat — nawet jeśli dwa żądania przyjdą w tej samej milisekundzie. To
mocniejsza gwarancja niż sprawdzenie w kodzie (wrócimy do tego przy race conditions w [09](./09-architektura-clean-cqrs-mediatr.md)).

Analogiczne unikalne constrainty w projekcie: `LessonProgress` (user+lekcja), `Review` (user+kurs).

> **Dlaczego osobne klasy konfiguracji, a nie atrybuty na encjach?** Bo encje w tym projekcie są **czyste**
> (warstwa Domain, zero zależności od EF). Konfiguracja mapowania to szczegół infrastruktury — mieszka w
> Infrastructure. To zasada Clean Architecture ([09](./09-architektura-clean-cqrs-mediatr.md)).

---

## Migracje — wersjonowanie schematu

```bash
# w kontenerze api, z katalogu /src
dotnet ef migrations add AddCourseRating \
  --project src/CoursePlatform.Infrastructure --startup-project src/CoursePlatform.API

dotnet ef database update \
  --project src/CoursePlatform.Infrastructure --startup-project src/CoursePlatform.API
```

- `migrations add` — porównuje twoje encje/konfigurację z ostatnim „snapshotem" i generuje plik z metodami
  `Up()` (zastosuj zmianę) i `Down()` (cofnij).
- `database update` — wykonuje migracje na bazie.
- Pliki migracji (`Migrations/`) **commitujesz** do repo.

Course Platform ma m.in. migracje `InitialCreate`, `AddCategoriesTechnologiesAndCourseLanguage`,
`AddUserStatistics`, a nawet parę „historycznych" (`AddCaseInsensitiveTextCollation` → potem cofnięte przez
`RemoveTextCollation`, bo case-insensitivity rozwiązano inaczej — patrz [13](./13-wyszukiwanie-i-elasticsearch.md)).
To normalne: schemat ewoluuje, a migracje są zapisem tej ewolucji.

> **Auto-migracja przy starcie.** Course Platform woła `context.Database.MigrateAsync()` w `Program.cs` (poza
> środowiskiem Testing). Wygodne lokalnie. Na produkcji z wieloma instancjami to bywa ryzykowne (wyścig migracji)
> — wtedy migruje się osobnym krokiem w CI/CD. Dla nauki: wiedz, że to kompromis. (Więcej w [17](./17-docker-cicd-produkcja.md).)

---

## Database design — minimum, które łączy się z EF

Znasz bazy, więc tylko to, co realnie dotyka backendu .NET:

- **Normalizacja (1NF–3NF)** — unikaj duplikacji danych; wyciągaj powtarzające się rzeczy do osobnych tabel
  (np. `Category` zamiast stringa w każdym kursie).
- **Klucze obce + constrainty** — wyrażasz je Fluent API (`HasForeignKey`, `IsUnique`, `OnDelete`).
- **Indeksy** — pod zapytania, które robisz często (`HasIndex(c => c.Status)`, bo filtrujesz po statusie).
- **`OnDelete` (kaskady)** — co się dzieje z „dziećmi" przy usunięciu „rodzica": `Cascade` (usuń dzieci),
  `Restrict` (zablokuj usunięcie), `SetNull`. Course Platform używa `Restrict` na instruktorze (nie kasuj jego
  kursów przy usuwaniu konta) i `Cascade` tam, gdzie dzieci nie mają sensu bez rodzica.

### NoSQL — jednym zdaniem (żebyś wiedział, że istnieje)

Poza relacyjnymi bazami są dokumentowe (**MongoDB**), klucz-wartość i chmurowe (**DynamoDB**, **Cosmos DB**).
Sięgasz po nie, gdy model jest dokumentowy albo potrzebujesz ekstremalnej skali/geografii — **nie** tam, gdzie
liczą się relacje i transakcje (enrollmenty, płatności). W Course Platform relacyjny PostgreSQL to właściwy wybór.

---

## Pułapki

1. **Encja EF zwracana z API.** Nie zwracaj encji bezpośrednio — mapuj na DTO ([05](./05-kontrolery-minimal-api-mvc-blazor.md), [09](./09-architektura-clean-cqrs-mediatr.md)).
2. **Zapomniana migracja.** Zmieniłeś encję, ale nie wygenerowałeś migracji → baza i kod się rozjeżdżają, błędy
   przy starcie/zapytaniach.
3. **Ręczna edycja bazy zamiast migracji.** Zmiany „na piechotę" w bazie nie trafią do repo/CI. Zawsze migracja.
4. **Brak unikalnego constraintu na regułę biznesową.** Sprawdzenie „czy już istnieje" tylko w kodzie nie chroni
   przed wyścigiem dwóch żądań. Unikalny indeks w bazie chroni.
5. **`decimal` bez `HasPrecision`.** Domyślna precyzja może obciąć grosze — dla pieniędzy ustaw `HasPrecision(18,2)`.

## Ćwiczenia

> Uruchom projekt. Pracuj w `src/CoursePlatform.Domain/Entities/` i `.../Infrastructure/Persistence/Configurations/`.

1. 🟢 **Mapa encji.** Wypisz 5 encji Course Platform i ich relacje (1:wiele / wiele:wiele). Narysuj prosty diagram.
2. 🟢 **Znajdź constrainty.** Wskaż 3 unikalne indeksy w `Configurations/` i powiedz, jaką regułę biznesową
   wyrażają.
3. 🟡 **Nowe pole + migracja.** Dodaj do `Course` property `Subtitle` (string, opcjonalny, max 300). Skonfiguruj
   w `CourseConfiguration`, wygeneruj migrację i zaaplikuj. *Done, gdy* `dotnet ef database update` przechodzi, a
   w bazie jest nowa kolumna.
4. 🟡 **Prześledź encję Certificate.** Otwórz istniejącą encję `Certificate`, `CertificateConfiguration` i
   `CertificateIssuer`. Wypisz relacje (user, kurs), unikalny constraint i pola (`Number`, `IssuedAt`, `PdfObjectKey`).
   Porównaj z tym, co sam byś zaprojektował.
5. 🔴 **Kaskady.** Prześledź, co się stanie z `Module` i `Lesson` przy usunięciu `Course` (sprawdź `OnDelete` w
   konfiguracjach). Czy to bezpieczne? Co byś zmienił i dlaczego?

## Pytania kontrolne

1. Co to jest ORM i co EF robi z twoim LINQ?
2. Czym różni się Code First od Database First? Które ma Course Platform?
3. Do czego służy `DbContext`, a do czego `DbSet<T>`?
4. Jak Fluent API wyraża „nie można zapisać się na kurs dwa razy"? Dlaczego w bazie, a nie tylko w kodzie?
5. Co robi `migrations add`, a co `database update`? Co commitujesz do repo?
6. Kiedy `OnDelete` ustawisz na `Restrict`, a kiedy na `Cascade`?

<details>
<summary>Rozwiązania</summary>

1. ORM mapuje wiersze tabel na obiekty. EF tłumaczy zapytania LINQ na SQL, wykonuje je i mapuje wynik na encje.
2. Code First: piszesz klasy → generujesz bazę (migracje). Database First: baza istnieje → generujesz klasy.
   Course Platform to Code First.
3. `DbContext` to sesja z bazą (połączenie, śledzenie zmian, transakcja przy `SaveChanges`). `DbSet<T>` to
   reprezentacja tabeli, na której piszesz zapytania.
4. `builder.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();` → unikalny indeks w SQL. W bazie, bo chroni
   przed wyścigiem dwóch równoczesnych żądań (kod sam tego nie gwarantuje) i jest twardą regułą integralności.
5. `migrations add` generuje plik zmiany schematu (Up/Down) z różnicy encji vs snapshot; `database update`
   wykonuje go na bazie. Do repo commitujesz pliki migracji.
6. `Restrict` — gdy nie chcesz kasować powiązanych danych przy usuwaniu rodzica (np. kursy instruktora przy
   usunięciu konta). `Cascade` — gdy dzieci nie mają sensu bez rodzica (np. lekcje bez modułu/kursu).

</details>

## Idź dalej

➡️ **[07 — EF Core: zapytania i wydajność](./07-ef-core-zapytania-i-wydajnosc.md)** — jak pisać zapytania,
ładować relacje, śledzić zmiany i nie zabić bazy problemem N+1.

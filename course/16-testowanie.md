# 16 — Testowanie

> **Poziom:** 🟡 rdzeń juniora · **Czas:** ~90 min · **Wymaga:** [08](./08-dependency-injection.md), [09](./09-architektura-clean-cqrs-mediatr.md)

## Po co ci to

Testy to nie „miły dodatek" — to sposób, by zmieniać kod bez strachu i by na rozmowie pokazać, że rozumiesz
jakość. Junior, który umie napisać sensowny test handlera i walidatora, wyróżnia się. Course Platform ma realne
testy (xUnit + FluentAssertions + Moq + WebApplicationFactory) — prześledzimy je i nauczysz się dokładać własne.

## Mostek z tego, co już znasz

- We froncie może robiłeś testy z Vitest/Jest (`describe`/`it`/`expect`). W .NET: **xUnit** (`[Fact]`/`[Theory]`)
  + biblioteka asercji. Ta sama filozofia: Arrange-Act-Assert.
- „Mock" (atrapa zależności) znasz z JS — w .NET najczęściej **Moq** albo NSubstitute.

---

## Piramida testów

```
        /\
       /E2E\        mało, wolne, kruche (Playwright: przeglądarka + cały system)
      /------\
     /Integr. \     średnio (WebApplicationFactory: prawdziwa app w pamięci, HTTP)
    /----------\
   /   Unit     \   dużo, szybkie (handlery, walidatory — logika w izolacji)
  /--------------\
```

Zasada: **dużo szybkich testów jednostkowych** (baza piramidy), mniej integracyjnych, garść E2E. Odwrócona
piramida (dużo wolnych E2E, mało unit) = wolny, kruchy zestaw testów.

---

## xUnit — framework testowy

```csharp
public class CreateCourseCommandValidatorTests
{
    private readonly CreateCourseCommandValidator _validator = new();

    [Fact]                                    // pojedynczy test
    public void EmptyTitle_HasError()
    {
        var cmd = new CreateCourseCommand("", "Desc", "Short", 0, CourseLevel.Beginner, "", "pl",
                                          new List<Guid>(), new List<Guid>());
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]                                  // test parametryzowany
    [InlineData(-1)]
    [InlineData(-100)]
    public void NegativePrice_HasError(decimal price) { /* ... */ }
}
```

- `[Fact]` — jeden przypadek. `[Theory]` + `[InlineData]` — ten sam test dla wielu danych.
- Konstruktor klasy testowej = „setup" przed każdym testem (nowa instancja na każdy test — izolacja).
- Wzorzec **AAA**: *Arrange* (przygotuj), *Act* (wykonaj), *Assert* (sprawdź).

To realny test z Course Platform ([tests/CoursePlatform.Application.UnitTests/Features/Courses/Validators/](../tests/CoursePlatform.Application.UnitTests/Features/Courses/Validators/CreateCourseCommandValidatorTests.cs)).

---

## Biblioteka asercji — stan projektu i rekomendacja

Course Platform używa **FluentAssertions 8.0.0** — czytelne asercje `result.Should()...`:

```csharp
result.Should().NotBeNull();
result.Items.Should().ContainSingle(c => c.Title == "Published");
await act.Should().ThrowAsync<ForbiddenAccessException>();
```

> **Uwaga licencyjna (2026):** FluentAssertions **8.0** przeszło na **licencję komercyjną**. Roadmapa rekomenduje
> teraz **Shouldly** (darmowe). Składnia Shouldly: `result.Items.Count.ShouldBe(3);`. Dla projektu portfolio FA 8
> jest OK (darmowe do zastosowań niekomercyjnych/otwartych), ale w komercji warto rozważyć migrację na Shouldly
> lub wbudowane asercje xUnit. Na rozmowie: znaj oba i wiedz o kwestii licencji.

---

## Test jednostkowy handlera (z bazą InMemory)

Handlery czyta się/testuje w izolacji. Course Platform używa `DbContext` na dostawcy **InMemory** (baza w pamięci)
+ Moq na `ICurrentUserService`. Przykład idei z `GetCoursesQueryTests`:

```csharp
public GetCoursesQueryTests()
{
    var options = new DbContextOptionsBuilder<TestDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())   // izolowana baza per test
        .Options;
    _context = new TestDbContext(options);
    _currentUserServiceMock = new Mock<ICurrentUserService>();
}

[Fact]
public async Task Handle_LoggedInNonAdmin_SeesOnlyPublished()
{
    // Arrange: seed Published + Draft, user = zwykły (nie admin)
    _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
    _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(false);

    // Act
    var handler = new GetCoursesQueryHandler(_context, _currentUserServiceMock.Object);
    var result = await handler.Handle(new GetCoursesQuery(/* ... */), CancellationToken.None);

    // Assert
    result.Items.Should().ContainSingle(c => c.Title == "Published");
}
```

To pokazuje, **dlaczego** warstwa Application zależy od interfejsów ([09](./09-architektura-clean-cqrs-mediatr.md)):
`ICurrentUserService` podmieniamy Mockiem, `IApplicationDbContext` na InMemory — testujemy logikę bez HTTP i bez
prawdziwej bazy.

### Moq — atrapy zależności

```csharp
var mock = new Mock<ICurrentUserService>();
mock.Setup(x => x.UserId).Returns(userId);      // "gdy ktoś zapyta o UserId, zwróć to"
mock.Setup(x => x.IsAdmin).Returns(false);
var handler = new SomeHandler(mock.Object);      // .Object to atrapa implementująca interfejs
```

Course Platform używa Moq (paczka `Moq`) m.in. w testach `GetLessonQuery`, `GetCourses`, `CreateModule`.
Alternatywa: **NSubstitute** (inna składnia, ta sama idea).

### AutoFixture (wzmianka, „niebieskie")

Generuje sensowne dane testowe, żebyś nie wypełniał 10 pól ręcznie:

```csharp
var fixture = new Fixture();
var command = fixture.Create<CreateCourseCommand>();   // losowe, poprawne dane
```

Przydatne, gdy interesuje cię jedno pole, a reszta ma być „jakakolwiek poprawna". Course Platform go nie używa —
warto znać.

---

## Test integracyjny — prawdziwe HTTP (WebApplicationFactory)

Test jednostkowy sprawdza kawałek. Test **integracyjny** uruchamia **całą aplikację w pamięci** i puka do niej
przez HTTP — sprawdza, że wszystko (routing, DI, middleware, serializacja) współgra. Course Platform ma
`CustomWebApplicationFactory`:

```csharp
public class CoursesApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    public CoursesApiTests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task GetCourses_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/courses?pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");                 // środowisko Testing → InMemory DB, bez migracji
        builder.UseSetting("Jwt:Key", "integration-test-jwt-signing-key-32chars");
        // + Issuer/Audience
    }
}
```

`WebApplicationFactory<Program>` startuje prawdziwą aplikację (dlatego `Program.cs` kończy się `public partial
class Program { }` — żeby test miał do niego dostęp). Środowisko `Testing` przełącza bazę na InMemory
([03](./03-pierwsza-aplikacja-host-program-cs.md)).

---

## Testcontainers — prawdziwy PostgreSQL w testach (🔴 „awans")

InMemory jest szybkie, ale **nie jest** PostgreSQL: nie sprawdza unikalnych constraintów, kolacji, tłumaczenia
niektórych zapytań, transakcji. **Testcontainers** uruchamia prawdziwy PostgreSQL w Dockerze na czas testu:

```csharp
await using var db = new PostgreSqlBuilder().Build();
await db.StartAsync();
// podłącz connection string do WebApplicationFactory → testy na prawdziwej bazie
```

Zalety: łapiesz błędy, których InMemory nie widzi (np. naruszenie unikalnego indeksu z [06](./06-ef-core-fundamenty.md)).
Koszt: wolniejsze, wymaga dostępu do Dockera z procesu testów. To „niebieskie" na roadmapie i realny standard w
poważniejszych projektach — Course Platform go nie ma (InMemory), ale to naturalny kolejny krok jakości.

---

## E2E i BDD (wzmianka)

- **Playwright** (🔵) — automatyzacja przeglądarki: testuje **cały** system (Vue + API) jak użytkownik. Dominuje w
  .NET E2E nad Cypress/Puppeteer.
  ```csharp
  await page.GotoAsync("http://localhost:5173/login");
  await page.FillAsync("[data-testid=email]", "student@test.com");
  await page.ClickAsync("[data-testid=submit]");
  await Expect(page).ToHaveURLAsync("**/courses");
  ```
- **SpecFlow** (🔵) — BDD: scenariusze w języku naturalnym (Gherkin `Given/When/Then`) wiązane z kodem C#. Używane
  w zespołach z QA/analitykami.

---

## Co testować jako junior (praktyczna rada)

- **Walidatory** — reguły wejścia (`CreateCourseCommandValidator`) — łatwe, szybkie, wartościowe.
- **Handlery** — logika biznesowa z InMemory + Moq (np. „non-admin widzi tylko Published", „brak dostępu → 403").
- **1–2 testy integracyjne** na kluczowe endpointy (że w ogóle wstają i zwracają sensowny kod).
- **Nie testuj frameworka** — czy ASP.NET routuje albo EF generuje SQL to robota Microsoftu, nie twoja.

Dobre testy handlera Course Platform to np.: „instruktor widzi lekcję własnego kursu bez enrollmentu", „admin
widzi", „obcy student → Forbidden" (realne testy `GetLessonQueryTests`).

---

## Pułapki

1. **Odwrócona piramida.** Dużo wolnych E2E, mało unit → wolny, kruchy CI.
2. **Testy zależne od siebie / kolejności.** Każdy test izolowany (osobna baza InMemory — `Guid.NewGuid()` jako nazwa).
3. **Testowanie implementacji zamiast zachowania.** Testuj „co robi", nie „jak" — inaczej każdy refactor psuje testy.
4. **InMemory tam, gdzie liczą się constrainty.** Unikalne indeksy/kolacje sprawdzisz tylko na prawdziwej bazie
   (Testcontainers).
5. **Brak testu na ścieżkę błędu.** Testuj też 403/400/404, nie tylko „happy path".

## Ćwiczenia

> Testy uruchamiasz: `docker compose exec api dotnet test`.

1. 🟢 **Uruchom testy.** Odpal cały zestaw i policz, ile testów przechodzi. Znajdź, gdzie leżą (folder `tests/`).
2. 🟢 **Przeczytaj test.** Otwórz `GetLessonQueryTests` i opisz w 3 zdaniach, co sprawdza każdy `[Fact]` (Arrange/Act/Assert).
3. 🟡 **Dopisz walidator-test.** Dodaj test do `CreateCourseCommandValidatorTests` sprawdzający, że zbyt długi
   `ShortDescription` daje błąd. *Done, gdy* test przechodzi.
4. 🟡 **Dopisz handler-test.** Napisz test, że `GetCoursesQuery` dla **anonima** z `status=Draft` zwraca tylko
   Published (Moq `IsAdmin=false`, `UserId=null`). *Done, gdy* zielony.
5. 🔴 **Zaprojektuj Testcontainers.** Naszkicuj (na papierze), jak podmienić InMemory na PostgreSQL w
   `CustomWebApplicationFactory` przez Testcontainers i który test (np. duplikat enrollmentu) zyskałby na tym.

## Pytania kontrolne

1. Opisz piramidę testów i dlaczego bazą są testy jednostkowe.
2. Czym różni się `[Fact]` od `[Theory]`?
3. Po co Mock i jak `ICurrentUserService` ułatwia testowanie handlera?
4. Co robi `WebApplicationFactory<Program>` i czym test integracyjny różni się od jednostkowego?
5. Czego InMemory NIE sprawdzi i co to naprawia?
6. Co testować, a czego nie, jako junior?

<details>
<summary>Rozwiązania</summary>

1. Dużo szybkich testów jednostkowych (baza), mniej integracyjnych, garść E2E. Unit są bazą, bo szybkie, tanie i
   precyzyjnie lokalizują błąd; E2E są wolne i kruche, więc jest ich mało.
2. `[Fact]` — jeden przypadek. `[Theory]` + `[InlineData]` — ten sam test uruchamiany dla wielu zestawów danych.
3. Mock to atrapa zależności o zadanym zachowaniu. Dzięki `ICurrentUserService` (interfejs) podstawiasz Mocka,
   który udaje zalogowanego/admina, i testujesz handler bez HTTP.
4. `WebApplicationFactory<Program>` uruchamia całą aplikację w pamięci i daje `HttpClient` do niej. Test
   integracyjny sprawdza współpracę warstw przez HTTP; jednostkowy — pojedynczy element w izolacji.
5. InMemory nie sprawdza unikalnych constraintów, kolacji, części tłumaczeń SQL, prawdziwych transakcji.
   Naprawia to Testcontainers (prawdziwy PostgreSQL w Dockerze).
6. Testuj walidatory, handlery (logikę), 1–2 endpointy integracyjnie, oraz ścieżki błędów. Nie testuj samego
   frameworka (routing/EF) — to nie twój kod.

</details>

## Idź dalej

➡️ **[17 — Docker, CI/CD, produkcja](./17-docker-cicd-produkcja.md)** — jak spakować i wdrożyć aplikację, żeby
działała nie tylko „u ciebie".

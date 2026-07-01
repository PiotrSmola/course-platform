# 20 — Plan nauki, ćwiczenia, checklist, słownik

> **Poziom:** 🟢 spinające · **Czas:** referencyjne · **Wymaga:** reszta kursu

Ten rozdział spina całość: harmonogram nauki, zbiorczy indeks ćwiczeń, checklist „gotów na rozmowę" i słownik.
Wracaj tu, gdy chcesz sprawdzić postęp albo szybko odświeżyć pojęcie.

---

## Plan nauki na 12 tygodni

Tempo dostosuj do siebie — to szkielet, nie wyrok. Każdy tydzień kończ pytaniem: **„czy potrafię wytłumaczyć to
komuś przy tablicy?"**. Jak nie — wróć do ćwiczeń.

### Tydzień 1–2 — Fundamenty 🟢
- Rozdziały: [01](./01-dotnet-i-csharp-od-zera.md) (C#/.NET), [02](./02-http-rest-i-jak-dziala-web-api.md) (HTTP/REST), [03](./03-pierwsza-aplikacja-host-program-cs.md) (Program.cs/konfiguracja).
- Cel: czytasz kod C# bez potykania się o składnię; rozumiesz cykl żądania.
- Kamień milowy: uruchom projekt, przejdź przez Swagger, zaloguj się, zawołaj chroniony endpoint.

### Tydzień 3–4 — Serce ASP.NET 🟢→🟡
- Rozdziały: [04](./04-middleware-filtry-atrybuty.md) (middleware), [05](./05-kontrolery-minimal-api-mvc-blazor.md) (kontrolery).
- Cel: rozumiesz pipeline i kolejność; umiesz opisać, jak żądanie dochodzi do handlera.
- Kamień milowy: naszkicuj własny middleware (timing) i wskaż, gdzie w kolejności go wstawić.

### Tydzień 5–7 — Dane (rdzeń!) 🟡
- Rozdziały: [06](./06-ef-core-fundamenty.md) (EF fundamenty), [07](./07-ef-core-zapytania-i-wydajnosc.md) (zapytania/wydajność).
- Cel: encje, migracje, LINQ→SQL, Include vs Select, N+1, Change Tracker.
- Kamień milowy: dodaj pole + migrację; napisz zapytanie do DTO liczące agregaty w bazie (bez pętli w C#).

### Tydzień 8–9 — DI i architektura 🟡
- Rozdziały: [08](./08-dependency-injection.md) (DI), [09](./09-architektura-clean-cqrs-mediatr.md) (Clean/CQRS/MediatR), [11](./11-logowanie-serilog.md) (Serilog).
- Cel: lifetimes bez pułapek; rozumiesz warstwy, CQRS, pipeline behaviors, manual mapping.
- Kamień milowy: zaprojektuj nowy przypadek użycia (command + handler + walidator) z resource-based auth.

### Tydzień 10 — Auth 🟡
- Rozdział: [10](./10-auth-identity-jwt.md) (Identity/JWT/resource authorization).
- Cel: cały flow logowania; 401 vs 403; resource-based authorization.
- Kamień milowy: prześledź `GetLessonQuery` — kto dostaje 403, a kto dane, i dlaczego.

### Tydzień 11 — Rozszerzenia pod pracę 🟡/🔴
- Rozdziały: [12](./12-cache-memory-redis.md) (cache), [13](./13-wyszukiwanie-i-elasticsearch.md) (search), [14](./14-komunikacja-graphql-grpc-signalr.md) (komunikacja), [15](./15-zadania-w-tle-i-brokery.md) (tło/brokery).
- Cel: wiesz, kiedy sięgnąć po Redis/ES/SignalR/broker i jaki jest koszt. (Teoria + mini-szkice, nie wdrożenia.)
- Kamień milowy: dodaj Memory Cache do `GetCategoriesQuery` z inwalidacją.

### Tydzień 12 — Jakość i produkcja 🟡
- Rozdziały: [16](./16-testowanie.md) (testy), [17](./17-docker-cicd-produkcja.md) (Docker/CI/CD), [18](./18-integracja-z-frontendem-vue.md) (front), [19](./19-git-jakosc-stylecop.md) (Git/jakość).
- Cel: piszesz test handlera i walidatora; rozumiesz Docker i pipeline PR.
- Kamień milowy: dopisz 2 testy (walidator + handler) i uruchom `docker compose exec api dotnet test` na zielono.

### Po 12 tygodniach
Wracaj do tematów 🔴 (keyed services, Outbox/Saga, refresh tokens, Testcontainers, K8s) „na żądanie" — gdy pojawią
się w pracy lub na konkretnej rozmowie. Nie ucz się ich „na zapas".

---

## Zbiorczy indeks ćwiczeń

Każdy rozdział ma ćwiczenia (🟢→🔴) na końcu. Rekomendowana ścieżka „minimum, które robi różnicę":

| Rozdział | Kluczowe ćwiczenie (zrób koniecznie) |
|----------|--------------------------------------|
| [01](./01-dotnet-i-csharp-od-zera.md) | LINQ na papierze + rozbiór encji `Lesson` |
| [02](./02-http-rest-i-jak-dziala-web-api.md) | Cały cykl: login → token → `GET /me` w Swaggerze |
| [03](./03-pierwsza-aplikacja-host-program-cs.md) | Rozgraniczyć fazę rejestracji od pipeline w `Program.cs` |
| [04](./04-middleware-filtry-atrybuty.md) | Zaprojektować `RequestTimingMiddleware` |
| [05](./05-kontrolery-minimal-api-mvc-blazor.md) | Mapa endpointów z 3 kontrolerów + wskazać brak logiki |
| [06](./06-ef-core-fundamenty.md) | Nowe pole + migracja + `database update` |
| [07](./07-ef-core-zapytania-i-wydajnosc.md) | Napraw N+1 (Include vs Select) |
| [08](./08-dependency-injection.md) | Diagnoza buga captive dependency |
| [09](./09-architektura-clean-cqrs-mediatr.md) | Nowy przypadek użycia (command+handler+walidator) |
| [10](./10-auth-identity-jwt.md) | Resource-based: 403 bez enrollmentu, 200 po zapisie |
| [11](./11-logowanie-serilog.md) | Dodać strukturalny log do handlera |
| [12](./12-cache-memory-redis.md) | Memory Cache dla kategorii + inwalidacja |
| [13](./13-wyszukiwanie-i-elasticsearch.md) | Test case-insensitivity na danych (`vue` vs `VUE`) |
| [14](./14-komunikacja-graphql-grpc-signalr.md) | Dobór narzędzia do 4 scenariuszy |
| [15](./15-zadania-w-tle-i-brokery.md) | `BackgroundService` z ręcznym scope |
| [16](./16-testowanie.md) | Dopisać test walidatora + handlera |
| [17](./17-docker-cicd-produkcja.md) | Szkic pipeline GitHub Actions (build+test) |
| [18](./18-integracja-z-frontendem-vue.md) | Prześledzić żądanie end-to-end w DevTools |
| [19](./19-git-jakosc-stylecop.md) | Feature branch + PR (symulacja) + `.editorconfig` |

---

## Checklist juniora .NET (gotów na rozmowę)

Po kursie powinieneś umieć **wytłumaczyć** (nie tylko rozpoznać):

**Fundamenty**
- [ ] Różnica .NET / C# / CLR / BCL
- [ ] `record` vs `class`; kiedy który
- [ ] `async/await`, `Task`, po co `CancellationToken`
- [ ] Metody HTTP i kody statusu; **401 vs 403**
- [ ] Cykl `Program.cs`: rejestracja vs pipeline; co robi Kestrel
- [ ] Hierarchia konfiguracji (appsettings/env/secrets); Options pattern

**Serce frameworka**
- [ ] Pipeline middleware „cebula" i **dlaczego auth przed authorization**
- [ ] Cienki kontroler; dlaczego logika w handlerach
- [ ] Middleware vs filtr (z grubsza)

**Dane**
- [ ] Code First + migracje; co commitujesz
- [ ] `DbContext`/`DbSet`; dlaczego Scoped
- [ ] Include vs Select; **problem N+1** i naprawa
- [ ] `AsNoTracking`; Change Tracker (dlaczego zmiana + SaveChanges = UPDATE)
- [ ] Unikalny constraint jako reguła biznesowa; obsługa `DbUpdateException`

**Architektura i DI**
- [ ] 4 warstwy Clean Architecture i reguła zależności
- [ ] Inwersja zależności na `IApplicationDbContext`
- [ ] Transient/Scoped/Singleton + captive dependency
- [ ] CQRS; po co MediatR; pipeline behaviors (walidacja/trim)
- [ ] Manual mapping vs AutoMapper/Mapperly (+ licencje)

**Bezpieczeństwo**
- [ ] JWT flow od loginu do `[Authorize]`; co gwarantuje podpis
- [ ] **Resource-based authorization** (dlaczego rola nie wystarcza)
- [ ] CORS (dlaczego front go potrzebuje); rate limiting

**Jakość i produkcja**
- [ ] Piramida testów; unit (handler/walidator) vs integracyjny (WebApplicationFactory)
- [ ] FluentAssertions vs Shouldly (+ kwestia licencji FA 8)
- [ ] Docker: obraz vs kontener; multi-stage; host `db` w Compose
- [ ] CI/CD: co łapie PR check
- [ ] Git: feature branch, PR, `.gitignore`, EditorConfig/analyzery

**Świadomość (znać nazwy i „kiedy")**
- [ ] Memory Cache vs Redis; cache invalidation
- [ ] LIKE/FTS vs Elasticsearch; kto jest źródłem prawdy
- [ ] REST vs GraphQL vs gRPC; SignalR vs polling
- [ ] BackgroundService vs Hangfire; broker (RabbitMQ/MassTransit); Outbox
- [ ] 🔴 refresh tokens/cookies, keyed services, Testcontainers, Kubernetes

---

## Słownik

| Termin | Definicja |
|--------|-----------|
| **.NET** | Platforma: runtime (CLR) + biblioteki (BCL) |
| **CLR** | Silnik uruchomieniowy .NET (pamięć, GC, wykonanie IL) |
| **ASP.NET Core** | Framework webowy Microsoftu na .NET |
| **Kestrel** | Wbudowany serwer HTTP .NET |
| **Middleware** | Komponent pipeline HTTP (opakowuje żądanie) |
| **Endpoint** | URL + metoda HTTP obsługiwane przez aplikację |
| **DI** | Dependency Injection — wstrzykiwanie zależności |
| **Lifetime** | Czas życia usługi DI: Transient/Scoped/Singleton |
| **ORM** | Mapowanie obiekt ↔ tabela SQL |
| **EF Core** | ORM Microsoftu dla .NET |
| **DbContext** | „Sesja" EF z bazą (śledzenie zmian, transakcja) |
| **DbSet<T>** | Reprezentacja tabeli w EF |
| **Migration** | Wersjonowana zmiana schematu bazy |
| **Change Tracker** | Mechanizm EF śledzący zmiany encji |
| **N+1** | Antywzorzec: 1 zapytanie o listę + N o szczegóły |
| **CQRS** | Rozdzielenie zapisu (Command) i odczytu (Query) |
| **MediatR** | Biblioteka realizująca wzorzec mediatora (CQRS) |
| **Pipeline behavior** | „Middleware" dla komend/zapytań w MediatR |
| **DTO** | Obiekt transferowy przez API (nie encja) |
| **Clean Architecture** | Warstwy z zależnościami skierowanymi do Domain |
| **JWT** | Podpisany token z claimami (tożsamość) |
| **Claim** | Para klucz-wartość w tokenie |
| **Authentication** | „Kim jesteś" (401 przy braku) |
| **Authorization** | „Czy wolno ci to" (403 przy braku) |
| **Resource-based authz** | Sprawdzenie dostępu do konkretnego zasobu |
| **CORS** | Zgoda przeglądarki na żądania cross-origin |
| **Rate limiting** | Ograniczenie liczby żądań (429 po przekroczeniu) |
| **Memory Cache** | Cache w pamięci procesu (1 instancja) |
| **Redis** | Baza klucz-wartość w RAM; cache rozproszony |
| **Elasticsearch** | Silnik wyszukiwania pełnotekstowego |
| **GraphQL** | Język zapytań, jeden endpoint, klient określa kształt |
| **Resolver** | Funkcja zwracająca pole w GraphQL |
| **gRPC** | RPC binarny po HTTP/2 (serwis↔serwis) |
| **SignalR** | Real-time push nad WebSocket |
| **BackgroundService** | Zadanie działające w tle aplikacji |
| **Hangfire** | Kolejka i harmonogram jobów z dashboardem |
| **Broker** | Pośrednik wiadomości (RabbitMQ, Kafka) |
| **MassTransit** | Abstrakcja .NET nad brokerem |
| **Outbox** | Wzorzec niezawodnej publikacji eventów z transakcją |
| **WebApplicationFactory** | Uruchomienie aplikacji in-memory do testów |
| **Testcontainers** | Prawdziwa baza w Dockerze na czas testu |
| **Docker image/container** | Szablon / działająca instancja szablonu |
| **CI/CD** | Automatyczny build/test/deploy |

---

## Zamiast zakończenia

Kurs jest mapą — **projekt Course Platform jest laboratorium**. Najwięcej nauczysz się, dłubiąc w prawdziwym
kodzie: dodając pole i migrację, pisząc handler, łapiąc własny błąd 403, dopisując test. Roadmapa
([roadmap.sh/aspnet-core](https://roadmap.sh/aspnet-core)) to teren, który poznajesz latami — nie musisz znać
wszystkiego naraz. Wystarczy solidny rdzeń (EF, DI, architektura/CQRS, auth, testy) + świadomość reszty. Tyle
wystarczy, żeby dobrze, profesjonalnie i pewnie wejść w ASP.NET Core i znaleźć pracę.

Powodzenia. 🚀

⬅️ Wróć do: **[00 — Jak korzystać z kursu i mapa](./00-jak-korzystac-z-kursu-i-mapa.md)**

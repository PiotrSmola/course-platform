# 00 — Jak korzystać z tego kursu + mapa

> **Poziom:** 🟢 dla każdego · **Czas:** ~20 min · **Wymaga:** nic

To jest **kurs ASP.NET Core prowadzący za rękę** — od „nie wiem, co to .NET" do poziomu **juniora z 1–3 latami
doświadczenia**, który realnie ogarnia framework pod pracę. Nie jest to encyklopedia ani ściąga: rozdziały mają
**rosnącą trudność**, każdy tłumaczy pojęcia od intuicji, a potem pogłębia.

Towarzyszem kursu jest realny projekt **Course Platform** (to repozytorium): fullstack platforma kursów online,
backend **ASP.NET Core 9** + frontend **Vue 3**. Za każdym razem, gdy poznasz teorię, zobaczysz ją w prawdziwym
kodzie — nie w wymyślonym „hello world".

---

## Dla kogo jest ten kurs

Zakładam, że **umiesz już programować** i znasz podstawy webu. Konkretnie, że:

- ogarniasz **bazy danych** i SQL (choćby PostgreSQL/MySQL),
- używałeś **Gita** (GitHub/GitLab),
- znasz **front**: HTML/CSS/JS/TS i jakiś framework (np. Vue),
- widziałeś **jakiś inny język backendowy** (Java, Python, PHP/Laravel — cokolwiek).

Czego **nie** zakładam: że znasz C#, .NET, ani ASP.NET Core. Od tego zaczynamy w rozdziale 01, spokojnie i z
mostkami „w JS/Laravel robisz X → w C# robisz Y".

> Jeśli jesteś kompletnie zielony w programowaniu — ten kurs będzie miejscami za szybki. Ale spróbuj; rozdziały
> 00–05 są celowo łagodne.

---

## Krzywa trudności (jak czytać oznaczenia)

Każdy rozdział ma w nagłówku **poziom**:

| Znak | Poziom | Co to znaczy |
|------|--------|--------------|
| 🟢 | laik → podstawy | Wchodzisz w temat od zera. Nie trzeba nic wcześniej wiedzieć poza poprzednimi 🟢. |
| 🟡 | rdzeń juniora | To musisz umieć na rozmowę i do pracy. Serce kursu. |
| 🔴 | junior+ / „awans" | Tematy, które robią różnicę na mid/senior. Oznaczam je osobno — **możesz je pominąć za pierwszym razem** i wrócić później. Nie blokują fundamentu. |

Globalna mapa poziomów:

```
00 ──🟢── 05      Fundamenty: C#, HTTP, pierwsza aplikacja, middleware, kontrolery
06 ──🟡── 11      Rdzeń juniora: EF Core, DI, architektura/CQRS, auth, logi
12 ──🟡/🔴── 19   Rozszerzenia pod pracę: cache, search, komunikacja, tło, testy, Docker, front, Git/jakość
20               Spięcie: plan nauki, indeks ćwiczeń, checklist, słownik
```

Nie przeskakuj rozdziałów 🟡 — one się na sobie opierają. Rozdziały 🔴 i „panorama" (GraphQL, Hangfire, Kafka,
Kubernetes…) możesz przelecieć pobieżnie i wrócić, gdy pojawią się w pracy — część z nich (Redis, Elasticsearch,
SignalR, MinIO) **już działa w Course Platform**, ale nie musisz od razu zgłębiać każdego detalu implementacji.

---

## Jak zbudowany jest każdy rozdział

Każdy plik trzyma ten sam rytm, żebyś wiedział, czego się spodziewać:

1. **Nagłówek** — poziom, szacowany czas, czego wymaga.
2. **„Po co ci to"** — motywacja + gdzie to widać w Course Platform / w pracy.
3. **Mostek z tego, co już znasz** — analogia do JS/TS/Vue/Laravel/Java/Python.
4. **Teoria stopniowana** — od intuicji do szczegółu.
5. **Przykład z Course Platform** — realny plik i ścieżka.
6. **Pułapki** — gdzie junior najczęściej się wykłada.
7. **Ćwiczenia** — zadania do zrobienia na projekcie (🟢→🔴).
8. **Pytania kontrolne** + **rozwiązania** (zwijane — najpierw pomyśl sam!).
9. **Idź dalej** — link do następnego rozdziału.

> **Ćwiczenia i pytania to nie ozdoba.** Nauka backendu jest przez palce. Przeczytanie rozdziału bez zrobienia
> ćwiczenia to jak oglądanie filmu o pływaniu.

---

## Setup środowiska (zrób raz, na starcie)

Course Platform jest w pełni skonteneryzowany — **nie instalujesz .NET ani Node na hoście**. Wszystko chodzi
w Dockerze.

```bash
# w katalogu repozytorium
cp .env.example .env          # uzupełnij sekrety (Jwt:Key min. 32 znaki itp.)
docker compose up -d          # wstaje pełny stack (db, api, frontend, redis, ES, MinIO…)
```

Co dostajesz (rdzeń dev):

| Kontener | Usługa (Compose) | Port (host) | Co to |
|----------|------------------|-------------|-------|
| `cp_api` | `api` | **8080** | Backend ASP.NET Core 9 (`dotnet watch run` — auto-reload) |
| `cp_frontend` | `frontend` | **5173** | Frontend Vue 3 (Vite, hot reload) |
| `cp_db` | `db` | **5432** | PostgreSQL 16 |
| `cp_redis` | `redis` | **6379** | Redis 7 (HybridCache / distributed cache) |
| `cp_elasticsearch` | `elasticsearch` | **9200** | Elasticsearch 8 (wyszukiwanie kursów, gdy `Elastic:Enabled`) |
| `cp_minio` | `minio` | **9000** / **9001** | MinIO (S3-compatible storage: wideo, miniatury, PDF certyfikatów) |
| `cp_minio_init` | `minio_init` | — | Jednorazowa inicjalizacja bucketu MinIO |
| `cp_mailhog` | `mailhog` | **8025** | Przechwytywanie maili dev (UI w przeglądarce) |
| `cp_aspire` | `aspire-dashboard` | **18888** / **18889** | OpenTelemetry / Aspire Dashboard (logi, trace) |
| `cp_stripe_cli` | `stripe_cli` | — | Opcjonalny profil `stripe` — forward webhooków Stripe do API |

Komendy .NET/EF/npm odpalasz **wewnątrz kontenerów**:

```bash
docker compose exec api bash          # wejście do backendu
docker compose exec api dotnet build  # jednorazowa komenda
docker compose exec frontend npm run build
```

> **Ważne:** z wnętrza kontenera `api` host bazy to **`db`** (nazwa usługi Compose), **nie** `localhost`.
> Swagger (dokumentacja API) w dev: `http://localhost:8080/swagger`.

Do włączenia danych demo (przykładowe kursy, użytkownicy) ustaw `Dev:Seed=true` w konfiguracji dev i zrestartuj
`api`. Loginy testowe znajdziesz w seedzie (`ApplicationDbContextSeed.cs`) — np. `admin@courseplatform.com` /
`Admin123!`.

---

## Mapa: 62 rekomendowane tematy z roadmap.sh → gdzie w kursie

Zakres kursu = **tematy oznaczone jako rekomendowane (niebieskie/„Personal Recommendation")** na
[roadmap.sh/aspnet-core](https://roadmap.sh/aspnet-core). To świadomy wybór: uczysz się tego, co realnie
przydaje się w pracy w .NET, a nie niszy używanej w promilu projektów. Egzotykę (Cosmos, Cassandra, Sphinx,
Dapr…) traktuję jednozdaniową wzmianką.

| # | Temat (rekomendowany) | Rozdział | W projekcie? |
|---|------------------------|----------|--------------|
| 1 | C# | 01 | ✅ encje, handlery |
| 2 | .NET | 00, 01, 03 | ✅ .NET 9 |
| 3 | .NET CLI | 03 | ✅ (przez Docker) |
| 4 | HTTP / HTTPS | 02 | ✅ REST |
| 5 | Data Structures & Algorithms | 06, 20 | ✅ HashSet w statystykach |
| 6 | Database Fundamentals | 06 | ✅ relacje |
| 7 | SQL Basics | 06 | (znasz) |
| 8 | Database Design Basics | 06 | ✅ Fluent API |
| 9 | Stored Procedures | 07 | — (FromSqlRaw) |
| 10 | Constraints | 06 | ✅ unique index |
| 11 | Git - Version Control | 19 | ✅ repo |
| 12 | GitHub/GitLab/BitBucket | 19 | ✅ |
| 13 | MVC | 05 | — (API, nie MVC) |
| 14 | REST | 02 | ✅ |
| 15 | Middlewares | 04 | ✅ |
| 16 | Filters & Attributes | 04 | ✅ `[Authorize]` |
| 17 | App Settings & Configs | 03 | ✅ Jwt, .env |
| 18 | StyleCop Rules | 19 | — (analyzers) |
| 19 | Minimal APIs | 05 | — (kontrolery) |
| 20 | Entity Framework Core | 06 | ✅ |
| 21 | Framework Basics | 06 | ✅ |
| 22 | Code First + Migrations | 06 | ✅ |
| 23 | Lazy/Eager/Explicit Loading | 07 | ✅ Include |
| 24 | Change Tracker API | 07 | ✅ |
| 25 | DI — Life Cycles | 08 | ✅ |
| 26 | DI Containers | 08 | ✅ built-in |
| 27 | Microsoft.Extensions | 08 | ✅ |
| 28 | Scoped/Transient/Singleton | 08 | ✅ |
| 29 | Memory Cache | 12 | ✅ HybridCache (`IAppCache`) |
| 30 | Distributed Cache | 12 | ✅ HybridCache + Redis |
| 31 | Redis | 12 | ✅ (`cp_redis`) |
| 32 | Elasticsearch | 13 | ✅ dual-mode (`Elastic:Enabled` → ES, fallback `LIKE`) |
| 33 | SQL Server | 07 | — (PostgreSQL) |
| 34 | Relational | 06, 07 | ✅ |
| 35 | DynamoDB | 06 | — (wzmianka) |
| 36 | MongoDB | 06 | — (wzmianka) |
| 37 | Serilog | 11 | ✅ |
| 38 | Gridify | 14 | — |
| 39 | HotChocolate | 14 | — |
| 40 | SignalR Core | 14 | ✅ `NotificationHub` + `useRealtime` |
| 41 | Manual Mapping | 09 | ✅ `Select()` |
| 42 | Native Background Service | 15 | ✅ EmailDispatcher, RefreshTokenCleanup, StaleMultipartUploadCleanup |
| 43 | Hangfire | 15 | — |
| 44 | xUnit | 16 | ✅ |
| 45 | Shouldly | 16 | ❌ (projekt: FluentAssertions 8.0.0) |
| 46 | Moq | 16 | ✅ w testach |
| 47 | AutoFixture | 16 | — |
| 48 | WebApplicationFactory | 16 | ✅ |
| 49 | Testcontainers | 16 | wzmianka |
| 50 | Playwright | 16 | ✅ smoke E2E w `frontend/` (Vitest + Playwright) |
| 51 | SpecFlow | 16 | — |
| 52 | RabbitMQ | 15 | — |
| 53 | MassTransit | 15 | — |
| 54 | Ocelot | 15 | — |
| 55 | YARP | 15 | — |
| 56 | Docker | 17 | ✅ |
| 57 | Kubernetes | 17 | panorama |
| 58 | GitHub Actions | 17 | panorama |
| 59 | MediatR | 09 | ✅ |
| 60 | FluentValidation | 09 | ✅ |
| 61 | Scalar | 17 | — (Swagger) |
| 62 | Blazor | 05 | — (Vue) |

> **O legendzie roadmap.sh:** pozycje „Personal Recommendation / Opinion" są na interaktywnej mapie
> **wyróżnione** (kolor zależny od wersji renderowania mapy — zwykle fioletowy/purpurowy). PDF eksportu
> nie pokazuje kolorów — lista jest płaska. Główna ścieżka (węzły-kontenery jak „Caching") mówi „wejdź w temat",
> a konkretne narzędzie wybierasz z rekomendacji. *Alternative* i *Optional* pomijamy na start.

---

## Korekty „na teraz" (roadmapa vs rynek)

| Temat | Na mapie | Stan realny | Co robić |
|-------|----------|-------------|----------|
| **FluentAssertions** | usunięte (2025, licencja komercyjna) | projekt wciąż pinuje FA **8.0.0** | Ucz się **Shouldly** (darmowe); rozważ migrację testów |
| **MediatR / AutoMapper** | rekomendowane | od 2025 **licencja komercyjna** (LuckyPennySoftware) | Znaj, ale świadomie; alternatywy: własny mediator, Wolverine, Mapperly |
| **Swagger** | standard | wciąż OK; **Scalar** rośnie | Projekt: Swagger; nowe projekty: rozważ Scalar |
| **PostgreSQL** | *alternative* na mapie ASP.NET | dominuje w nowych projektach | Course Platform używa go słusznie; EF przenosi się 1:1 na SQL Server |
| **NEST (Elasticsearch)** | — | wycofany | Nowy klient: `Elastic.Clients.Elasticsearch` |

---

## Idź dalej

➡️ **[01 — .NET i C# od zera](./01-dotnet-i-csharp-od-zera.md)** — poznajemy język i platformę, na której stoi
całe ASP.NET Core.

Materiały referencyjne (oficjalne, gdy chcesz pogłębić dowolny temat):
- [Microsoft Learn — ASP.NET Core](https://learn.microsoft.com/aspnet/core)
- [EF Core](https://learn.microsoft.com/ef/core)
- [roadmap.sh — ASP.NET Core](https://roadmap.sh/aspnet-core)

# Course Platform — Plan wdrożenia

Kompletny plan budowy platformy kursów online w stacku **Vue 3 + ASP.NET Core 9**. Dokument przeznaczony do przekazania do Claude Code jako mapa całego projektu.

---

## 0. Status środowiska — PRZECZYTAJ NAJPIERW

**Środowisko jest już postawione. Szkielet projektu istnieje. NIE twórz go od nowa.**

### Co już istnieje
- Solucja `CoursePlatform.sln` i cztery projekty Clean Architecture (`Domain`, `Application`, `Infrastructure`, `API`) w `src/` — utworzone przez `dotnet new`, z ustawionymi referencjami między projektami.
- Projekty testowe w `tests/` (`CoursePlatform.Application.UnitTests`, `CoursePlatform.IntegrationTests`).
- Projekt frontendu w `frontend/` — Vue 3 + TS przez Vite, z zainstalowanymi paczkami (`vue-router`, `pinia`, `@tanstack/vue-query`, `axios`, `vee-validate`, `zod`, `@vueuse/core`, `vue3-toastify`, `sass`).
- Pełna konfiguracja Docker: `Dockerfile.api.dev`, `frontend/Dockerfile.dev`, `docker-compose.yml`, `.gitignore`, `.dockerignore`.

### Czego NIE robić
- Nie uruchamiaj `dotnet new sln`, `dotnet new`, `npm create vite` ani niczego, co tworzy szkielet od zera.
- Nie nadpisuj istniejącej konfiguracji Docker bez wyraźnej prośby.
- Zadanie agenta to **dopełnianie** struktury (encje, handlery, komponenty), nie scaffolding.

### Jak uruchamiać komendy — WSZYSTKO przez Docker
Build, EF Core, npm — odpalane **wewnątrz kontenerów**, nigdy bezpośrednio na hoście:

```bash
docker compose exec api bash       # wejście do backendu (.NET 9 SDK + dotnet-ef)
docker compose exec frontend bash  # wejście do frontendu (Node 20)
```

Lub jednorazowo, bez wchodzenia do shella:
```bash
docker compose exec api dotnet build
docker compose exec frontend npm run build
```

### Konkrety środowiska
- **Nazwa projektu Compose:** `course-platform`
- **Kontenery:** `cp_api` (backend), `cp_frontend` (frontend), `cp_db` (postgres)
- **Porty (host):** API `8080`, frontend `5173`, postgres `5432`
- **Connection string (z wnętrza kontenera api):** host to `db`, NIE `localhost`:
  ```
  Host=db;Port=5432;Database=courseplatform;Username=postgres;Password=postgres
  ```
- **Komendy EF Core** (z wnętrza `cp_api`, z katalogu `/src`):
  ```bash
  dotnet ef migrations add <Name> --project src/CoursePlatform.Infrastructure --startup-project src/CoursePlatform.API
  dotnet ef database update --project src/CoursePlatform.Infrastructure --startup-project src/CoursePlatform.API
  ```
- Backend startuje przez `dotnet watch run` (auto-reload), frontend przez `npm run dev -- --host 0.0.0.0` (hot reload). Oba skonfigurowane w `docker-compose.yml`.

### Stan implementacji (aktualny)

**Etapy 1–3 z planu są zaimplementowane.** Aplikacja to działające MVP+ portfolio, nie pusty szkielet.

| Obszar | Status |
|--------|--------|
| Domain + CQRS (MediatR) + FluentValidation | Gotowe |
| Auth JWT + refresh tokens + Identity roles | Gotowe |
| Forgot/reset password + email confirmation | Gotowe |
| Catalog, enrollment, progress, reviews (CRUD własnej opinii) | Gotowe |
| Instructor CMS (modules/lessons, publish/hide/delete) | Gotowe |
| Admin (users, roles, courses, reviews, audit, reindex) | Gotowe |
| MinIO presigned uploads (thumbnail + multipart video) | Gotowe |
| Stripe Checkout + webhook (idempotencja) | Gotowe |
| Certyfikaty PDF, SignalR, Redis cache, Elasticsearch | Gotowe |
| Authorization policies (`AdminOnly`, `InstructorOrAdmin`, `ManageCourse`) | Gotowe |
| Frontend: katalog, learning player, checkout, panele | Gotowe |
| Strony błędów 403 / 404 / 500 / 501 + catch-all | Gotowe |
| Testy: unit (Application/Infrastructure), integration, Vitest, Playwright smoke | Gotowe |

**Nie scaffolduj od zera.** Rozszerzaj istniejący kod. Nowe feature’y według konwencji CQRS / feature-based Vue.

---

## 1. Przegląd projektu

Platforma kursów online umożliwiająca instruktorom tworzenie i sprzedaż kursów, a studentom ich zakup i odtwarzanie. Projekt portfolio demonstrujący kompetencje fullstack: Clean Architecture, CQRS, resource-based authorization, file storage poza bazą oraz nowoczesny frontend SPA.

### Stack technologiczny

**Backend**
- ASP.NET Core 9
- Entity Framework Core 9 (Code First + Migrations)
- PostgreSQL 16
- ASP.NET Identity + JWT
- Clean Architecture (API / Application / Domain / Infrastructure)
- MediatR (CQRS), FluentValidation, Serilog

**Frontend**
- Vue 3 (Composition API + `<script setup>`)
- TypeScript
- Vite
- Pinia (client state), TanStack Vue Query (server state)
- Vue Router, Axios
- VeeValidate + Zod
- SCSS (architektura 7-1)

**Infrastruktura**
- Docker Compose
- MinIO (storage wideo)
- Redis (cache — opcjonalnie)
- nginx (serwowanie frontendu w produkcji)

### Kluczowe mechanizmy wyróżniające projekt

1. **Resource-based authorization** — dostęp do lekcji tylko po enrollment na dany kurs (nie tylko "czy zalogowany", ale "czy ten user ma dostęp do tego zasobu").
2. **File storage poza bazą** — MinIO + presigned URLs zamiast publicznych linków.
3. **Progress tracking** — relacja user↔lesson ze stanem ukończenia.

---

## 2. Zakres funkcjonalności

### Student
- Rejestracja i logowanie (JWT)
- Przeglądanie katalogu kursów z filtrowaniem i paginacją
- Podgląd strony kursu (opis, program, recenzje) przed zakupem
- Zakup / enrollment
- Dostęp do lekcji wyłącznie po enrollment
- Odtwarzanie wideo
- Oznaczanie lekcji jako ukończonej
- Śledzenie postępu (% kursu)
- Wystawianie recenzji po zakupie

### Instructor
- Tworzenie i edycja kursów
- Budowanie struktury Module → Lesson
- Upload materiałów wideo
- Publikowanie / ukrywanie kursu
- Podgląd swoich kursów i liczby zapisanych studentów

### Admin
- Zarządzanie użytkownikami
- Moderacja kursów i recenzji
- Nadawanie roli instructor

---

## 3. Kolejność budowy (etapy)

Etapy są obowiązkowe — różnica między skończonym a porzuconym projektem.

### Etap 1 — domena bez plików
Encje, relacje, auth, enrollment, progress, recenzje, resource authorization. Wideo jako zwykły string URL (placeholder). To 70% wartości projektu.

### Etap 2 — storage
MinIO, upload materiałów, presigned URLs zamiast placeholderów.

### Etap 3 — płatności
**Decyzja: Stripe w test mode** (nie mock). Checkout po stronie Stripe, webhook potwierdzający płatność tworzy enrollment.

---

## 4. Architektura backendu — Clean Architecture

Cztery projekty w solucji. Zależności kierowane do środka: API → Application → Domain; Infrastructure → Application/Domain. Domain nie zależy od niczego.

```
CoursePlatform.sln
│
├── src/
│   ├── CoursePlatform.Domain/
│   │   ├── Entities/
│   │   │   ├── Course.cs
│   │   │   ├── Module.cs
│   │   │   ├── Lesson.cs
│   │   │   ├── Enrollment.cs
│   │   │   ├── LessonProgress.cs
│   │   │   ├── Review.cs
│   │   │   └── ApplicationUser.cs
│   │   ├── Enums/
│   │   │   ├── CourseLevel.cs
│   │   │   └── CourseStatus.cs
│   │   └── Common/
│   │       └── BaseEntity.cs
│   │
│   ├── CoursePlatform.Application/
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IApplicationDbContext.cs
│   │   │   │   ├── ICurrentUserService.cs
│   │   │   │   └── IFileStorageService.cs
│   │   │   ├── Behaviours/
│   │   │   │   └── ValidationBehaviour.cs
│   │   │   └── Exceptions/
│   │   │       ├── NotFoundException.cs
│   │   │       └── ForbiddenAccessException.cs
│   │   ├── Courses/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateCourse/
│   │   │   │   │   ├── CreateCourseCommand.cs
│   │   │   │   │   ├── CreateCourseCommandHandler.cs
│   │   │   │   │   └── CreateCourseCommandValidator.cs
│   │   │   │   └── UpdateCourse/
│   │   │   └── Queries/
│   │   │       ├── GetCourses/
│   │   │       └── GetCourseDetails/
│   │   ├── Enrollments/
│   │   ├── Lessons/
│   │   └── DependencyInjection.cs
│   │
│   ├── CoursePlatform.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── CourseConfiguration.cs
│   │   │   │   ├── ModuleConfiguration.cs
│   │   │   │   └── LessonConfiguration.cs
│   │   │   ├── Migrations/
│   │   │   └── ApplicationDbContextSeed.cs
│   │   ├── Services/
│   │   │   ├── CurrentUserService.cs
│   │   │   ├── MinioFileStorageService.cs
│   │   │   └── DateTimeService.cs
│   │   ├── Identity/
│   │   │   └── JwtTokenGenerator.cs
│   │   └── DependencyInjection.cs
│   │
│   └── CoursePlatform.API/
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── CoursesController.cs
│       │   ├── EnrollmentsController.cs
│       │   ├── LessonsController.cs
│       │   └── ReviewsController.cs
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs
│       ├── Program.cs
│       ├── appsettings.json
│       └── appsettings.Development.json
│
├── tests/
│   ├── CoursePlatform.Application.UnitTests/
│   └── CoursePlatform.IntegrationTests/
│
├── frontend/
├── docker-compose.yml
├── docker-compose.override.yml
├── Dockerfile.api
└── .dockerignore
```

### Odpowiedzialność warstw

- **Domain** — encje i logika biznesowa, zero zależności od EF czy ASP.NET. Czyste C#.
- **Application** — przypadki użycia jako CQRS (Commands/Queries przez MediatR), interfejsy do rzeczy zewnętrznych, walidacja. Definiuje *czego potrzebuje*, nie *jak to zrobione*.
- **Infrastructure** — konkretne implementacje: EF Core DbContext, MinIO storage, generowanie JWT.
- **API** — kontrolery (cienkie, delegują do MediatR), middleware, konfiguracja w Program.cs.

### Flow jednej operacji
`Controller` odbiera request → wysyła `Command/Query` przez MediatR → `Handler` w Application robi robotę przez interfejsy → Infrastructure dostarcza implementacje → wynik wraca do kontrolera.

---

## 5. Model bazy danych

```
ApplicationUser (Identity)
   │
   ├──< Course (Instructor)         instructor tworzy kursy
   ├──< Enrollment                  student zapisany na kursy
   ├──< Review
   └──< LessonProgress

Course
   ├── InstructorId (FK)
   ├── Title, Description, Price, Level, Status, ThumbnailUrl
   └──< Module
          └──< Lesson (VideoUrl, Duration, Order)

Enrollment
   ├── UserId (FK), CourseId (FK)
   ├── EnrolledAt
   └── unique(UserId, CourseId)     nie można zapisać się dwa razy

LessonProgress
   ├── UserId (FK), LessonId (FK)
   ├── IsCompleted, CompletedAt
   └── unique(UserId, LessonId)

Review
   ├── UserId (FK), CourseId (FK)
   ├── Rating (1-5), Comment
   └── unique(UserId, CourseId)     jedna recenzja na kurs
```

Constrainty `unique` to logika biznesowa wyrażona w bazie — konfigurowane przez Fluent API w klasach `Configurations/`.

---

## 6. Biblioteki backendu (NuGet)

### Application
- `MediatR` — CQRS, handlery komend/zapytań
- `FluentValidation.DependencyInjectionExtensions` — walidacja

### Infrastructure
- `Microsoft.EntityFrameworkCore` + `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Minio` — klient MinIO (presigned URLs, upload)

### API
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Serilog.AspNetCore`
- `Swashbuckle.AspNetCore` — Swagger/OpenAPI
- `Microsoft.EntityFrameworkCore.Design` — tooling migrations

### Testy
- `xUnit`, `Moq`, `FluentAssertions`
- `Microsoft.AspNetCore.Mvc.Testing` — testy integracyjne
- `Testcontainers` — realna baza w testach

Mapowanie encja↔DTO: ręczne na start (mniej magii niż AutoMapper).

---

## 7. Architektura frontendu

Fundament: Vue 3 (Composition API + `<script setup>`) + TypeScript + Vite.

### Podział odpowiedzialności state
- **Pinia** trzyma **client state** (token, zalogowany user, UI state)
- **TanStack Vue Query** trzyma **server state** (kursy, lekcje, enrollmenty — wszystko z API)

Nie cache'uje się ręcznie list kursów w storze.

### Struktura plików (feature-based)

```
frontend/
├── src/
│   ├── main.ts
│   ├── App.vue
│   │
│   ├── app/
│   │   ├── router/
│   │   │   ├── index.ts
│   │   │   └── guards.ts
│   │   ├── plugins/
│   │   │   ├── vue-query.ts
│   │   │   └── axios.ts
│   │   └── layouts/
│   │       ├── DefaultLayout.vue
│   │       ├── DashboardLayout.vue
│   │       └── AuthLayout.vue
│   │
│   ├── shared/
│   │   ├── api/
│   │   │   └── client.ts
│   │   ├── components/
│   │   │   ├── ui/
│   │   │   │   ├── BaseButton.vue
│   │   │   │   ├── BaseInput.vue
│   │   │   │   ├── BaseModal.vue
│   │   │   │   ├── BaseCard.vue
│   │   │   │   └── BaseSpinner.vue
│   │   │   └── layout/
│   │   │       ├── AppHeader.vue
│   │   │       ├── AppSidebar.vue
│   │   │       └── AppFooter.vue
│   │   ├── composables/
│   │   │   ├── usePagination.ts
│   │   │   └── useDebounce.ts
│   │   └── types/
│   │       └── api.ts
│   │
│   ├── features/
│   │   ├── auth/
│   │   │   ├── api/
│   │   │   │   └── auth.api.ts
│   │   │   ├── components/
│   │   │   │   ├── LoginForm.vue
│   │   │   │   └── RegisterForm.vue
│   │   │   ├── composables/
│   │   │   │   └── useAuth.ts
│   │   │   ├── stores/
│   │   │   │   └── auth.store.ts
│   │   │   ├── pages/
│   │   │   │   ├── LoginPage.vue
│   │   │   │   └── RegisterPage.vue
│   │   │   └── schemas/
│   │   │       └── auth.schema.ts
│   │   │
│   │   ├── courses/
│   │   │   ├── api/
│   │   │   │   └── courses.api.ts
│   │   │   ├── components/
│   │   │   │   ├── CourseCard.vue
│   │   │   │   ├── CourseList.vue
│   │   │   │   ├── CourseFilters.vue
│   │   │   │   └── CourseReviews.vue
│   │   │   ├── composables/
│   │   │   │   ├── useCourses.ts
│   │   │   │   └── useCourseDetails.ts
│   │   │   ├── pages/
│   │   │   │   ├── CourseCatalogPage.vue
│   │   │   │   └── CourseDetailsPage.vue
│   │   │   └── types/
│   │   │       └── course.types.ts
│   │   │
│   │   ├── learning/
│   │   │   ├── api/
│   │   │   │   └── learning.api.ts
│   │   │   ├── components/
│   │   │   │   ├── LessonPlayer.vue
│   │   │   │   ├── CourseSidebar.vue
│   │   │   │   └── ProgressBar.vue
│   │   │   ├── composables/
│   │   │   │   └── useProgress.ts
│   │   │   └── pages/
│   │   │       └── LearningPage.vue
│   │   │
│   │   ├── enrollment/
│   │   │   ├── api/
│   │   │   ├── composables/
│   │   │   └── pages/
│   │   │       └── MyCoursesPage.vue
│   │   │
│   │   └── instructor/
│   │       ├── api/
│   │       ├── components/
│   │       │   ├── CourseForm.vue
│   │       │   ├── ModuleEditor.vue
│   │       │   └── LessonUploader.vue
│   │       └── pages/
│   │           ├── InstructorDashboardPage.vue
│   │           └── CourseEditorPage.vue
│   │
│   └── assets/
│       └── styles/
│           ├── main.scss
│           ├── abstracts/
│           │   ├── _variables.scss
│           │   ├── _mixins.scss
│           │   └── _functions.scss
│           ├── base/
│           │   ├── _reset.scss
│           │   ├── _typography.scss
│           │   └── _global.scss
│           ├── components/
│           │   └── _glass.scss
│           └── themes/
│               └── _dark.scss
│
├── public/
├── index.html
├── vite.config.ts
├── tsconfig.json
├── package.json
├── Dockerfile
└── nginx.conf
```

Logika podziału: `app/` to konfiguracja aplikacji, `shared/` to rzeczy reużywalne wszędzie, `features/` to domeny — każda zamknięta, z własnym api/components/composables/pages.

### Mapa routingu

```
/                          DefaultLayout    katalog (landing)
/courses                   DefaultLayout    katalog z filtrami
/courses/:id               DefaultLayout    szczegóły kursu (publiczne)
/login                     AuthLayout       logowanie
/register                  AuthLayout       rejestracja

── wymaga zalogowania ──
/my-courses                DashboardLayout  zapisane kursy
/learn/:courseId/:lessonId DashboardLayout  odtwarzacz (gating!)

── wymaga roli instructor ──
/instructor                DashboardLayout  dashboard instruktora
/instructor/courses/new    DashboardLayout  tworzenie kursu
/instructor/courses/:id    DashboardLayout  edytor kursu
```

Trzy poziomy ochrony tras przez navigation guards: publiczne, wymagające auth, wymagające roli. Front tylko *chowa* UI — prawdziwą bramkę trzyma backend.

### Komunikacja z API
`shared/api/client.ts` — instancja axios z interceptorami. Request interceptor dokleja `Authorization: Bearer <token>`. Response interceptor łapie 401 → refresh token → powtórzenie requestu, a przy nieudanym refresh wylogowuje i przerzuca na `/login`.

Każdy feature ma `*.api.ts` z czystymi funkcjami, composables owijają je w Vue Query. Komponenty nie wołają axiosa bezpośrednio — zawsze przez composable.

---

## 8. Warstwa wizualna — SCSS + Liquid Glass

Styling oparty na SCSS (paczka `sass`), nie na frameworku utility. Architektura 7-1 (uproszczona) w `assets/styles/`.

### Dwie warstwy stylowania
- **Globalne** (`assets/styles/`) — zmienne, mixiny, reset, motyw, efekt glass jako mixin.
- **Lokalne** — każdy komponent `.vue` ma `<style lang="scss" scoped>`.

Dostęp do globalnych zmiennych i mixinów w komponentach bez ręcznego importu: konfiguracja `additionalData` w opcjach preprocessora SCSS w `vite.config.ts`.

### Liquid Glass
Efekt definiowany raz jako parametryzowany mixin w `abstracts/_mixins.scss` (poziom blura i przezroczystości jako argumenty). Bazuje na `backdrop-filter: blur()`, półprzezroczystym tle, subtelnym borderze i shadow. Działa tylko na ciemnym/kolorowym tle. Stosować na: karty kursów, sidebar odtwarzacza, modalne, header. Uwaga na wydajność — `backdrop-filter` jest kosztowny.

---

## 9. Biblioteki frontendu

- `vue`, `vue-router`, `pinia`
- `@tanstack/vue-query`
- `axios`
- `vee-validate` + `zod`
- `sass`
- `vue3-toastify` / `@vueuse/core`

---

## 10. Kontenery Docker

`docker-compose.yml` spina stack:

```
services:
  api          → ASP.NET Core (Dockerfile.api)
  db           → postgres:16
  frontend     → Vue (nginx z buildem, albo node w dev)
  minio        → MinIO (storage wideo) — od Etapu 2
  redis        → cache (opcjonalnie)
  pgadmin      → GUI do bazy (dev)
  seq          → podgląd logów Serilog (opcjonalne)
```

Na Etap 1 wystarczą trzy: `api`, `db`, `frontend`. MinIO dochodzi w Etapie 2.

### Frontend w produkcji
Multi-stage Dockerfile: stage 1 `node:20` robi `npm run build`, stage 2 `nginx:alpine` kopiuje `dist/`. `nginx.conf` z fallbackiem na `index.html` (SPA routing) i proxy `/api` na backend (uniknięcie CORS w dev). W dev alternatywnie Vite dev server z hot reload i wolumenem ze źródłami.

---

## 11. Sugerowana kolejność implementacji

### Backend
1. Szkielet solucji (4 projekty + zależności)
2. Domain (encje, enums)
3. Infrastructure: DbContext + Configurations + pierwsza migracja
4. Auth (Identity + JWT)
5. Feature Courses (CQRS: Commands + Queries)
6. Enrollment + resource authorization
7. Lessons + progress tracking
8. Reviews
9. Etap 2: MinIO + upload + presigned URLs
10. Etap 3: płatności

### Frontend
1. Auth flow (store + axios interceptory + guardy) — bez tego nie przetestujesz ról
2. Katalog kursów (lista, filtry, szczegóły — Vue Query)
3. Learning (odtwarzacz + progress)
4. Panel instruktora z uploadem (wymaga storage z Etapu 2 backendu)

---

## 12. Konwencje projektu

- Kod, nazwy plików, komentarze, commit messages — po angielsku
- Komunikacja/dokumentacja z deweloperem — po polsku
- Brak komentarzy w kodzie, chyba że wprost wymagane
- Backend: nazewnictwo C# PascalCase, async wszędzie gdzie I/O
- Frontend: `<script setup>` zawsze, composables zaczynają się od `use`
- Kontrolery cienkie — logika w handlerach MediatR
- Komponenty nie wołają axiosa bezpośrednio — przez composable + Vue Query

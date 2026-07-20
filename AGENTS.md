# AGENTS.md

Reguły dla agentów AI pracujących nad projektem **Course Platform**. Plik zgodny z konwencją `AGENTS.md` — czytany przez narzędzia agentowe (Claude Code, Cursor, Copilot i inne). Stanowi uzupełnienie `CLAUDE.md` i `IMPLEMENTATION_PLAN.md`.

---

## O projekcie

Platforma kursów online w stacku Vue 3 + ASP.NET Core 9. Fullstack portfolio project. Backend w Clean Architecture, frontend Vue 3 + TypeScript + SCSS. Pełna specyfikacja w `IMPLEMENTATION_PLAN.md`.

## Status środowiska

**Środowisko i kod aplikacji istnieją — nie scaffolduj od zera.** Solucja Clean Architecture, testy, Vue, Docker oraz pełna domena (Etapy 1–3 + extras) są na miejscu. Aktualny status: sekcja 0 w `IMPLEMENTATION_PLAN.md`.

**Wszystkie komendy (`dotnet`, `dotnet ef`, `npm`) odpalane wewnątrz kontenerów przez `docker compose exec`, nigdy na hoście.**

---

## Struktura repozytorium

```
/                       root
├── src/                backend (4 projekty Clean Architecture)
│   ├── CoursePlatform.Domain/
│   ├── CoursePlatform.Application/
│   ├── CoursePlatform.Infrastructure/
│   └── CoursePlatform.API/
├── tests/              testy backendu (unit + integration)
├── frontend/           Vue 3 + TS + SCSS
├── docker-compose.yml
└── IMPLEMENTATION_PLAN.md
```

---

## Setup środowiska

Środowisko już postawione. Start dnia:
```bash
docker compose up -d        # uruchomienie stacku (cp_api, cp_frontend, cp_db)
docker compose ps           # sprawdzenie statusu
docker compose logs -f api  # logi backendu
docker compose down         # zatrzymanie (dane bazy zostają w wolumenie pgdata)
```

Wejście do kontenerów po komendy:
```bash
docker compose exec api bash       # backend (.NET 9 SDK + dotnet-ef)
docker compose exec frontend bash  # frontend (Node 20)
```

Backend działa na `localhost:8080`, frontend na `localhost:5173`, postgres na `localhost:5432`. Z wnętrza kontenera api baza jest pod hostem `db`.

Migracje EF Core (z wnętrza `cp_api`, z `/src`):
```bash
dotnet ef migrations add <Name> --project src/CoursePlatform.Infrastructure --startup-project src/CoursePlatform.API
dotnet ef database update --project src/CoursePlatform.Infrastructure --startup-project src/CoursePlatform.API
```

---

## Komendy weryfikacyjne

Przed zgłoszeniem zmian jako gotowe, agent uruchamia (przez kontenery):

### Backend
```bash
docker compose exec api dotnet build
docker compose exec api dotnet test
```

### Frontend
```bash
docker compose exec frontend npm run type-check
docker compose exec frontend npm run lint
docker compose exec frontend npm run test
docker compose exec frontend npm run build
```

Zmiany nie są kompletne, dopóki build i testy nie przechodzą. Skrypty `type-check` i `lint` muszą istnieć w `package.json` — jeśli ich nie ma, dodaj je przy konfiguracji frontendu.

---

## Konwencje kodu

### Ogólne
- Kod, nazwy, commit messages — angielski.
- Komunikacja z deweloperem — polski, terminy techniczne po angielsku.
- Bez komentarzy w kodzie, chyba że wprost wymagane.
- Pełne pliki przy zmianach, nie fragmenty (chyba że mała poprawka).

### Backend (C# / ASP.NET Core)
- Clean Architecture — kierunek zależności do środka, Domain niezależny.
- CQRS przez MediatR — Command/Query + Handler + Validator.
- Kontrolery cienkie, logika w handlerach.
- Fluent API do konfiguracji encji (osobne klasy `Configurations/`).
- Async dla I/O.
- PascalCase dla typów i metod publicznych.

### Frontend (Vue / TypeScript)
- Composition API + `<script setup>`, nigdy Options API.
- TypeScript bez `any` bez uzasadnienia.
- Pinia = client state, Vue Query = server state.
- Komponent → composable → api → axios. Bez bezpośrednich wywołań axiosa w komponentach.
- Feature-based: kod do `features/<domena>/`, reużywalne do `shared/`.
- VeeValidate + Zod do formularzy.

### Style (SCSS)
- Tylko SCSS, bez utility frameworków.
- Globalne style wg 7-1 w `assets/styles/`.
- `<style lang="scss" scoped>` w komponentach.
- Liquid Glass tylko przez mixin.

---

## Reguły bezpieczeństwa

- Autoryzacja zawsze weryfikowana na backendzie — frontend tylko chowa UI.
- Resource-based authorization — sprawdzaj dostęp do konkretnego zasobu, nie tylko status zalogowania.
- Sekrety (JWT key, connection strings, klucze MinIO) w zmiennych środowiskowych / `appsettings.Development.json` poza repo. Nigdy nie commituj sekretów.
- Pliki wideo w MinIO, nie w bazie. Dostęp przez presigned URLs.

---

## Etapowość

Praca zgodnie z etapami z `IMPLEMENTATION_PLAN.md`:
1. Domena bez plików (placeholder URLs).
2. MinIO + storage.
3. Płatności.

Nie wyprzedzaj etapów — nie buduj uploadu przed ukończeniem domeny.

---

## Pull requests / commits

- **AI nigdy nie robi commitów samodzielnie.** Agent przygotowuje zmiany, uruchamia build/testy/lint, a decyzję o commicie i treść message pozostawia deweloperowi.
- Commit messages po angielsku, w trybie rozkazującym (`add course filtering`, `fix enrollment constraint`).
- Jeden commit = jedna logiczna zmiana.
- Przed commitem: build + testy + lint przechodzą.

---

## Pełny kontekst

Szczegóły architektury, model bazy, lista bibliotek i pełna struktura plików: **`IMPLEMENTATION_PLAN.md`**. Reguły specyficzne dla Claude Code: **`CLAUDE.md`**. W razie konfliktu instrukcji priorytet ma `IMPLEMENTATION_PLAN.md`.
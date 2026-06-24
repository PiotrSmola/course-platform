# AGENTS.md

Reguły dla agentów AI pracujących nad projektem **Course Platform**. Plik zgodny z konwencją `AGENTS.md` — czytany przez narzędzia agentowe (Claude Code, Cursor, Copilot i inne). Stanowi uzupełnienie `CLAUDE.md` i `IMPLEMENTATION_PLAN.md`.

---

## O projekcie

Platforma kursów online w stacku Vue 3 + ASP.NET Core 9. Fullstack portfolio project. Backend w Clean Architecture, frontend Vue 3 + TypeScript + SCSS. Pełna specyfikacja w `IMPLEMENTATION_PLAN.md`.

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

### Backend
```bash
dotnet restore
dotnet build
dotnet ef database update --project src/CoursePlatform.Infrastructure --startup-project src/CoursePlatform.API
dotnet run --project src/CoursePlatform.API
```

### Frontend
```bash
cd frontend
npm install
npm run dev
```

### Docker (cały stack)
```bash
docker compose up -d
```

---

## Komendy weryfikacyjne

Przed zgłoszeniem zmian jako gotowe, agent uruchamia:

### Backend
```bash
dotnet build
dotnet test
```

### Frontend
```bash
cd frontend
npm run type-check
npm run lint
npm run build
```

Zmiany nie są kompletne, dopóki build i testy nie przechodzą.

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

- Commit messages po angielsku, w trybie rozkazującym (`add course filtering`, `fix enrollment constraint`).
- Jeden commit = jedna logiczna zmiana.
- Przed commitem: build + testy + lint przechodzą.

---

## Pełny kontekst

Szczegóły architektury, model bazy, lista bibliotek i pełna struktura plików: **`IMPLEMENTATION_PLAN.md`**. Reguły specyficzne dla Claude Code: **`CLAUDE.md`**. W razie konfliktu instrukcji priorytet ma `IMPLEMENTATION_PLAN.md`.

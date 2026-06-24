# CLAUDE.md

Reguły pracy nad projektem **Course Platform** (Vue 3 + ASP.NET Core 9). Ten plik czyta Claude Code przed każdą sesją.

---

## Kontekst projektu

Platforma kursów online. Fullstack: backend ASP.NET Core 9 (Clean Architecture), frontend Vue 3 + TypeScript. Projekt portfolio — jakość kodu i architektura mają znaczenie. Pełny plan w `IMPLEMENTATION_PLAN.md` — przeczytaj go przed rozpoczęciem pracy, ze szczególnym uwzględnieniem **sekcji 0 (Status środowiska)**.

## Środowisko jest już postawione

Szkielet istnieje — solucja, cztery projekty Clean Architecture, projekty testowe, projekt Vue z zainstalowanymi paczkami, pełna konfiguracja Docker. **Nie scaffolduj od zera** (`dotnet new`, `npm create vite` itp.). Twoje zadanie to dopełnianie struktury kodem. Kod aplikacji jeszcze nie istnieje — zaczynasz od Domain (encje).

## Wszystkie komendy przez Docker

`dotnet`, `dotnet ef`, `npm` odpalasz **wewnątrz kontenerów**, nigdy na hoście:

```bash
docker compose exec api bash       # backend (kontener cp_api)
docker compose exec frontend bash  # frontend (kontener cp_frontend)
```

Konkrety: kontenery `cp_api` / `cp_frontend` / `cp_db`, porty 8080 / 5173 / 5432, connection string z hostem `db` (nie `localhost`). Pełne komendy EF Core i szczegóły w sekcji 0 planu.

---

## Język i komunikacja

- **Odpowiadaj po polsku.** Nazwy techniczne, terminy branżowe zostawiaj po angielsku.
- **Kod, nazwy plików, zmienne, commit messages — po angielsku.**
- Bądź konkretny — gotowe rozwiązania zamiast ogólników.
- Jeśli czegoś nie wiesz lub nie jesteś pewien — powiedz wprost, nie zgaduj.

---

## Reguły kodu

- **Nie dodawaj komentarzy do kodu**, chyba że wprost o to poproszę.
- Przy zmianach podawaj pełne pliki, nie fragmenty — chyba że poprawka jest mała i jednoznaczna.
- Trzymaj się istniejących konwencji w projekcie. Zanim dodasz bibliotekę, sprawdź czy podobna już nie jest używana.

---

## Backend — reguły

- **Clean Architecture jest nienaruszalna.** Kierunek zależności: API → Application → Domain; Infrastructure → Application/Domain. Domain nie zależy od niczego.
- **Domain** — czyste C#, zero EF Core, zero ASP.NET, zero atrybutów z bibliotek zewnętrznych.
- **Application** — logika przez CQRS (MediatR). Każda operacja to Command albo Query z własnym Handlerem i Validatorem (FluentValidation).
- **Infrastructure** — implementacje interfejsów z Application (DbContext, MinIO, JWT).
- **Kontrolery są cienkie** — tylko odbierają request, wysyłają przez MediatR, zwracają wynik. Zero logiki biznesowej.
- Konfiguracja encji przez **Fluent API** w osobnych klasach `Configurations/`, nie przez atrybuty na encjach.
- Wszystkie operacje I/O **async**.
- `unique` constrainty (Enrollment, LessonProgress, Review) konfigurowane w Fluent API — to logika biznesowa.
- **Resource-based authorization** — sprawdzaj nie tylko czy user zalogowany, ale czy ma dostęp do konkretnego zasobu (enrollment na dany kurs).

---

## Frontend — reguły

- **Vue 3 Composition API + `<script setup>`** zawsze. Nigdy Options API.
- **TypeScript** wszędzie, bez `any` bez uzasadnienia.
- **Podział state:** Pinia = client state (token, user, UI). TanStack Vue Query = server state (dane z API). Nie trzymaj danych serwerowych w Pinii.
- **Komponenty nie wołają axiosa bezpośrednio.** Flow: komponent → composable → `*.api.ts` → axios client. Composable owija wywołanie w Vue Query.
- Composables zaczynają się od `use`.
- **Struktura feature-based.** Nowy kod trafia do odpowiedniego `features/<domena>/`. Reużywalne rzeczy do `shared/`.
- Walidacja formularzy przez VeeValidate + Zod. Schematy Zod w `schemas/`.

---

## Style — reguły

- **SCSS, nie Tailwind ani inny framework utility.**
- Style globalne (zmienne, mixiny, reset, glass) w `assets/styles/` wg architektury 7-1.
- Style komponentów w `<style lang="scss" scoped>`.
- Efekt Liquid Glass tylko przez mixin z `abstracts/_mixins.scss` — nie powtarzaj `backdrop-filter` ręcznie w komponentach.
- Globalne zmienne/mixiny dostępne w komponentach przez `additionalData` w `vite.config.ts` — nie importuj ich ręcznie w każdym pliku.

---

## Etapy projektu

Trzymaj się kolejności z `IMPLEMENTATION_PLAN.md`. Nie wyprzedzaj etapów:

1. **Etap 1** — domena bez plików (wideo jako placeholder URL). To 70% wartości.
2. **Etap 2** — MinIO + storage + presigned URLs.
3. **Etap 3** — płatności.

Nie implementuj uploadu wideo (MinIO) przed ukończeniem domeny z Etapu 1.

---

## Czego nie robić

- Nie scaffolduj szkieletu od nowa — środowisko stoi (patrz sekcja 0 planu).
- Nie odpalaj komend `dotnet`/`npm` na hoście — zawsze przez `docker compose exec`.
- Nie używaj `localhost` jako hosta bazy w kodzie backendu — z kontenera to `db`.
- Nie łam kierunku zależności Clean Architecture.
- Nie wrzucaj logiki biznesowej do kontrolerów.
- Nie trzymaj plików wideo w bazie — zawsze storage zewnętrzny (MinIO).
- Nie ufaj frontendowi w kwestii autoryzacji — prawdziwa bramka jest na backendzie.
- Nie dodawaj komentarzy do kodu bez prośby.
- Nie mieszaj Tailwind ani innych utility frameworków — tylko SCSS.
- Nie używaj Options API w Vue.
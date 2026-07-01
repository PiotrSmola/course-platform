# 19 — Git, jakość kodu, StyleCop

> **Poziom:** 🟡 · **Czas:** ~45 min · **Wymaga:** [17](./17-docker-cicd-produkcja.md)

## Po co ci to

Git i jakość kodu to nie „ekstra" — to codzienność w zespole. Znasz już Git (GitHub/GitLab), więc tutaj skupiamy
się na **praktykach w kontekście .NET**: jak wygląda współpraca, co powinien łapać PR check, i jak zespoły
utrzymują jednolity styl C# (StyleCop/analyzery/EditorConfig). To wiedza, która sprawia, że pierwsze dni w pracy
idą gładko.

## Mostek z tego, co już znasz

- Pracowałeś z GitHubem i GitLabem — workflow (branch → commit → PR → review → merge) jest **identyczny** w .NET.
- Znasz linter/prettier z frontu — w C# odpowiednikiem są **analyzery + `.editorconfig`** (styl i reguły
  wymuszane przy kompilacji).

---

## Git w pracy zespołowej — minimum, którego się oczekuje

| Praktyka | Po co |
|----------|-------|
| Branch `feature/xyz` | Izolacja zmian od głównej gałęzi |
| Małe, sensowne commity | Czytelna historia, łatwy review i `git bisect` |
| Commit message po angielsku, w trybie rozkazującym | Konwencja (jak w Course Platform: „add", „fix", „harden") |
| Pull Request + code review | Druga para oczu przed mergem; wiedza się rozchodzi |
| Rozwiązywanie konfliktów | Nieuniknione przy równoległej pracy |

Workflow jest ten sam niezależnie od platformy:

| Platforma | Typowy kontekst |
|-----------|-----------------|
| **GitHub** | Open source, startupy, GitHub Actions (CI) |
| **GitLab** | Self-hosted, cały DevOps w jednym |
| **BitBucket** | Firmy w ekosystemie Atlassian (Jira obok) |

Uczysz się **Gita**; platforma to tylko UI + CI wokół niego.

### `merge` vs `rebase` (krótko)

- **merge** — łączy gałęzie, zachowując historię obu (tworzy commit scalający). Bezpieczne, „prawdziwa" historia.
- **rebase** — „przenosi" twoje commity na czubek innej gałęzi (liniowa, czysta historia). Nie rebasuj gałęzi,
  które ktoś już pobrał (przepisujesz historię).

Na start: `merge` jest bezpieczniejszy; `rebase` poznaj do czyszczenia własnej gałęzi przed PR.

---

## `.gitignore` w projekcie .NET

Nie commituj artefaktów i sekretów. Kluczowe wpisy dla .NET:

```gitignore
bin/
obj/
*.user
.env
appsettings.Development.json      # jeśli zawiera sekrety
```

`bin/` i `obj/` to wyniki kompilacji (odtwarzalne, ogromne, źródło konfliktów). `.env` i lokalne `appsettings` z
sekretami — nigdy do repo ([03](./03-pierwsza-aplikacja-host-program-cs.md), [10](./10-auth-identity-jwt.md)).

> **Line endings (praktyczny detal).** Windows używa CRLF, Linux/macOS LF. Bez ustawień diff potrafi „zaświecić
> się" na całych plikach. Rozwiązanie: `core.autocrlf` i/lub plik `.gitattributes` (`* text=auto`), który
> normalizuje końce linii w repo — warto go dodać w projektach zespołowych z mieszanymi systemami.

---

## PR check — co automat powinien łapać

PR (Pull Request) to brama do głównej gałęzi. Automatyczny check (CI, [17](./17-docker-cicd-produkcja.md)) powinien
**blokować merge**, jeśli:

- `dotnet build` się nie kompiluje,
- `dotnet test` — testy nie przechodzą,
- (opcjonalnie) analyzery/format zgłaszają naruszenia, brak spadku pokrycia.

W Course Platform uruchomisz to lokalnie: `docker compose exec api dotnet test`. W firmie robi to GitHub Actions
przy każdym PR — nikt nie merguje „na czuja".

---

## StyleCop, analyzery, EditorConfig — jednolity styl C#

W zespole nie chcesz kłótni o spacje i kolejność usingów ani PR-ów odrzucanych za formatowanie. Rozwiązanie: reguły
**wymuszane maszynowo**.

### .NET Analyzers (wbudowane)

Nowoczesne .NET ma wbudowane analyzery jakości i stylu. Włączasz je w `.csproj`:

```xml
<PropertyGroup>
  <EnableNETAnalyzers>true</EnableNETAnalyzers>
  <AnalysisLevel>latest</AnalysisLevel>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>   <!-- ostrzeżenia = błędy kompilacji -->
</PropertyGroup>
```

`TreatWarningsAsErrors` to mocna, ale zdrowa dyscyplina: kod z ostrzeżeniami się nie skompiluje, więc dług nie
narasta.

### `.editorconfig` — jeden standard dla całego zespołu

Plik `.editorconfig` w korzeniu repo definiuje reguły formatowania i stylu, które respektują **wszystkie** IDE
(Rider, Visual Studio, VS Code):

```ini
[*.cs]
indent_size = 4
dotnet_sort_system_directives_first = true
csharp_prefer_braces = true:warning
dotnet_style_null_propagation = true:suggestion
```

Dzięki temu każdy dostaje ten sam format automatycznie — koniec z „u mnie się inaczej formatuje".

### StyleCop

**StyleCop** (historycznie `StyleCop.Analyzers`) to zestaw reguł stylu C# (kolejność składowych, dokumentacja,
nazewnictwo). Na roadmapie jest jako „niebieski". W praktyce 2026 wiele zespołów łączy **wbudowane analyzery +
`.editorconfig`** (czasem + StyleCop dla surowszych reguł). Na rozmowie wystarczy: *„używamy analyzerów i
EditorConfig, żeby styl był jednolity i wymuszany w CI"*.

---

## Konwencje C#, które warto znać

- **PascalCase** dla typów, metod, properties (`CreateCourseCommand`, `Title`); **camelCase** dla pól/parametrów
  lokalnych; prywatne pola często z prefiksem `_` (`_context`).
- Jeden plik = jedna klasa publiczna (konwencja).
- `using`-i uporządkowane, nieużywane usunięte.
- Nazwy mówiące — `GetPublishedCoursesAsync`, nie `GetData`.

Course Platform trzyma te konwencje konsekwentnie — przejrzyj dowolny handler, żeby zobaczyć wzorzec.

---

## Pułapki

1. **Commit `bin/`/`obj/`/`.env`.** Śmieci i sekrety w repo. Zadbaj o `.gitignore`.
2. **Gigantyczne commity „wszystko naraz".** Trudny review, trudny rollback. Małe, tematyczne commity.
3. **Rebase współdzielonej gałęzi.** Przepisujesz historię, którą inni już mają → konflikty i chaos.
4. **Styl „na czuja".** Bez EditorConfig/analyzerów każdy pisze inaczej → szum w diffach, kłótnie na review.
5. **Merge bez zielonego CI.** Wpuszczanie niezbudowanego/nieztestowanego kodu do głównej gałęzi.

## Ćwiczenia

1. 🟢 **Historia projektu.** `git log --oneline -10` w Course Platform. Oceń: czy commity są małe i opisowe? Co
   byś poprawił?
2. 🟢 **`.gitignore`.** Sprawdź, czy `bin/`, `obj/`, `.env` są ignorowane. Czego jeszcze byś nie commitował?
3. 🟡 **Feature branch + PR (symulacja).** Utwórz branch `feature/course-subtitle`, zrób drobną zmianę (np.
   komentarz w README-less pliku ćwiczeniowym), commit z sensownym message, i opisz, co znalazłby recenzent w PR.
4. 🟡 **EditorConfig.** Dodaj minimalny `.editorconfig` (indent 4, sort usingów) i wyjaśnij, dlaczego działa w
   każdym IDE.
5. 🔴 **CI gate.** Zaprojektuj regułę PR: „nie można zmergować, jeśli `dotnet test` czerwony". Gdzie to
   skonfigurujesz i jak to wymusza jakość?

## Pytania kontrolne

1. Jak wygląda typowy workflow feature branch → merge i po co PR/review?
2. Czym różni się `merge` od `rebase` i czego nie wolno rebasować?
3. Czego nie commitujesz w projekcie .NET i dlaczego?
4. Co powinien łapać PR check?
5. Jak zespół wymusza jednolity styl C# (trzy mechanizmy)?
6. Co robi `TreatWarningsAsErrors` i dlaczego bywa zdrowe?

<details>
<summary>Rozwiązania</summary>

1. Tworzysz branch `feature/...`, robisz commity, otwierasz PR, ktoś robi review, po zielonym CI i akceptacji
   następuje merge. PR/review daje drugą parę oczu i rozprzestrzenia wiedzę o zmianie.
2. `merge` łączy gałęzie zachowując historię (commit scalający); `rebase` przenosi commity na czubek innej gałęzi
   (liniowa historia, ale przepisuje ją). Nie rebasuj gałęzi już pobranych przez innych.
3. `bin/`, `obj/` (odtwarzalne artefakty kompilacji), `.env` i lokalne `appsettings` z sekretami — bo to śmieci
   i/lub wrażliwe dane, które nie powinny trafić do repo.
4. Kompilację (`dotnet build`) i testy (`dotnet test`); opcjonalnie analyzery/format i pokrycie. Ma blokować
   merge zepsutego kodu.
5. Wbudowane .NET Analyzers, `.editorconfig` (respektowany przez wszystkie IDE) i ewentualnie StyleCop — plus
   wymuszenie ich w CI.
6. Zamienia ostrzeżenia kompilatora/analyzerów w błędy — kod z ostrzeżeniami się nie kompiluje, więc dług
   techniczny nie narasta po cichu.

</details>

## Idź dalej

➡️ **[20 — Plan nauki, ćwiczenia, checklist, słownik](./20-plan-nauki-cwiczenia-checklist-slownik.md)** —
spinamy kurs: harmonogram, zbiorczy indeks ćwiczeń i checklist gotowości na rozmowę.

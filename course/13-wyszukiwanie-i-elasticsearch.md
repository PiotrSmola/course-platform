# 13 — Wyszukiwanie i Elasticsearch

> **Poziom:** 🟡→🔴 · **Czas:** ~60 min · **Wymaga:** [07](./07-ef-core-zapytania-i-wydajnosc.md)

## Po co ci to

Wyszukiwanie to funkcja, która „jakoś działa" na `LIKE`, a potem — przy wzroście danych i wymagań — okazuje się
za słaba. Musisz wiedzieć, jak wygląda ścieżka: **SQL `LIKE` → Postgres Full Text Search → Elasticsearch**, i
kiedy zrobić kolejny krok. Course Platform ma **dual-mode search** przez `ICourseSearchService`: gdy
`Elastic:Enabled=true` (domyślnie w Compose), działa `ElasticCourseSearchService`; w przeciwnym razie fallback
to `EfCourseSearchService` (`LIKE` + `ToLower()` w PostgreSQL).

## Mostek z tego, co już znasz

Robiłeś `WHERE title LIKE '%vue%'` w SQL i wiesz, że przy dużych tabelach to wolne (full scan). Elasticsearch to
osobny silnik wyspecjalizowany w wyszukiwaniu tekstu — jak dedykowana baza „tylko do szukania", z rankingiem
trafności i funkcjami, których SQL nie ma.

---

## Etap 1: `LIKE` + `ToLower()` — fallback (`EfCourseSearchService`)

Gdy Elasticsearch jest wyłączony (`Elastic:Enabled=false`) albo jako ścieżka awaryjna, wyszukiwanie idzie przez
[EfCourseSearchService](../src/CoursePlatform.Infrastructure/Search/EfCourseSearchService.cs) — EF i
`EF.Functions.Like`, z `ToLower()` po obu stronach dla **niewrażliwości na wielkość liter**:

```csharp
var search = $"%{request.SearchTerm.Trim().ToLower()}%";
query = query.Where(c =>
    EF.Functions.Like(c.Title.ToLower(), search) ||
    EF.Functions.Like(c.ShortDescription.ToLower(), search) ||
    EF.Functions.Like(c.Description.ToLower(), search) ||
    c.Categories.Any(cat => EF.Functions.Like(cat.Name.ToLower(), search)) ||
    c.Technologies.Any(tech => EF.Functions.Like(tech.Name.ToLower(), search)));
```

`GetCoursesQuery` deleguje wyszukiwanie do `ICourseSearchService` — handler nie wie, czy pod spodem jest ES czy EF.

`c.Title.ToLower()` tłumaczy się na SQL `lower("Title")`, więc „vue" znajdzie „Vue 3". To poprawne i wystarczające
na małą/średnią skalę.

### Dygresja historyczna: dlaczego nie kolacja

Projekt miał kiedyś migrację `AddCaseInsensitiveTextCollation`, która nadawała kolumnom kolację ICU `und-x-icu`.
Problem: to kolacja **deterministyczna**, a `LIKE` na niej **nadal był case-sensitive** (a niedeterministyczne
kolacje w PostgreSQL nie współpracują z `LIKE`). Efekt: wyszukiwanie „vue" nie znajdowało „Vue 3". Rozwiązanie:
migracja została **cofnięta** (`RemoveTextCollation`), a case-insensitivity osiągnięto przez `ToLower()` (patrz
kod wyżej). To dobra lekcja: **weryfikuj założenia na danych**, a nie „powinno działać".

### Ograniczenia `LIKE`

- **Wolne na dużych tabelach** — `%term%` nie użyje standardowego indeksu B-tree (full scan).
- **Brak rankingu trafności** — nie wiesz, który wynik „lepiej pasuje".
- **Brak fuzzy search** („veu" → „vue"), synonimów, odmiany, facetów („ile kursów per kategoria").

Gdy to zaczyna przeszkadzać — czas na krok drugi.

---

## Etap 2: PostgreSQL Full Text Search (często wystarcza!)

Zanim sięgniesz po Elasticsearch, PostgreSQL ma **wbudowane** wyszukiwanie pełnotekstowe (`tsvector`/`tsquery`) z
indeksami GIN. Radzi sobie z tokenizacją, rankingiem (`ts_rank`) i jest **w tej samej bazie** (bez nowej
infrastruktury). Npgsql wspiera to w EF.

Koncepcyjnie:
```sql
-- kolumna tsvector + indeks GIN
ALTER TABLE "Courses" ADD COLUMN search_vector tsvector;
CREATE INDEX ix_courses_search ON "Courses" USING GIN(search_vector);
-- zapytanie
SELECT * FROM "Courses" WHERE search_vector @@ plainto_tsquery('vue') ORDER BY ts_rank(...) DESC;
```

> **Reguła kciuka:** dla większości aplikacji Postgres FTS to złoty środek między `LIKE` a Elasticsearch — szybki,
> z rankingiem, bez dodatkowego serwera. Elasticsearch dopiero, gdy potrzebujesz jego zaawansowanych funkcji lub
> skali.

---

## Etap 3: Elasticsearch — dedykowany silnik

**Elasticsearch (ES)** to silnik wyszukiwania i analizy oparty na Apache Lucene. Indeksuje tekst, ocenia trafność
(score), robi fuzzy search, synonimy, agregacje (facety). Osobny serwer, osobna kopia danych.

### Architektura (bardzo ważna intuicja)

ES **nie zastępuje** bazy relacyjnej — jest jej **kopią do wyszukiwania**. Baza pozostaje źródłem prawdy:

```
[API] ──zapis──► [PostgreSQL]   (source of truth: transakcje, relacje)
   │
   └──synchronizacja──► [Elasticsearch]   (kopia zoptymalizowana pod wyszukiwanie)
```

- Przy `CreateCourse`/`UpdateCourse`: zapis do Postgres (transakcja) **i** zaindeksowanie dokumentu w ES (od razu
  albo przez event/worker — patrz [15](./15-zadania-w-tle-i-brokery.md)).
- Przy wyszukiwaniu: pytasz ES (zamiast `LIKE`), dostajesz posortowane po trafności id + score, opcjonalnie
  doładowujesz szczegóły z Postgres po id.

### Dokument w ES

```json
{
  "id": "3fa85f64-...",
  "title": "Vue 3 Masterclass",
  "description": "Composition API, Pinia...",
  "categories": ["Frontend"],
  "price": 99.99,
  "publishedAt": "2026-01-15"
}
```

### Klient .NET

> **Aktualność 2026:** stary klient **NEST** (7.x) jest wycofany. Nowy oficjalny to
> **`Elastic.Clients.Elasticsearch`** (v8+). Poniżej idea zapytania (składnia zależy od wersji klienta):

```csharp
var response = await _client.SearchAsync<CourseDocument>(s => s
    .Index("courses")
    .Query(q => q.MultiMatch(m => m
        .Fields(new[] { "title", "description" })
        .Query(searchTerm)
        .Fuzziness(new Fuzziness("AUTO"))))   // toleruje literówki
    .From((page - 1) * pageSize)
    .Size(pageSize));
```

### Koszt: synchronizacja i spójność

Największy koszt ES to **utrzymanie kopii w zgodzie** ze źródłem. Trzeba obsłużyć: indeksowanie przy każdej
zmianie, ponowne indeksowanie po awarii, ewentualne rozjazdy (ES jest „eventually consistent"). Dlatego nie
wprowadza się ES „bo brzmi pro" — tylko gdy funkcje/skala tego wymagają.

### Solr, Sphinx (wzmianka)

Alternatywne silniki wyszukiwania. W ekosystemie .NET dominuje ES (+ stack ELK: Elasticsearch, Logstash, Kibana —
także do logów, [11](./11-logowanie-serilog.md)). Znać różnicę „SQL search vs dedykowany silnik" wystarczy.

---

## Kiedy który etap — decyzja

| Sytuacja | Wybór |
|----------|-------|
| Mała/średnia baza, proste szukanie, ES wyłączony | `LIKE` + `ToLower()` (`EfCourseSearchService`) |
| Dev Compose z `Elastic:Enabled=true` | **Elasticsearch** (`ElasticCourseSearchService`) + fallback EF |
| Rosnąca baza, potrzebny ranking/wydajność, jedna baza | **PostgreSQL FTS** |
| Fuzzy, synonimy, facety, ogromna skala, analityka | **Elasticsearch** |

> **Na rozmowie:** „zaczynam od `LIKE`/FTS w Postgresie; Elasticsearch, gdy potrzebuję rankingu, fuzzy i facetów
> lub skali — świadom kosztu synchronizacji" — to dojrzała odpowiedź.

---

## Pułapki

1. **`LIKE '%x%'` na dużej tabeli.** Full scan; przy wzroście danych zabija wydajność. Wtedy FTS/ES.
2. **Traktowanie ES jak bazy głównej.** ES to kopia do szukania, nie źródło prawdy (brak transakcji jak w SQL).
3. **Brak strategii synchronizacji.** ES rozjeżdża się ze źródłem bez planu na (re)indeksowanie.
4. **ES „na wyrost".** Dokładasz serwer i złożoność, gdy Postgres FTS by wystarczył.
5. **Case-sensitivity z założenia.** Jak pokazała historia projektu — sprawdź na danych, czy szukanie faktycznie
   ignoruje wielkość liter.

## Ćwiczenia

1. 🟢 **Prześledź dual-mode.** Otwórz `ICourseSearchService`, `DependencyInjection.cs` (rejestracja ES vs EF) i
   `GetCoursesQueryHandler`. Wskaż, po których polach szuka `SearchTerm` i kiedy używany jest który backend.
2. 🟢 **Test na danych.** Uruchom projekt z seedem i zawołaj `GET /api/courses?searchTerm=vue` oraz `?searchTerm=VUE`.
   Czy wyniki są takie same? Dlaczego?
3. 🟡 **Granice LIKE.** Wypisz 3 rzeczy, których obecne wyszukiwanie nie potrafi (ranking, fuzzy, facety…) i podaj
   przykład zapytania użytkownika, które by ucierpiało.
4. 🔴 **Projekt migracji.** Naszkicuj (na papierze) przejście wyszukiwania kursów na PostgreSQL FTS: kolumna
   `tsvector`, indeks GIN, jak zmieni się zapytanie, jak utrzymać wektor aktualnym.

## Pytania kontrolne

1. Jak Course Platform osiąga wyszukiwanie niewrażliwe na wielkość liter w ścieżce EF (`EfCourseSearchService`)
   i na jaki SQL się to tłumaczy? Kiedy używany jest Elasticsearch zamiast EF?
2. Dlaczego migracja z kolacją nie zadziałała i jak problem rozwiązano?
3. Wymień 3 ograniczenia `LIKE` jako mechanizmu wyszukiwania.
4. Czym jest PostgreSQL FTS i kiedy wybrać go zamiast Elasticsearch?
5. Jaka jest rola Elasticsearch względem bazy relacyjnej (kto jest źródłem prawdy)?
6. Jaki jest największy koszt wprowadzenia Elasticsearch?

<details>
<summary>Rozwiązania</summary>

1. W ścieżce EF: `EF.Functions.Like(c.Title.ToLower(), "%term%")` → SQL `lower(...) LIKE lower(...)`. Gdy
   `Elastic:Enabled=true`, `GetCoursesQuery` idzie przez `ElasticCourseSearchService` (multi-match, fuzzy); EF
   zostaje fallbackiem przy wyłączonym ES.
2. Kolacja `und-x-icu` była deterministyczna, a `LIKE` na niej pozostawał case-sensitive (a niedeterministyczne
   kolacje nie współpracują z `LIKE`). Cofnięto migrację i użyto `ToLower()`.
3. Wolne na dużych tabelach (full scan), brak rankingu trafności, brak fuzzy/synonimów/facetów.
4. To wbudowane wyszukiwanie pełnotekstowe Postgresa (`tsvector`/`tsquery` + indeks GIN, ranking `ts_rank`).
   Wybierasz je, gdy `LIKE` za słaby, ale nie chcesz dokładać osobnego serwera — jest w tej samej bazie.
5. Elasticsearch to kopia danych zoptymalizowana pod wyszukiwanie; źródłem prawdy pozostaje baza relacyjna
   (transakcje, relacje). ES trzeba synchronizować ze źródłem.
6. Utrzymanie kopii w zgodzie ze źródłem: indeksowanie przy zmianach, reindeksowanie, obsługa rozjazdów
   (eventual consistency).

</details>

## Idź dalej

➡️ **[14 — Komunikacja: GraphQL, gRPC, SignalR, OData/Gridify](./14-komunikacja-graphql-grpc-signalr.md)** —
alternatywne i uzupełniające sposoby, w jakie klient rozmawia z API.

# 02 — HTTP, REST i jak działa Web API

> **Poziom:** 🟢 laik · **Czas:** ~60 min · **Wymaga:** [01](./01-dotnet-i-csharp-od-zera.md)

## Po co ci to

Backend to program, który **odbiera żądania HTTP i odsyła odpowiedzi**. Cała reszta (kontrolery, EF, auth) to
obudowa wokół tego jednego zdania. Jeśli rozumiesz HTTP i REST, rozumiesz, co robi każdy endpoint w Course
Platform i dlaczego zwraca taki, a nie inny kod. To wiedza, którą **dostaniesz na każdej rozmowie** — i tu
opanujesz ją porządnie.

## Mostek z tego, co już znasz

Robiłeś we froncie `fetch('/api/courses')` albo `axios.get(...)`. To była **jedna strona** rozmowy HTTP —
klient. Teraz przechodzisz na **drugą stronę** — piszesz serwer, który te żądania odbiera. Nic nowego
pojęciowo: te same metody (GET/POST), te same statusy (200/404), te same nagłówki. Tylko teraz to Ty decydujesz,
co odpowiedzieć.

W Vue widziałeś w Course Platform `client.ts` (axios) wysyłający `Authorization: Bearer <token>`. W tym
rozdziale zobaczysz, co się z tym nagłówkiem dzieje po stronie serwera.

---

## HTTP — protokół tekstowy

HTTP to rozmowa: **klient wysyła request, serwer odsyła response**. Oba to zwykły tekst o ustalonej strukturze.

### Anatomia żądania (request)

```
POST /api/courses HTTP/1.1
Host: localhost:8080
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

{"title":"Vue 3","price":99.99}
```

| Element | Co to |
|---------|-------|
| **Metoda** | Co chcesz zrobić: `GET` (czytaj), `POST` (twórz), `PUT`/`PATCH` (zmień), `DELETE` (usuń) |
| **Ścieżka (path)** | Który zasób: `/api/courses`, `/api/courses/3fa85f64-...` |
| **Nagłówki (headers)** | Metadane: typ treści (`Content-Type`), token (`Authorization`), język |
| **Ciało (body)** | Treść — zwykle JSON. Tylko przy `POST`/`PUT`/`PATCH` |

### Anatomia odpowiedzi (response)

```
HTTP/1.1 201 Created
Content-Type: application/json

{"id":"3fa85f64-5717-4562-b3fc-2c963f66afa6","title":"Vue 3","price":99.99}
```

Odpowiedź ma **kod statusu** (liczba mówiąca, jak poszło), nagłówki i opcjonalnie ciało.

### Metody HTTP i ich znaczenie

| Metoda | Znaczenie | Idempotentna? | Ma body? |
|--------|-----------|---------------|----------|
| `GET` | Pobierz zasób | Tak (nie zmienia stanu) | Nie |
| `POST` | Utwórz nowy zasób | Nie (dwa POST = dwa zasoby) | Tak |
| `PUT` | Zastąp cały zasób | Tak | Tak |
| `PATCH` | Zmień część zasobu | Zależy | Tak |
| `DELETE` | Usuń zasób | Tak | Zwykle nie |

> **Idempotentna** = powtórzenie tego samego żądania daje ten sam efekt. `GET /courses/5` możesz wywołać
> 100 razy — nic się nie zepsuje. `POST /courses` dwa razy = dwa kursy. Ta różnica jest ważna np. przy retry.

---

## Kody statusu — te musisz znać na pamięć

| Kod | Nazwa | Kiedy | W Course Platform |
|-----|-------|-------|-------------------|
| **200** | OK | Sukces, jest treść | `GET /api/courses` |
| **201** | Created | Utworzono zasób | (czasem po POST) |
| **204** | No Content | Sukces, brak treści | po `PUT`/`DELETE` (np. `UpdateCourse` zwraca `NoContent()`) |
| **400** | Bad Request | Złe dane wejściowe (walidacja) | FluentValidation zwraca `{ errors: {...} }` |
| **401** | Unauthorized | Brak/zły token (nie wiadomo kim jesteś) | brak `Authorization` na `[Authorize]` |
| **403** | Forbidden | Wiadomo kim jesteś, ale nie masz prawa | student bez enrollmentu → lekcja |
| **404** | Not Found | Zasób nie istnieje | `GET /api/courses/{zły-id}` |
| **409** | Conflict | Konflikt (np. duplikat) | (potencjalnie przy duplikacie enrollmentu) |
| **429** | Too Many Requests | Rate limiting | po przekroczeniu limitu żądań |
| **500** | Internal Server Error | Bug serwera | nieobsłużony wyjątek |

> **401 vs 403 — klasyczne pytanie rekrutacyjne.** 401 = „nie wiem, kim jesteś" (brak/zły token, zaloguj się).
> 403 = „wiem, kim jesteś, ale nie wolno ci tego" (jesteś zalogowany, ale nie masz uprawnień). Course Platform
> rozróżnia je: `[Authorize]` bez tokenu → 401; `ForbiddenAccessException` (np. dostęp do cudzego kursu) → 403.

Zapamiętaj grupy: **2xx** = sukces, **4xx** = twoja wina (klienta), **5xx** = wina serwera.

---

## REST — konwencja projektowania API

REST to zestaw zasad, jak układać URL-e i metody, żeby API było przewidywalne. Nie jest to standard z policją —
to konwencja, którą wszyscy znają, więc warto jej trzymać.

### Zasoby jako rzeczowniki w liczbie mnogiej

```
GET    /api/courses            — lista kursów
GET    /api/courses/{id}       — jeden kurs
POST   /api/courses            — utwórz kurs
PUT    /api/courses/{id}       — zastąp kurs
DELETE /api/courses/{id}       — usuń kurs
```

Zasoby zagnieżdżone (kurs → jego lekcje) w Course Platform:

```
GET  /api/courses/{courseId}/lessons/{lessonId}          — pobierz lekcję
POST /api/courses/{courseId}/lessons/{lessonId}/complete — oznacz jako ukończoną
```

To realne trasy z `LessonsController` — zobaczysz je w [05](./05-kontrolery-minimal-api-mvc-blazor.md).

### Konwencje, które robią różnicę

- **Rzeczowniki, nie czasowniki:** `/courses`, nie `/getCourses`. Czasownik wyraża **metoda HTTP**.
- **Id w ścieżce, nie w body:** `/courses/5`, nie `/courses` z `{id:5}`.
- **Filtrowanie w query stringu:** `/api/courses?searchTerm=vue&pageNumber=1&pageSize=10&level=2`.
- **Błędy jako JSON:** `{ "error": "..." }` albo `{ "errors": { "Title": ["..."] } }` (Course Platform używa obu).

### Wersjonowanie (dla świadomości)

Gdy API żyje długo i nie chcesz psuć starych klientów:

```
/api/v1/courses
/api/v2/courses
```

albo nagłówek `Api-Version: 2`. Course Platform tego nie ma (jedna wersja) — ale warto wiedzieć, że istnieje.

---

## HTTPS — HTTP + szyfrowanie

**HTTPS** to HTTP owinięty w szyfrowanie **TLS**. W dev często jedziesz na `http://localhost` (jak w Course
Platform: `http://localhost:8080`). W produkcji **zawsze HTTPS** — certyfikat od Let's Encrypt / chmury /
Cloudflare. Bez tego token i hasła lecą przez sieć jawnym tekstem.

---

## Stateless — serwer nie pamięta poprzednich żądań

HTTP jest **bezstanowy**: każde żądanie jest „świeże", serwer nie wie, że minutę temu rozmawialiście. To znaczy,
że **tożsamość trzeba dołączać do każdego żądania**. Dwie szkoły:

- **Sesje** — serwer trzyma stan (kto jest zalogowany), klient nosi tylko ciasteczko z ID sesji.
- **JWT (token)** — klient nosi podpisany token z informacją „kim jestem"; serwer tylko **weryfikuje podpis**,
  nic nie pamięta. Skalowalne (każda instancja serwera weryfikuje token niezależnie).

Course Platform używa **JWT**: po logowaniu dostajesz token, front dokłada go do każdego żądania jako
`Authorization: Bearer <token>`, a backend go weryfikuje. Pełny mechanizm w [10](./10-auth-identity-jwt.md) —
teraz wystarczy, że rozumiesz **dlaczego** token wędruje w każdym żądaniu (bo HTTP nic nie pamięta).

---

## Jak to spina się w Course Platform (przepływ jednego żądania)

```
[Vue: axios GET /api/courses]
        │  Authorization: Bearer <token>
        ▼
[ASP.NET Core :8080]
        │  middleware: logi → CORS → auth (odczyt tokenu) → ...
        ▼
[CoursesController.GetCourses]  ── wysyła GetCoursesQuery ──►  [Handler] ──► [baza]
        ▼
[JSON: { items: [...], totalCount: 42 }]  ── status 200 ──►  z powrotem do Vue
```

Nie musisz jeszcze rozumieć każdego kroku — wrócą w kolejnych rozdziałach. Ważne: **request wchodzi, przechodzi
przez łańcuch, wychodzi response**. To jest ASP.NET Core w jednym zdaniu.

---

## Pułapki

1. **Mylenie 401 i 403.** (Patrz wyżej — pytają o to na rozmowach.)
2. **POST tam, gdzie powinien być GET (i odwrotnie).** GET nie zmienia stanu i można go cache'ować; nie rób
   `GET /courses/5/delete`.
3. **Zwracanie 200 z komunikatem błędu w body.** Jeśli coś poszło źle, użyj kodu 4xx/5xx — klient (i cache,
   i monitoring) na tym polega.
4. **Wrażliwe dane w URL/query.** URL trafia do logów i historii. Token i hasła → nagłówek/body, nie query.
5. **Zapominanie, że HTTP jest stateless.** „Przecież się zalogowałem" — tak, ale kolejne żądanie tego nie wie,
   dopóki nie dołączysz tokenu.

## Ćwiczenia

> Uruchom projekt i otwórz Swagger: `http://localhost:8080/swagger`.

1. 🟢 **Zwiedzanie API.** W Swaggerze wypisz 5 endpointów Course Platform z metodą i ścieżką. Przy każdym
   zgadnij kod sukcesu (200/201/204). *Done, gdy* masz listę 5 pozycji.
2. 🟢 **Publiczne vs chronione.** Zawołaj `GET /api/courses` bez tokenu (przez Swagger lub `curl`). Potem zawołaj
   endpoint z kłódką (np. `GET /api/auth/me`) bez tokenu. Jakie kody dostajesz i dlaczego?
3. 🟡 **Cały cykl.** Zaloguj się (`POST /api/auth/login` danymi seed), skopiuj token, ustaw go w Swaggerze
   (Authorize) i zawołaj `GET /api/auth/me`. *Done, gdy* dostajesz 200 z danymi użytkownika.
4. 🟡 **Statusy w praktyce.** Zawołaj `GET /api/courses/00000000-0000-0000-0000-000000000000`. Jaki kod? Dlaczego
   akurat ten?

## Pytania kontrolne

1. Wymień 5 metod HTTP i po jednym zdaniu, do czego służą.
2. Czym różni się 401 od 403? Podaj scenariusz z Course Platform dla każdego.
3. Co znaczy, że HTTP jest „stateless" i jak to obchodzimy przy logowaniu?
4. Dlaczego id zasobu dajemy w ścieżce, a filtry w query stringu?
5. Kiedy zwrócisz 204 zamiast 200?
6. Co robi nagłówek `Authorization: Bearer ...` i kto go weryfikuje?

<details>
<summary>Rozwiązania</summary>

1. `GET` — pobierz; `POST` — utwórz; `PUT` — zastąp całość; `PATCH` — zmień część; `DELETE` — usuń.
2. **401** = brak/zły token, serwer nie wie, kim jesteś (np. `GET /api/auth/me` bez tokenu). **403** = jesteś
   zalogowany, ale nie masz prawa (np. student bez enrollmentu prosi o lekcję → `ForbiddenAccessException`).
3. Serwer nie pamięta poprzednich żądań. Przy logowaniu obchodzimy to, dołączając do **każdego** żądania token
   (JWT) w nagłówku `Authorization`, który serwer weryfikuje od nowa.
4. Id identyfikuje **konkretny zasób** (część jego adresu). Filtry/paginacja to **parametry zapytania**, nie
   tożsamość zasobu — dlatego query string. Dodatkowo id w ścieżce jest cache-friendly i czytelne.
5. Gdy operacja się udała, ale nie ma czego zwracać — np. po `PUT`/`DELETE` (w Course Platform `UpdateCourse`
   zwraca `NoContent()`).
6. Niesie token JWT identyfikujący klienta. Weryfikuje go backend (middleware uwierzytelniania) — sprawdza
   podpis i ważność, a potem wypełnia `HttpContext.User`.

</details>

## Idź dalej

➡️ **[03 — Pierwsza aplikacja: host, `Program.cs`, konfiguracja](./03-pierwsza-aplikacja-host-program-cs.md)** —
tworzymy i uruchamiamy aplikację ASP.NET Core i rozumiemy, co dzieje się od `dotnet run` do nasłuchu na porcie.

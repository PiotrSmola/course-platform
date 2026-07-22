# 18 — Integracja z frontendem (Vue)

> **Poziom:** 🟡 · **Czas:** ~50 min · **Wymaga:** [02](./02-http-rest-i-jak-dziala-web-api.md), [10](./10-auth-identity-jwt.md)

## Po co ci to

Znasz Vue — to Twoja przewaga. Ten rozdział spina backend, którego się uczysz, z frontem, który już rozumiesz:
jak API i SPA się dogadują, jak wędruje token, jak wyglądają błędy i serializacja. Dzięki temu zobaczysz **cały
obieg** funkcji: od kliknięcia w Vue do zapisu w bazie i z powrotem. To też realia pracy — junior .NET często
współpracuje z frontem.

## Mostek z tego, co już znasz

Course Platform ma front w Vue 3 (Composition API, Pinia, Vue Query, axios). Backend to osobny program, który
podaje JSON. Wszystko, co robiłeś w Vue (axios, store, obsługa 401), teraz ma „drugą stronę", którą już poznałeś
w rozdziałach [02](./02-http-rest-i-jak-dziala-web-api.md), [04](./04-middleware-filtry-atrybuty.md), [10](./10-auth-identity-jwt.md).

---

## Model mentalny: dwa osobne programy

```
[Przeglądarka]
   Vue SPA (:5173)  ── HTTP/JSON ──►  ASP.NET API (:8080)  ──►  PostgreSQL
                    ◄── JSON ────────
```

Vue **nie jest** częścią ASP.NET — to dwa niezależne procesy (dwa kontenery). Łączą je trzy rzeczy:
1. **HTTP/REST** — kontrakt żądań i odpowiedzi,
2. **CORS** — zgoda przeglądarki na rozmowę między różnymi originami (`:5173` ↔ `:8080`),
3. **JWT** — tożsamość dołączana do każdego żądania.

---

## CORS — dlaczego front go potrzebuje

Przeglądarka blokuje żądania między różnymi **originami** (host:port) z powodów bezpieczeństwa (same-origin
policy). `localhost:5173` (Vue) i `localhost:8080` (API) to różne originy — więc backend musi **jawnie pozwolić**.
Course Platform w `Program.cs`:

```csharp
builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader().AllowAnyMethod()
              .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset")));
// ...
app.UseCors("AllowFrontend");
```

> **Ważne:** CORS to mechanizm **przeglądarki**. Postman/`curl` ignorują CORS — dlatego „w Postmanie działa, w
> przeglądarce nie" to prawie zawsze problem z CORS. Na produkcji często obchodzi się to inaczej: front i API pod
> tym samym originem (nginx serwuje front i proxuje `/api` do backendu) — wtedy CORS znika.

---

## Przepływ tożsamości (JWT) — od loginu po żądanie

To domknięcie [10](./10-auth-identity-jwt.md) od strony frontu:

```
1. Vue: POST /api/auth/login { email, password }
2. Backend: zwraca { token, roles, ... }
3. Vue: zapisuje token (Course Platform: w localStorage + Pinia)
4. axios request interceptor dokłada: Authorization: Bearer <token>   (do KAŻDEGO żądania)
5. Backend: UseAuthentication weryfikuje token → HttpContext.User
6. axios response interceptor: przy 401 → wyloguj i przekieruj na /login
```

Fragment klienta axios w Course Platform (`shared/api/client.ts`) — koncepcyjnie:

```typescript
client.interceptors.request.use((config) => {
  const auth = useAuthStore()
  if (auth.token) config.headers.Authorization = `Bearer ${auth.token}`
  return config
})

client.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401) { useAuthStore().logout(); router.push({ name: 'Login' }) }
    return Promise.reject(error)
  }
)
```

> Backend jest **prawdziwą bramką** — front tylko chowa przyciski i reaguje na 401/403. Ukrycie akcji w UI to
> UX, nie bezpieczeństwo ([10](./10-auth-identity-jwt.md)).

---

## Serializacja JSON — camelCase ↔ PascalCase

C# używa `PascalCase` (`Title`, `InstructorId`), JS/Vue — `camelCase` (`title`, `instructorId`). ASP.NET Core
**domyślnie** serializuje do `camelCase`, więc:

```csharp
// C#
public record CourseListDto(Guid Id, string Title, decimal Price);
```
```json
// JSON, który dostaje Vue
{ "id": "...", "title": "Vue 3", "price": 99.99 }
```

W drugą stronę: Vue wysyła `{ "title": "..." }`, a ASP.NET wiąże to na `CreateCourseCommand.Title`. Konwersja jest
automatyczna — nie musisz nic robić, ale **musisz wiedzieć**, że nazwy pól po stronie JSON są camelCase (żeby
front i typy TS się zgadzały).

---

## Obsługa błędów — spójny kontrakt

Backend (`ExceptionHandlingMiddleware`, [04](./04-middleware-filtry-atrybuty.md)) zwraca dwa kształty błędów:

```json
// błąd ogólny (404/403/500)
{ "error": "Course ... not found.", "statusCode": 404 }

// błąd walidacji (400) — per pole
{ "errors": { "Title": ["Tytuł jest wymagany"], "Price": ["..."] }, "statusCode": 400 }
```

Front musi umieć odczytać **oba**. W Course Platform robi to helper (`getApiErrorMessage`): najpierw sprawdza
`error`, potem `errors` (bierze pierwszy komunikat), a jak nic — pokazuje fallback. Dzięki temu formularze mogą
pokazać konkretny komunikat z pola (`errors.Title[0]`), a nie ogólne „coś poszło źle".

> To realny detal, który poprawiono w projekcie: gdyby front czytał tylko `error`, komunikaty walidacji (które są
> w `errors`) nie dotarłyby do użytkownika. Spójny kontrakt błędów + front, który go zna = dobry UX.

---

## Podział stanu na froncie (kontekst dla backendowca)

Warto rozumieć, jak front układa dane, bo to wpływa na to, co API powinno zwracać:

| | Pinia (client state) | Vue Query (server state) |
|--|----------------------|--------------------------|
| Token, zalogowany user, stan UI | ✅ | ❌ |
| Lista kursów, szczegóły, enrollmenty (dane z API) | ❌ | ✅ |
| Cache serwera, invalidacja, refetch | ❌ | ✅ |

Warstwy frontu lustrzane do backendu (komponent → composable → api → axios):

```
Komponent.vue
  → composable (useCourses)   — Vue Query: cache, loading, error
    → courses.api.ts          — czyste funkcje HTTP
      → client.ts             — axios: baseURL, Bearer token, interceptory
```

Dla ciebie jako backendowca płynie z tego wniosek: **API powinno zwracać DTO skrojone pod ekran** (żeby Vue Query
cache'owało sensowne kształty), a nie surowe encje ([05](./05-kontrolery-minimal-api-mvc-blazor.md)/[09](./09-architektura-clean-cqrs-mediatr.md)).

---

## SignalR + Vue (real-time w Course Platform)

Push jest już podpięty: composable **`useRealtime()`** ([frontend/src/shared/composables/useRealtime.ts](../frontend/src/shared/composables/useRealtime.ts))
łączy się z `NotificationHub` (`/hubs/notifications`) przez `@microsoft/signalr`, przekazuje JWT przez
`accessTokenFactory` i reaguje na eventy (invalidacja Vue Query). Wołany z `App.vue` przy starcie aplikacji.
Szczegóły huba po stronie backendu: [14](./14-komunikacja-graphql-grpc-signalr.md).

---

## Pułapki

1. **Błędny origin w CORS.** Front dostaje błąd CORS, a Postman działa → prawie zawsze zła konfiguracja CORS.
2. **Front czyta tylko `error`, ignoruje `errors`.** Komunikaty walidacji nie docierają — czytaj oba kształty.
3. **Zakładanie PascalCase w JSON.** JSON jest camelCase; typy TS muszą to odzwierciedlać.
4. **Zwracanie encji zamiast DTO.** Wyciek pól, cykle, over-fetch; psuje też cache Vue Query.
5. **Traktowanie ukrytego przycisku jako zabezpieczenia.** Bramka jest na backendzie; front tylko poprawia UX.
6. **Token w `localStorage` na publicznym serwerze.** Podatność na XSS; rozważ cookie `HttpOnly` ([10](./10-auth-identity-jwt.md)).

## Ćwiczenia

1. 🟢 **Prześledź żądanie end-to-end.** Zaloguj się w UI (`:5173`), otwórz DevTools → Network, znajdź `GET
   /api/courses`. Sprawdź nagłówek `Authorization` i kształt odpowiedzi (camelCase!).
2. 🟢 **Wywołaj 401.** Usuń/zepsuj token (np. wyloguj w innej karcie) i wywołaj chronioną akcję. Zobacz, jak
   interceptor reaguje na 401.
3. 🟡 **Kontrakt błędu.** Wyślij niepoprawny formularz (np. pusty tytuł kursu jako Instructor) i podejrzyj JSON
   `400` z `errors`. Wskaż, którą gałąź (`error` vs `errors`) czyta front, by pokazać komunikat.
4. 🟡 **DTO vs encja.** Weź jeden endpoint zwracający DTO (`CourseDetailsDto`) i wypisz, które pola encji **nie**
   trafiają do frontu i dlaczego to dobrze (np. `VideoUrl` tylko dla uprawnionych).
5. 🔴 **CORS na produkcji.** Opisz, jak zniknie potrzeba CORS, gdy nginx serwuje front i proxuje `/api` do
   backendu pod jednym originem.

## Pytania kontrolne

1. Dlaczego front potrzebuje CORS, a Postman nie?
2. Jak token trafia do każdego żądania i co robi front przy 401?
3. Jak wygląda serializacja nazw pól C# ↔ JSON i dlaczego to ważne dla typów TS?
4. Jakie dwa kształty błędów zwraca backend i dlaczego front musi znać oba?
5. Co powinno być w Pinia, a co w Vue Query — i jak to wpływa na projekt API?
6. Dlaczego API zwraca DTO, a nie encje, także z perspektywy frontu?

<details>
<summary>Rozwiązania</summary>

1. CORS to mechanizm przeglądarki (same-origin policy) — blokuje żądania między originami, dopóki serwer nie
   pozwoli. Postman/`curl` nie są przeglądarką, więc CORS ich nie dotyczy.
2. axios request interceptor dokłada `Authorization: Bearer <token>` do każdego żądania; przy odpowiedzi 401
   response interceptor wylogowuje i przekierowuje na `/login`.
3. C# PascalCase serializuje się do camelCase w JSON (i odwrotnie przy bindowaniu). Typy TS muszą używać
   camelCase, żeby pasowały do odpowiedzi.
4. `{ error }` (ogólny, np. 404/403/500) i `{ errors: { pole: [...] } }` (walidacja 400). Front czyta oba, by
   pokazać konkretny komunikat walidacji zamiast ogólnego fallbacku.
5. Pinia: client state (token, user, UI). Vue Query: server state (dane z API, cache, invalidacja). Dlatego API
   powinno zwracać DTO skrojone pod ekran — dobrze cache'owalne kształty.
6. Bo encje wyciekają wewnętrzne pola/relacje, grożą cyklami i over-fetchem, wiążą API ze schematem bazy i psują
   cache po stronie frontu. DTO daje kontrolę nad tym, co widzi klient (np. `VideoUrl` tylko dla uprawnionych).

</details>

## Idź dalej

➡️ **[19 — Git, jakość kodu, StyleCop](./19-git-jakosc-stylecop.md)** — praktyki pracy zespołowej i utrzymania
jakości, których oczekuje się w firmie.

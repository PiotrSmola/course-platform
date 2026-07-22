# 14 — Komunikacja: GraphQL, gRPC, SignalR, OData/Gridify

> **Poziom:** 🟡→🔴 · **Czas:** ~75 min · **Wymaga:** [02](./02-http-rest-i-jak-dziala-web-api.md), [05](./05-kontrolery-minimal-api-mvc-blazor.md)

## Po co ci to

REST to standard, ale nie jedyny sposób, w jaki klient rozmawia z serwerem. Na rozmowie i w pracy usłyszysz o
**GraphQL**, **gRPC**, **SignalR**, **OData/Gridify** — musisz wiedzieć, co to, kiedy się przydaje i czym różni
się od REST. Course Platform używa REST — więc tutaj poznasz alternatywy **solidnie, ale bez pełnego setupu** (to
świadoma decyzja: żebyś rozumiał i umiał wybrać, nie żebyś wdrażał każde z nich).

## Mostek z tego, co już znasz

- REST/axios już ogarniasz ([02](./02-http-rest-i-jak-dziala-web-api.md)).
- GraphQL — jeśli dotknąłeś go we froncie (Apollo/urql), to backend GraphQL jest drugą stroną tej samej umowy.
- SignalR ≈ WebSocket/Socket.IO, które mogłeś widzieć w JS — real-time push z serwera.

---

## GraphQL i HotChocolate

### REST vs GraphQL — intuicja

REST: wiele endpointów o **stałym** kształcie odpowiedzi.

```
GET /api/courses/1          → cały kurs + moduły + lekcje (może za dużo — over-fetching)
GET /api/courses/1/reviews  → osobne żądanie (under-fetching: kilka round-tripów)
```

GraphQL: **jeden endpoint** (`POST /graphql`), klient mówi **dokładnie**, czego chce:

```graphql
query {
  course(id: "3fa85f64-...") {
    title
    instructor { firstName }
    modules { title lessons { title duration } }
  }
}
```

Odpowiedź = dokładnie ten kształt JSON, w jednym round-tripie. Klient nie dostaje ani za dużo, ani za mało.

### Podstawowe pojęcia

| Pojęcie | Co to |
|---------|-------|
| **Schema** | Kontrakt: typy `Query`, `Mutation`, `Subscription` |
| **Query** | Odczyt (jak GET) |
| **Mutation** | Zmiana (jak POST/PUT) |
| **Subscription** | Real-time push (przez WebSocket) |
| **Resolver** | Funkcja C#, która zwraca dane pola |
| **N+1** | Resolver odpalany per element → wiele zapytań; rozwiązanie: **DataLoader** (batchuje) |

### HotChocolate — GraphQL w .NET

Najpopularniejszy serwer GraphQL dla ASP.NET Core:

```csharp
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddProjections().AddFiltering().AddSorting();

app.MapGraphQL();   // endpoint /graphql

public class Query
{
    public async Task<Course?> GetCourse(Guid id, [Service] IApplicationDbContext ctx)
        => await ctx.Courses.FindAsync(id);
}
```

IDE do testów GraphQL nazywa się **Nitro** (dawniej Banana Cake Pop) — odpowiednik Swaggera dla REST.

### Kiedy GraphQL, kiedy REST

| GraphQL | REST |
|---------|------|
| Wiele różnych widoków, klienci mobilni z wolną siecią | Proste CRUD, publiczne API |
| Złożone, zagnieżdżone agregacje danych | Cache HTTP (CDN) na GET |
| Jeden zespół fullstack ustala schemat | Standard branżowy, łatwy onboarding |

**Bezpieczeństwo GraphQL:** limit głębokości zapytań (depth limiting), ocena złożoności (complexity), autoryzacja
na resolverach — bo jeden „głęboki" query może być kosztowny.

> Course Platform = REST (właściwy wybór dla nauki i typowego SPA). GraphQL dodajesz, gdy front prosi o
> elastyczność zapytań. **HotChocolate** jest „niebieskie" na roadmapie — jeśli uczysz się GraphQL w .NET, ucz się
> HotChocolate, nie „GraphQL ogólnie".

---

## gRPC — szybka komunikacja serwis↔serwis

**gRPC** = zdalne wywołania (RPC) po HTTP/2, dane w **Protocol Buffers** (format binarny, nie JSON). Kontrakt
definiujesz w pliku `.proto`:

```protobuf
service CourseService {
  rpc GetCourse (GetCourseRequest) returns (CourseResponse);
}
```

```csharp
app.MapGrpcService<CourseGrpcService>();
```

**Plusy:** szybki (binarnie, HTTP/2), ścisły kontrakt, streaming dwukierunkowy.
**Minusy:** przeglądarka nie woła gRPC wprost (potrzebny gRPC-Web/proxy).

**Kiedy:** komunikacja **między mikroserwisami** (wewnętrzna), czasem mobile↔backend. **Nie** jako główne API dla
Vue — tam REST/GraphQL są wygodniejsze. Junior: „znam z nazwy, używane między serwisami, front SPA zwykle REST".

---

## SignalR — real-time push

### Problem, który rozwiązuje

REST: klient **pyta** serwer. Żeby mieć „na żywo", robiłbyś polling (`setInterval(fetch, 1000)`) — brzydkie i
obciąża sieć. **WebSocket** daje **stałe połączenie**, przez które serwer **sam wypycha** dane do klienta.

**SignalR** to abstrakcja Microsoftu nad WebSocket (z fallbackiem na Server-Sent Events / long polling, gdy
WebSocket niedostępny).

### Przypadki użycia

- Czat na żywo, powiadomienia („nowy student zapisał się na kurs"),
- Live dashboard (statystyki na żywo), współedycja,
- **Blazor Server** (cały UI leci przez SignalR).

### Hub (serwer)

```csharp
public class NotificationHub : Hub
{
    public async Task JoinCourseGroup(string courseId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"course-{courseId}");
}

// Program.cs
builder.Services.AddSignalR();
app.MapHub<NotificationHub>("/hubs/notifications");
```

Wysłanie powiadomienia do grupy z dowolnego miejsca aplikacji:

```csharp
await _hubContext.Clients.Group($"course-{courseId}").SendAsync("ReviewAdded", payload);
```

### Klient Vue

```typescript
import * as signalR from '@microsoft/signalr'

const connection = new signalR.HubConnectionBuilder()
  .withUrl('http://localhost:8080/hubs/notifications', {
    accessTokenFactory: () => authStore.token ?? ''      // JWT do huba
  })
  .build()

await connection.start()
await connection.invoke('JoinCourseGroup', courseId)
connection.on('ReviewAdded', (payload) => { /* zaktualizuj UI */ })
```

> **Auth w SignalR:** token często idzie w query string / przez `accessTokenFactory`, bo WebSocket nie zawsze
> przenosi nagłówki jak `fetch`.

**Gdzie w Course Platform:** `NotificationHub` pod `/hubs/notifications`
([Infrastructure/Hubs/NotificationHub.cs](../src/CoursePlatform.Infrastructure/Hubs/NotificationHub.cs)) — user
dołącza do grupy `user:{id}`, admin do `admins`. Powiadomienia wysyła `SignalRNotificationService` (np. po nowej
recenzji). Front: composable `useRealtime()` w `App.vue` łączy się z hubem i odbiera eventy (Vue Query invalidation).

---

## OData i Gridify — elastyczne filtrowanie

Gdy kontroler zaczyna mieć 20 parametrów filtrowania, można oddać część kontroli klientowi.

### OData

Standard Microsoftu: jeden endpoint, klient buduje zapytanie w URL:

```
GET /api/courses?$filter=Price lt 100&$orderby=Title&$top=10&$skip=20
```

Pakiet `Microsoft.AspNetCore.OData`. **Plus:** elastyczne filtrowanie bez mnóstwa parametrów. **Minus:** złożoność
i **bezpieczeństwo** — musisz ograniczyć, co wolno filtrować/sortować (inaczej klient wygeneruje kosztowne query).

### Gridify

Lżejsza alternatywa — filtr/sort jako string z frontu:

```
GET /api/courses?filter=Price<100,Level==2&orderBy=Title
```

Pakiet `Gridify`. Popularne w mniejszych API.

> Course Platform ma **ręczne** parametry w `GetCoursesQuery` (searchTerm, level, price, kategorie…) — czytelne i
> bezpieczne (backend kontroluje, co wolno). OData/Gridify to refactor „gdy parametrów jest za dużo". Gridify jest
> „niebieskie" na roadmapie.

---

## Pułapki

1. **GraphQL bez limitów.** Brak depth/complexity limiting → jeden głęboki query kładzie serwer.
2. **N+1 w resolverach GraphQL.** Bez DataLoadera każde pole odpala osobne zapytanie.
3. **gRPC jako API dla przeglądarki.** Bez gRPC-Web/proxy przeglądarka go nie zawoła — to komunikacja serwis↔serwis.
4. **SignalR zamiast prostego pollingu wszędzie.** Real-time dokłada złożoność (stan połączeń, skalowanie przez
   backplane/Redis). Używaj, gdy naprawdę potrzeba push.
5. **OData bez ograniczeń.** Odsłonięcie pełnego filtrowania = ryzyko drogich zapytań i wycieku struktury.

## Ćwiczenia

1. 🟢 **REST vs GraphQL.** Dla `GET /api/courses/{id}` (z modułami i lekcjami) napisz równoważne zapytanie
   GraphQL. Wskaż, gdzie GraphQL wygrywa (over/under-fetching).
2. 🟢 **Dobór narzędzia.** Do każdego scenariusza dobierz technologię: (a) czat na żywo, (b) wewnętrzne wołanie
   Payments→Courses, (c) publiczne proste API, (d) mobilny klient chcący różne kształty danych.
3. 🟡 **Prześledź SignalR w CP.** Otwórz `NotificationHub`, `SignalRNotificationService` i `useRealtime.ts`.
   Opisz: jakie grupy, skąd leci `SendAsync`, jak front się podłącza i co robi po evencie.
4. 🔴 **GraphQL bezpieczeństwo.** Wyjaśnij, czemu potrzebny jest depth/complexity limiting i jak DataLoader
   rozwiązuje N+1.

## Pytania kontrolne

1. Jaki problem REST (over/under-fetching) rozwiązuje GraphQL i jak?
2. Co to resolver i czym grozi N+1 w GraphQL?
3. Kiedy gRPC, a kiedy REST/GraphQL? Dlaczego gRPC nie jest domyślnym API dla przeglądarki?
4. Jaki problem rozwiązuje SignalR i czym różni się od pollingu?
5. Jak przekazuje się JWT do huba SignalR i dlaczego inaczej niż w REST?
6. Czym różni się OData od ręcznych parametrów i jakie ma ryzyka?

<details>
<summary>Rozwiązania</summary>

1. Over-fetching (dostajesz za dużo pól) i under-fetching (musisz zrobić kilka żądań). GraphQL: jeden endpoint,
   klient określa dokładny kształt odpowiedzi → jeden round-trip, dokładnie potrzebne pola.
2. Resolver to funkcja zwracająca dane pola. N+1: resolver odpalany osobno dla każdego elementu listy → wiele
   zapytań; rozwiązuje to DataLoader (batchuje i cache'uje w obrębie żądania).
3. gRPC — szybka komunikacja serwis↔serwis (binarny, HTTP/2, ścisły kontrakt). REST/GraphQL — API dla klientów,
   w tym przeglądarek. gRPC nie jest domyślny dla przeglądarki, bo ta nie woła go wprost (potrzeba gRPC-Web/proxy).
4. SignalR daje serwerowi możliwość **wypychania** danych do klienta przez stałe połączenie (WebSocket); polling
   to ciągłe odpytywanie przez klienta — obciąża sieć i ma opóźnienie.
5. Zwykle przez `accessTokenFactory` / query string, bo WebSocket nie zawsze przenosi nagłówki HTTP tak jak
   `fetch`/axios.
6. OData pozwala klientowi budować filtr/sort w URL (elastyczność), ręczne parametry ograniczają, co wolno
   (bezpieczeństwo, kontrola kosztu zapytań). Ryzyko OData: drogie zapytania i odsłonięcie struktury, jeśli nie
   ograniczysz.

</details>

## Idź dalej

➡️ **[15 — Zadania w tle i brokery](./15-zadania-w-tle-i-brokery.md)** — jak robić rzeczy poza cyklem żądania
HTTP (maile, reindeks, harmonogramy) i jak serwisy komunikują się asynchronicznie.

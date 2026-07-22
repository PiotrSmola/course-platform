# 12 — Cache: Memory Cache i Redis

> **Poziom:** 🟡→🔴 · **Czas:** ~75 min · **Wymaga:** [08](./08-dependency-injection.md), [07](./07-ef-core-zapytania-i-wydajnosc.md)

## Po co ci to

Baza i sieć są wolne w porównaniu z pamięcią. Jeśli **często** czytasz te same, rzadko zmieniające się dane
(lista kategorii, konfiguracja, popularne kursy), cache potrafi zmniejszyć obciążenie bazy o rzędy wielkości. To
temat „pod pracę i skalę". Course Platform **ma już** cache — `HybridCache` + Redis przez abstrakcję `IAppCache`
(`HybridAppCache` w Infrastructure). Pokażę koncepcje solidnie i wskażę, gdzie w projekcie to widać.

## Mostek z tego, co już znasz

- We froncie masz **Vue Query** — ono cache'uje odpowiedzi API po stronie klienta. Cache backendowy to ta sama
  idea, ale po stronie serwera (i współdzielony między wszystkimi userami).
- Znasz `Map`/obiekt jako podręczny cache w JS. `IMemoryCache` to to samo, tylko zarządzane (wygasanie, limity).

---

## Po co cache (i kiedy NIE)

Cache trzyma **kopię** danych bliżej i szybciej. Sięgasz po niego, gdy:
- dane są **czytane często**, a **zmieniają się rzadko** (kategorie, technologie, config),
- obliczenie/zapytanie jest **kosztowne** (agregacje, złożone joiny).

**Nie** cache'uj wszystkiego: dane, które muszą być zawsze świeże (stan konta, uprawnienia), albo prawie nigdy
nie czytane. Cache dokłada złożoność (invalidation!) — używaj tam, gdzie się opłaca.

---

## Memory Cache — wbudowany, najprostszy

`IMemoryCache` trzyma dane w **pamięci procesu** aplikacji. Najszybszy, zero dodatkowej infrastruktury.

```csharp
// rejestracja (Program.cs)
builder.Services.AddMemoryCache();

// użycie
public class CategoryService
{
    private readonly IMemoryCache _cache;
    private readonly IApplicationDbContext _context;
    public CategoryService(IMemoryCache cache, IApplicationDbContext context)
    { _cache = cache; _context = context; }

    public async Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken ct)
    {
        return await _cache.GetOrCreateAsync("categories", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);  // wygasa po 10 min
            return await _context.Categories.AsNoTracking()
                .Select(c => new CategoryDto(c.Id, c.Name, c.Slug, c.Description))
                .ToListAsync(ct);
        }) ?? [];
    }
}
```

Wzorzec **cache-aside**: „sprawdź cache → jak pusto, policz i zapisz → zwróć". `GetOrCreateAsync` robi to za ciebie.

| Pojęcie | Znaczenie |
|---------|-----------|
| **Absolute expiration** | Wygasa po X od zapisu (niezależnie od użycia) |
| **Sliding expiration** | Przedłuża się przy każdym odczycie (wygasa po X bezczynności) |
| **Cache key** | Unikalny string: `"categories"`, `"course:{id}"` |

### Ograniczenia Memory Cache

- **Tylko jedna instancja.** Masz 3 repliki API (Kubernetes) → każda ma **osobny** cache. Możliwe niespójności.
- **Znika po restarcie** procesu.
- **Nie współdzielony** między serwerami.

Dla małej aplikacji na jednej instancji — idealny. Dla skali poziomej — potrzebujesz cache rozproszonego.

> **Gdzie w Course Platform:** `GetCategoriesQuery` i `GetTechnologiesQuery` używają `IAppCache.GetOrCreateAsync`
> (klucze `cp:categories`, `cp:technologies`, TTL 1 h). Implementacja `HybridAppCache` łączy `IMemoryCache` z
> Redis (`IDistributedCache`) — lokalna szybkość + współdzielenie między instancjami. Inwalidacja przez tagi
> (`InvalidateTagAsync`) przy zmianach kategorii/technologii.

---

## Distributed Cache i Redis

Gdy masz **wiele instancji** API albo chcesz, by cache przetrwał restart — potrzebujesz warstwy **poza** procesem.
ASP.NET ma abstrakcję `IDistributedCache`, a najpopularniejszą implementacją jest **Redis**.

```csharp
// rejestracja
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "CoursePlatform:";
});
```

API jest inne niż Memory Cache — operujesz na **bajtach** (serializujesz obiekt sam, np. do JSON):

```csharp
public async Task<CourseDto?> GetCourseAsync(Guid id, CancellationToken ct)
{
    var key = $"course:{id}";
    var bytes = await _distributedCache.GetAsync(key, ct);
    if (bytes is not null)
        return JsonSerializer.Deserialize<CourseDto>(bytes);

    var course = await LoadFromDbAsync(id, ct);
    await _distributedCache.SetAsync(key,
        JsonSerializer.SerializeToUtf8Bytes(course),
        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) }, ct);
    return course;
}
```

### Czym jest Redis (jak dla laika)

**Redis** = bardzo szybka baza **klucz-wartość** trzymana w RAM (z opcjonalnym zapisem na dysk). Używana jako:

1. **Distributed cache** (powyżej),
2. **Session store** (sesje współdzielone między instancjami),
3. **Rate limiting** (liczniki żądań),
4. **Pub/Sub** (proste komunikaty),
5. **Kolejki** (listy/streams — prostsze niż Kafka),
6. **Distributed lock** (jeden worker robi jednorazowy job).

### Redis vs Memory Cache — tabela decyzyjna

| | Memory Cache | Redis |
|--|--------------|-------|
| Szybkość | Najszybszy (RAM procesu) | Bardzo szybki (sieć + RAM serwera Redis) |
| Współdzielony między instancjami | ❌ | ✅ |
| Przeżywa restart aplikacji | ❌ | ✅ (i restart Redisa z persystencją) |
| Dodatkowa infrastruktura | brak | Redis do postawienia i monitorowania |
| Kiedy | 1 instancja, proste dane | Produkcja, wiele replik, sesje, rate limit |

---

## Cache invalidation — „jeden z dwóch trudnych problemów w informatyce"

Cache jest łatwy do dodania, trudny do **utrzymania świeżym**. Gdy dane się zmienią, musisz usunąć/odświeżyć
wpis, inaczej użytkownicy widzą stare dane do wygaśnięcia TTL.

```csharp
// po zmianie kategorii przez admina:
_cache.Remove("categories");
// po zmianie kursu:
await _distributedCache.RemoveAsync($"course:{courseId}", ct);
```

Strategie:
- **TTL (time-to-live)** — najprostsza: akceptujesz, że dane są nieświeże do X minut. Dobre dla rzeczy, które
  mogą chwilę „poczekać".
- **Jawna inwalidacja** — przy zapisie usuwasz klucz. Precyzyjne, ale trzeba pamiętać o każdym miejscu zapisu.
- **Kombinacja** — TTL jako siatka bezpieczeństwa + jawna inwalidacja przy zmianach.

> **Uwaga na styk z frontem:** unieważnienie Vue Query (`invalidateQueries`) czyści cache **przeglądarki**. Cache
> **backendu** (Memory/Redis) to osobna warstwa — musisz go czyścić po stronie serwera przy zapisie. To dwa różne
> cache i łatwo o tym zapomnieć.

---

## Wzmianki (żebyś znał nazwy)

- **EF Core 2nd Level Cache** (np. `EFCoreSecondLevelCacheInterceptor`) — cache wyników zapytań EF między
  żądaniami. Rzadziej na start; najpierw `AsNoTracking`, dobre indeksy, potem Redis na gorących ścieżkach.
- **Memcached** — starszy, prostszy cache klucz-wartość. W .NET spotkasz go rzadziej niż Redis; Redis ma więcej
  struktur danych i funkcji.

---

## Pułapki

1. **Cache bez inwalidacji.** Użytkownicy widzą nieaktualne dane. Zawsze miej plan na świeżość (TTL i/lub jawne
   `Remove`).
2. **Memory Cache przy wielu instancjach.** Każda instancja ma swój cache → niespójności. Do skali poziomej Redis.
3. **Cache na dane per-użytkownik pod globalnym kluczem.** `"course:5"` jest OK; ale cache stanu zależnego od
   usera (np. „czy zapisany") pod globalnym kluczem wymiesza dane między userami.
4. **Cache'owanie wszystkiego.** Dane wymagające świeżości (uprawnienia, salda) — nie cache'uj bez potrzeby.
5. **Brak obsługi „miss".** Zawsze przewidź ścieżkę, gdy w cache pusto (policz z bazy i zapisz).

## Ćwiczenia

1. 🟢 **Kandydaci na cache.** Przejrzyj Queries w `Application/Features/**` i wskaż 2–3, które nadają się do
   cache'owania. Uzasadnij (często czytane, rzadko zmieniane). Porównaj z tym, co już cache'uje `IAppCache`.
2. 🟡 **Prześledź istniejący cache.** Otwórz `GetCategoriesQueryHandler` i `HybridAppCache`. Opisz flow
   `GetOrCreateAsync`: skąd bierze dane, jaki TTL, co robi Redis vs memory. *Done, gdy* potrafisz wytłumaczyć
   różnicę między hit a miss.
3. 🟡 **Inwalidacja.** Znajdź miejsca, gdzie wywoływane jest `InvalidateTagAsync` (np. po zmianie kategorii).
   Opisz, co by się stało bez inwalidacji.
4. 🔴 **Rozszerzenie cache.** Naszkicuj (na papierze) cache dla szczegółów kursu (`course:{id}`) przez ten sam
   `IAppCache`: klucz, TTL, tag do inwalidacji przy `UpdateCourse`. Jakie ryzyka vs brak cache?

## Pytania kontrolne

1. Kiedy warto cache'ować dane, a kiedy nie?
2. Czym różni się Memory Cache od Redisa? Podaj scenariusz dla każdego.
3. Co to cache-aside i jak realizuje go `GetOrCreateAsync`?
4. Absolute vs sliding expiration — różnica?
5. Dlaczego cache invalidation jest trudny i jakie masz strategie?
6. Dlaczego `invalidateQueries` w Vue nie wystarcza, gdy backend ma własny cache?

<details>
<summary>Rozwiązania</summary>

1. Warto: dane często czytane i rzadko zmieniane lub kosztowne do policzenia. Nie: dane wymagające świeżości
   (uprawnienia, salda) albo prawie nieczytane — cache dokłada złożoność (inwalidacja).
2. Memory Cache: RAM procesu, najszybszy, ale osobny na instancję i znika po restarcie (dobry dla 1 instancji).
   Redis: poza procesem, współdzielony między replikami, przeżywa restart (dobry na produkcję/skalę, sesje,
   rate limit).
3. Cache-aside: „sprawdź cache → miss → policz z bazy i zapisz → zwróć". `GetOrCreateAsync` robi to w jednym
   wywołaniu (fabryka liczy wartość tylko przy miss).
4. Absolute: wygasa po ustalonym czasie od zapisu. Sliding: wygasa po czasie **bezczynności** (każdy odczyt
   przedłuża).
5. Bo trzeba pamiętać o każdym miejscu zapisu danych i tam czyścić cache; ryzyko nieświeżych danych. Strategie:
   TTL, jawna inwalidacja przy zmianie, kombinacja obu.
6. Bo `invalidateQueries` czyści cache przeglądarki (Vue Query), a cache backendu (Memory/Redis) to osobna
   warstwa — trzeba go czyścić po stronie serwera przy zapisie.

</details>

## Idź dalej

➡️ **[13 — Wyszukiwanie i Elasticsearch](./13-wyszukiwanie-i-elasticsearch.md)** — od `LIKE` w SQL po dedykowany
silnik wyszukiwania: kiedy i dlaczego przejść.

# 15 — Zadania w tle i brokery wiadomości

> **Poziom:** 🟡→🔴 · **Czas:** ~75 min · **Wymaga:** [08](./08-dependency-injection.md), [09](./09-architektura-clean-cqrs-mediatr.md)

## Po co ci to

Nie wszystko mieści się w cyklu żądania HTTP. Wysyłka maila, generowanie raportu, reindeks wyszukiwarki,
czyszczenie danych, harmonogram „co noc o 3:00" — to zadania w tle. A gdy system rośnie w wiele serwisów,
komunikują się one **asynchronicznie** przez brokery wiadomości. Musisz znać `BackgroundService`, Hangfire i
ideę RabbitMQ/MassTransit — pojawiają się w pracy i na rozmowach.

## Mostek z tego, co już znasz

- W Laravelu miałeś **kolejki i joby** (`dispatch(new SendEmail(...))`) oraz **scheduler** (`schedule->daily()`).
  .NET ma odpowiedniki: `BackgroundService`/Hangfire (joby, harmonogram) i MassTransit+RabbitMQ (kolejki/eventy).
- Idea „nie rób wolnej rzeczy w trakcie żądania — oddeleguj" jest identyczna.

---

## Dlaczego nie robić wszystkiego w handlerze

Rozważ: po zapisaniu na kurs chcesz wysłać maila powitalnego. Kuszące, by zrobić to w `EnrollCommandHandler`. Ale:
- **Wolne** — SMTP potrafi trwać sekundy; użytkownik czeka na odpowiedź HTTP.
- **Kruche** — jeśli SMTP padnie, cała operacja „zapisz na kurs" się wywala (albo mail ginie).
- **Nie skaluje się** — długie operacje blokują wątki żądań.

Rozwiązanie: handler robi **tylko** rzecz krytyczną (zapis do bazy) i **deleguje** resztę (mail) do tła.

---

## `BackgroundService` / Hosted Services — wbudowane w .NET

`BackgroundService` to klasa z pętlą działającą w tle przez całe życie aplikacji (z `Microsoft.Extensions.Hosting`):

```csharp
public class EmailBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    public EmailBackgroundService(IServiceProvider services) => _services = services;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = _services.CreateAsyncScope();          // ⬅ własny scope!
            var queue = scope.ServiceProvider.GetRequiredService<IEmailQueue>();
            var job = await queue.DequeueAsync(stoppingToken);
            if (job is not null) await ProcessAsync(job);
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}

// Program.cs
builder.Services.AddHostedService<EmailBackgroundService>();
```

### ⚠️ Scoped w tle — klasyczna pułapka

`BackgroundService` jest **Singletonem** (jeden na aplikację). Ale `DbContext` jest **Scoped** (na żądanie —
[08](./08-dependency-injection.md)). W tle **nie ma żądania**, więc nie możesz po prostu wstrzyknąć `DbContext` do
konstruktora (captive dependency!). Musisz **utworzyć scope ręcznie** na każdą jednostkę pracy:

```csharp
await using var scope = _services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
```

To najczęstszy błąd juniora przy pracy w tle — zapamiętaj.

`IHostedService` to niższy poziom (`StartAsync`/`StopAsync`); `BackgroundService` to wygodna baza z pętlą
`ExecuteAsync`.

> Course Platform robi drobne „przy starcie" rzeczy w `Program.cs` (migracje, seed ról) właśnie w ręcznie
> utworzonym scope — ta sama zasada.

---

## Hangfire — kolejki i harmonogram z dashboardem

Gdy `BackgroundService` to za mało (chcesz cron, retry, dashboard, persystencję jobów), sięgasz po bibliotekę.
Najpopularniejsza to **Hangfire**:

```csharp
builder.Services.AddHangfire(c => c.UsePostgreSqlStorage(connectionString));
builder.Services.AddHangfireServer();

// wrzuć job "fire and forget"
BackgroundJob.Enqueue(() => _emailService.SendWelcome(userId));

// job cykliczny (cron)
RecurringJob.AddOrUpdate("cleanup-tokens", () => _tokens.CleanupExpired(), Cron.Daily);
```

Hangfire daje **dashboard** pod `/hangfire` — widzisz joby, te nieudane, retry. Joby są **persystowane** (w bazie),
więc przeżyją restart aplikacji. Bardzo popularny w .NET.

### Quartz.NET i Coravel (alternatywy)

- **Quartz.NET** — rozbudowany scheduler (cron, triggery, klaster). Potężny, mniej „baterie w zestawie" niż UI
  Hangfire.
- **Coravel** — lekki, „w stylu Laravela": `scheduler.Schedule<ReindexJob>().Daily();`. Miły dla kogoś z tłem
  Laravelowym.

| | BackgroundService | Hangfire | Quartz |
|--|-------------------|----------|--------|
| Prosta pętla | ✅ | overkill | overkill |
| Cron | ręcznie | ✅ | ✅ |
| Dashboard/UI | ❌ | ✅ | plugin |
| Kolejka + retry + persystencja | sam piszesz | ✅ | ✅ |
| Złożoność | niska | średnia | wyższa |

---

## Brokery wiadomości — komunikacja między serwisami

Gdy system to nie monolit, lecz kilka serwisów (Courses, Payments, Notifications), komunikują się **asynchronicznie**
przez broker. Serwis A publikuje **event**, broker dostarcza go do zainteresowanych serwisów — bez czekania w HTTP.

```
[Courses API] --publish EnrolledEvent--> [RabbitMQ/Kafka] --consume--> [Email Service]
                                                                 └----> [Search Service (reindeks)]
```

| Broker | Charakterystyka |
|--------|-----------------|
| **RabbitMQ** | Klasyczna kolejka, łatwy start, dojrzały |
| **Kafka** | Log eventów, ogromna przepustowość, stream processing |
| **Azure Service Bus** | Zarządzany broker w Azure |

### MassTransit — abstrakcja .NET nad brokerem

Zamiast pisać pod surowy RabbitMQ, używasz **MassTransit** (upraszcza publish/consume, retry, serializację):

```csharp
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<EnrolledEmailConsumer>();
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("localhost", "/", h => { h.Username("guest"); h.Password("guest"); });
        cfg.ConfigureEndpoints(ctx);
    });
});

public record EnrolledEvent(Guid UserId, Guid CourseId);

public class EnrolledEmailConsumer : IConsumer<EnrolledEvent>
{
    public async Task Consume(ConsumeContext<EnrolledEvent> context) { /* wyślij mail */ }
}
```

W handlerze: `await _publishEndpoint.Publish(new EnrolledEvent(userId, courseId));` — i tyle; handler nie czeka na
maila. RabbitMQ i MassTransit są „niebieskie" na roadmapie — to zestaw, który realnie spotkasz.

### 🔴 Outbox pattern — niezawodność

Problem: co, jeśli **zapiszesz** enrollment do bazy, ale **publikacja eventu** do RabbitMQ się nie powiedzie (albo
odwrotnie)? Rozwiązanie: **Outbox** — zapisujesz event do tabeli w **tej samej transakcji** co dane; osobny worker
odczytuje tabelę i publikuje do brokera. Gwarantuje „albo jedno i drugie, albo nic". MassTransit ma wbudowany
outbox. To temat rozproszony (🔴) — znaj nazwę i po co.

---

## API Gateway (Ocelot, YARP) — jedna brama

W architekturze mikroserwisowej klient nie powinien znać adresów wszystkich serwisów. **API Gateway** to jeden
publiczny punkt wejścia, który routuje do serwisów wewnętrznych (+ auth, rate limit, SSL na brzegu):

```
https://api.firma.pl/courses/*   → Courses Service
https://api.firma.pl/payments/*  → Payments Service
```

- **Ocelot** — gateway konfigurowany w JSON (trasy upstream→downstream).
- **YARP** — reverse proxy Microsoftu, konfigurowany w kodzie/`appsettings`; aktywnie rozwijany, częsty w stacku
  MS/Azure.

Oba są „niebieskie" — znaj oba, umiej powiedzieć różnicę. **Monolit (Course Platform) gatewaya nie potrzebuje.**

---

## Kiedy monolit wystarcza (ważna perspektywa)

Nie wprowadzaj brokerów, gatewaya i mikroserwisów „bo brzmią pro". Na start kariery i dla większości produktów
**monolit warstwowy wystarcza** (Course Platform): jeden deploy, jedna baza, prostszy debug. Brokery/tło dokładasz,
gdy: masz realnie długie operacje (maile, raporty), potrzebujesz harmonogramu, albo rozdzielasz system na serwisy.
Umiejętność powiedzenia „tego jeszcze nie potrzebujemy" to też kompetencja.

---

## Pułapki

1. **`DbContext` bez scope w tle.** `BackgroundService` jest Singletonem — twórz scope ręcznie (`CreateAsyncScope`).
2. **Ciężka praca w handlerze HTTP.** Maile/raporty/reindeks → tło, nie w cyklu żądania.
3. **Brak retry/obsługi błędów w jobach.** Zewnętrzne systemy (SMTP, broker) padają — przewidź ponowienie.
4. **Broker „na wyrost".** Monolit z jedną instancją nie potrzebuje Kafki. Dokładaj, gdy jest realna potrzeba.
5. **Publikacja eventu bez outboxa przy wymogu niezawodności.** Ryzyko „zapisane, event zgubiony". Wtedy outbox.

## Ćwiczenia

1. 🟢 **Zidentyfikuj kandydatów.** Wypisz 3 operacje w Course Platform, które warto by przenieść do tła
   (podpowiedź: maile, reindeks, czyszczenie). Uzasadnij.
2. 🟢 **Scope w tle.** Wyjaśnij, dlaczego w `BackgroundService` nie można wstrzyknąć `DbContext` do konstruktora i
   jak to obejść.
3. 🟡 **Projekt joba.** Zaprojektuj `BackgroundService`, który raz na dobę czyści wygasłe dane (np. tokeny).
   Napisz szkielet z pętlą, `Task.Delay` i ręcznym scope. Kiedy wolałbyś tu Hangfire?
4. 🔴 **Event po enrollmencie.** Naszkicuj przepływ „po `EnrollCommand` wyślij mail" przez MassTransit+RabbitMQ:
   event, publikacja w handlerze, consumer. Gdzie i po co dołożyłbyś outbox?

## Pytania kontrolne

1. Dlaczego nie wysyłać maila synchronicznie w handlerze HTTP?
2. Dlaczego w `BackgroundService` trzeba ręcznie tworzyć scope dla `DbContext`?
3. Kiedy wybierzesz `BackgroundService`, a kiedy Hangfire?
4. Do czego służy broker wiadomości i czym różni się od wywołania HTTP między serwisami?
5. Co robi MassTransit i jak wygląda publish/consume?
6. Jaki problem rozwiązuje Outbox pattern?

<details>
<summary>Rozwiązania</summary>

1. Bo to wolne i kruche — użytkownik czeka na SMTP, a awaria maila może wywalić całą operację. Handler robi rzecz
   krytyczną (zapis), resztę deleguje do tła.
2. Bo `BackgroundService` jest Singletonem, a `DbContext` Scoped — wstrzyknięcie do konstruktora to captive
   dependency. Tworzysz scope na jednostkę pracy (`CreateAsyncScope`) i z niego pobierasz `DbContext`.
3. `BackgroundService` — prosta pętla/lekkie zadanie. Hangfire — gdy potrzebujesz cron, retry, dashboardu,
   persystencji jobów.
4. Broker dostarcza wiadomości/eventy asynchronicznie — nadawca nie czeka na odbiorcę i nie musi znać jego
   adresu; HTTP jest synchroniczne i wiąże serwisy „na sztywno". Broker daje luźne sprzężenie i odporność.
5. MassTransit to abstrakcja nad brokerem (RabbitMQ/Azure SB). Publish: `_publishEndpoint.Publish(event)`;
   consume: klasa `IConsumer<TEvent>` z metodą `Consume`.
6. Zapewnia atomowość „zapis danych + publikacja eventu": event trafia do tabeli w tej samej transakcji co dane,
   a worker publikuje go do brokera — nie zgubisz eventu ani nie wyślesz go bez zapisu.

</details>

## Idź dalej

➡️ **[16 — Testowanie](./16-testowanie.md)** — jak upewnić się, że kod działa i nie psuje się przy zmianach:
testy jednostkowe, integracyjne i E2E.

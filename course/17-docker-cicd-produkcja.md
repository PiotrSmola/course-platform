# 17 — Docker, CI/CD, produkcja

> **Poziom:** 🟡→🔴 · **Czas:** ~75 min · **Wymaga:** [03](./03-pierwsza-aplikacja-host-program-cs.md), [16](./16-testowanie.md)

## Po co ci to

Kod, który działa „u ciebie", musi zadziałać na serwerze — identycznie. **Docker** to gwarantuje (pakuje
aplikację z zależnościami w obraz), a **CI/CD** automatyzuje budowanie, testy i wdrożenie. Znasz już Docker z
uruchamiania Course Platform — tutaj zrozumiesz, **jak** to działa i jak wygląda droga na produkcję.

## Mostek z tego, co już znasz

- Odpalałeś `docker compose up` w tym projekcie. Teraz zajrzymy pod maskę: obrazy, warstwy, multi-stage build.
- CI/CD ≈ „automatyczny asystent", który po każdym push robi to, co ty robisz ręcznie: `build` → `test` → `deploy`.
- W Laravelu deployowałeś np. przez Forge/pipeline — idea „zbuduj artefakt, wypchnij" jest identyczna.

---

## Po co Docker w .NET

Problem „u mnie działa" bierze się z różnic środowisk (wersja .NET, biblioteki systemowe, konfiguracja). Docker
pakuje aplikację + jej środowisko w **obraz** — ten sam obraz uruchamiasz na dev, stage i prod.

Course Platform w dev to pełny stack Compose (nie tylko rdzeń):

| Kontener | Usługa | Port | Rola |
|----------|--------|------|------|
| `cp_api` | `api` | 8080 | Backend (`dotnet watch run` — auto-reload w dev) |
| `cp_frontend` | `frontend` | 5173 | Frontend Vue (Vite) |
| `cp_db` | `db` | 5432 | PostgreSQL 16 |
| `cp_redis` | `redis` | 6379 | Redis (HybridCache) |
| `cp_elasticsearch` | `elasticsearch` | 9200 | Elasticsearch (search, gdy włączony) |
| `cp_minio` | `minio` | 9000/9001 | Object storage (wideo, miniatury, certyfikaty) |
| `cp_minio_init` | `minio_init` | — | Inicjalizacja bucketu |
| `cp_mailhog` | `mailhog` | 8025 | Maile dev |
| `cp_aspire` | `aspire-dashboard` | 18888/18889 | OpenTelemetry / Aspire Dashboard |
| `cp_stripe_cli` | `stripe_cli` | — | Opcjonalny profil `stripe` (webhook forward) |

Kluczowa rzecz (znasz z [00](./00-jak-korzystac-z-kursu-i-mapa.md)): z kontenera `api` host bazy to **`db`** (nazwa
usługi Compose), nie `localhost` — bo każdy kontener to osobny „komputer" w sieci Compose.

---

## Obraz, warstwy, kontener (jak dla laika)

- **Obraz (image)** — „zamrożony" szablon: system + .NET + twoja aplikacja. Niezmienny.
- **Kontener** — działająca instancja obrazu (jak obiekt z klasy).
- **Warstwy (layers)** — obraz budowany jest warstwami; Docker cache'uje warstwy, więc jeśli nie zmieniłeś
  zależności, kolejny build jest szybki.
- **Dockerfile** — przepis na obraz.

---

## Multi-stage Dockerfile (produkcja)

W dev używasz SDK (kompilator, `dotnet watch`). Na produkcji chcesz **mały, bezpieczny** obraz — bez SDK, tylko
runtime. Rozwiązanie: **multi-stage build** (buduj w jednym etapie, kopiuj wynik do drugiego):

```dockerfile
# Etap 1: build (pełne SDK)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/CoursePlatform.API -c Release -o /app

# Etap 2: runtime (samo ASP.NET runtime — mniejszy, mniejsza powierzchnia ataku)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "CoursePlatform.API.dll"]
```

Efekt: obraz produkcyjny nie zawiera kompilatora ani źródeł — jest mniejszy i bezpieczniejszy. To standardowy
wzorzec dla .NET.

> **Frontend na produkcji** buduje się analogicznie: etap `node` robi `npm run build`, etap `nginx` serwuje
> statyczny `dist/` (+ fallback na `index.html` dla routingu SPA). W dev jedzie Vite dev server (jak teraz).

---

## Konfiguracja i sekrety na produkcji

Znasz to z [03](./03-pierwsza-aplikacja-host-program-cs.md): sekrety wstrzykujesz **środowiskiem**, nie w obrazie.
Na produkcji connection string i `Jwt:Key` pochodzą ze zmiennych środowiskowych / menedżera sekretów (Azure Key
Vault, AWS Secrets Manager), nigdy z commita. Course Platform waliduje kluczowe ustawienia przy starcie (fail
fast) — to szczególnie ważne na produkcji.

Produkcyjny checklist konfiguracji:
- **HTTPS** wymuszony (certyfikat; TLS terminowany na proxy/nginx albo w aplikacji).
- Sekrety ze środowiska/menedżera, nie z repo.
- Poziom logów `Information`/`Warning` ([11](./11-logowanie-serilog.md)), logi do trwałego sinka.
- Swagger wyłączony poza dev ([05](./05-kontrolery-minimal-api-mvc-blazor.md)).

---

## Migracje bazy na produkcji (🔴 ważny niuans)

Course Platform woła `context.Database.MigrateAsync()` **przy starcie** (poza środowiskiem Testing) — wygodne
lokalnie. Na produkcji z **wieloma instancjami** to bywa ryzykowne: kilka instancji mogłoby próbować migrować
naraz (wyścig), a start aplikacji zależy od dostępności/poprawności migracji.

Dojrzalsze podejście produkcyjne: **migracja jako osobny krok w CI/CD** (przed wdrożeniem nowej wersji), a
aplikacja startuje na gotowej bazie. Dla nauki i pojedynczej instancji auto-migracja jest OK — ale wiedz, że to
świadomy kompromis (to samo dotyczy seedu, który w projekcie jest za flagą `Dev:Seed`).

---

## CI/CD — automatyzacja

**CI (Continuous Integration):** po każdym push/PR automat buduje i testuje kod. **CD (Continuous Delivery/Deployment):**
automat buduje obraz i wdraża go na środowisko.

Przykład pipeline (GitHub Actions, uproszczony — „niebieskie" na roadmapie):

```yaml
name: ci
on: [push, pull_request]
jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '9.0.x' }
      - run: dotnet restore
      - run: dotnet build --no-restore -c Release
      - run: dotnet test --no-build -c Release
      # dalej (CD): docker build & push, deploy na Azure/AWS
```

**PR check:** build + testy **muszą przejść**, zanim ktoś zmerguje. To siatka bezpieczeństwa — nie wpuszcza
zepsutego kodu do głównej gałęzi. Alternatywy dla GitHub Actions: Azure Pipelines, GitLab CI, CircleCI (na
roadmapie jako *alternative*).

---

## Kubernetes — panorama

Gdy masz wiele kontenerów, replik i potrzebujesz automatycznego skalowania, load balancingu, self-healingu —
wchodzi **Kubernetes (K8s)**. Orkiestruje kontenery: uruchamia „pody" z twoich obrazów, restartuje padłe, rozkłada
ruch, zarządza sekretami i configiem.

Junior: wiedz, że **K8s uruchamia kontenery Docker w skali** i po co (repliki, skalowanie, HA). Szczegóły (Deployment,
Service, Ingress, HPA) poznasz, gdy realnie wejdziesz w infrastrukturę — to nie jest fundament backendowca na
start. Course Platform tego nie używa (Compose wystarcza lokalnie).

---

## Dokumentacja API: Swagger vs Scalar

Course Platform używa **Swagger** (`AddSwaggerGen` + `UseSwaggerUI` w dev) — interaktywna dokumentacja i „poligon"
do wołania endpointów (widziałeś ją na `:8080/swagger`). To wciąż standard.

**Scalar** (🔵 na roadmapie) to nowocześniejszy UI dla OpenAPI:

```csharp
app.MapOpenApi();
app.MapScalarApiReference();   // NuGet: Scalar.AspNetCore
```

Ładniejszy UI, lepsze UX; rośnie w projektach .NET 8/9. Junior: umiesz otworzyć dokumentację API i wywołać
endpoint z tokenem — narzędzie jest drugorzędne, ważna jest umiejętność.

---

## Pułapki

1. **`localhost` zamiast nazwy usługi w Compose.** Z kontenera baza to `db`, nie `localhost`.
2. **SDK w obrazie produkcyjnym.** Bez multi-stage obraz jest ogromny i ma zbędną powierzchnię ataku.
3. **Sekrety w obrazie/repo.** Zawsze przez środowisko/menedżer sekretów.
4. **Auto-migracja przy wielu instancjach.** Ryzyko wyścigu; na produkcji migruj osobnym krokiem CI/CD.
5. **Brak testów w CI.** Pipeline bez `dotnet test` przepuszcza zepsuty kod do głównej gałęzi.
6. **Swagger na produkcji.** Odsłania strukturę API — zostaw go tylko w dev.

## Ćwiczenia

1. 🟢 **Zwiedź Compose.** Otwórz `docker-compose.yml` i wypisz usługi, porty i nazwy kontenerów. Gdzie ustawiony
   jest host bazy dla `api`?
2. 🟢 **Uruchom i sprawdź.** `docker compose up -d`, potem `docker compose ps` i `docker compose logs api`.
   Znajdź w logach linię startu Kestrela (nasłuch na 8080).
3. 🟡 **Multi-stage.** Wyjaśnij, dlaczego produkcyjny Dockerfile ma dwa etapy i co zawiera obraz końcowy vs obraz
   budujący. Co byś stracił, robiąc jeden etap na SDK?
4. 🟡 **Pipeline.** Napisz szkic GitHub Actions dla Course Platform: restore → build → test. Który krok jest
   „bramką" PR i dlaczego?
5. 🔴 **Migracje na prod.** Opisz, jak przenieść migracje ze startu aplikacji do osobnego kroku CI/CD i dlaczego
   to bezpieczniejsze przy wielu instancjach.

## Pytania kontrolne

1. Jaki problem rozwiązuje Docker i czym różni się obraz od kontenera?
2. Po co multi-stage Dockerfile i co jest w obrazie produkcyjnym?
3. Dlaczego z kontenera `api` host bazy to `db`, a nie `localhost`?
4. Czym jest CI, a czym CD? Co powinno być „bramką" PR?
5. Jakie ryzyko niesie auto-migracja przy starcie na produkcji i jak je zmniejszyć?
6. Czym Kubernetes różni się od samego Dockera?

<details>
<summary>Rozwiązania</summary>

1. Docker pakuje aplikację z jej środowiskiem, eliminując „u mnie działa". Obraz to niezmienny szablon; kontener
   to działająca instancja tego obrazu.
2. Żeby obraz produkcyjny nie zawierał SDK/źródeł — buduje się w etapie z SDK, a wynik kopiuje do lekkiego obrazu
   runtime (`aspnet`). Efekt: mniejszy, bezpieczniejszy obraz.
3. Bo w sieci Compose każdy kontener to osobny host; `api` łączy się z bazą po nazwie usługi (`db`), a `localhost`
   wskazywałby na sam kontener `api`.
4. CI: automatyczny build+test po push/PR. CD: automatyczne budowanie obrazu i wdrożenie. Bramką PR powinny być
   przechodzące build + testy (nie wpuszczają zepsutego kodu).
5. Wiele instancji może migrować naraz (wyścig), a start zależy od migracji. Zmniejszasz to, wykonując migracje
   osobnym krokiem CI/CD przed wdrożeniem, tak by aplikacja startowała na gotowej bazie.
6. Docker uruchamia pojedyncze kontenery; Kubernetes orkiestruje je w skali — repliki, load balancing,
   self-healing, skalowanie, zarządzanie configiem/sekretami.

</details>

## Idź dalej

➡️ **[18 — Integracja z frontendem (Vue)](./18-integracja-z-frontendem-vue.md)** — jak backend ASP.NET i Twój
front Vue realnie się dogadują.

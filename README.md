# Course Platform

Online course platform — ASP.NET Core 9 (Clean Architecture) + Vue 3 + TypeScript. Portfolio / learning project with real integrations (MinIO, Stripe, Redis, Elasticsearch, SignalR, Aspire/OTEL).

## Architecture highlights

- **Backend:** Clean Architecture (API → Application → Domain; Infrastructure → Application/Domain), CQRS via MediatR, FluentValidation, EF Core + PostgreSQL, ASP.NET Identity + JWT/refresh tokens
- **AuthZ:** role policies (`AdminOnly`, `InstructorOrAdmin`) + resource policy `ManageCourse` (owner or admin); lesson access via enrollment **or** active All-access subscription, with progress gates
- **Frontend:** Vue 3 Composition API, Pinia (auth), TanStack Vue Query (server state), VeeValidate + Zod, SCSS (7-1)
- **Infra:** Docker Compose — API, Vue, Postgres, MinIO, Redis, Elasticsearch, MailHog, Aspire Dashboard (OTEL), optional Stripe CLI

## Prerequisites

- Docker Desktop
- Git

## Quick start

```bash
cp .env.example .env
# Edit .env — set JWT_KEY to a unique value (at least 32 characters)

cp src/CoursePlatform.API/appsettings.Development.json.example \
   src/CoursePlatform.API/appsettings.Development.json

docker compose up -d
```

- API: http://localhost:8080
- Frontend: http://localhost:5173
- Swagger (dev only): http://localhost:8080/swagger
- MailHog UI: http://localhost:8025 (password reset / confirm email / waitlist publish mail in dev)
- MinIO console: http://localhost:9001
- Aspire Dashboard (OTEL): http://localhost:18888

Official local dev path is **Docker Compose**. `launchSettings.json` ports (5089/7079) are for optional IDE runs outside containers.

## Features (demo checklist)

| Role | Capabilities |
|------|----------------|
| Student | Register/login, confirm email, reset password, browse/filter courses, wishlist, waitlist on coming-soon (`Hidden`) courses, enroll free / Stripe checkout or **All-access** monthly subscription, learn with playback speed + resume, progress gates (lesson order + quiz pass), lesson Q&A, MCQ quizzes, downloadable lesson materials, reviews, certificates, profile (stats / purchases / delete account) |
| Instructor | Create/edit courses, modules/lessons, thumbnail & video upload, lesson resources, publish/hide/delete own courses, Q&A moderation, dashboard + analytics (completion, drop-off, revenue) |
| Admin | Users & roles, course status, moderate reviews, coupons, Q&A moderation, audit log, Elastic reindex, revenue overview |

Also public: **learning paths**, **business plans**, certificate verification. Realtime toasts via SignalR (purchase, certificate, reindex) — no notification inbox.

Error pages: `/error/403`, `/error/404`, `/error/500`, `/error/501` (+ SPA catch-all → 404).

Aspire Dashboard (OpenTelemetry): http://localhost:18888 when the `aspire` service is up.

## Configuration

### Secrets

JWT signing key is **not** stored in committed config files.

| Source | Purpose |
|--------|---------|
| `.env` (gitignored) | `JWT_KEY` — interpolated into `docker-compose.yml` as `Jwt__Key` |
| `appsettings.Development.json` (gitignored) | Connection string, `Dev:Seed`, dev CSP |
| `appsettings.Development.json.example` | Template copied on first setup |

ASP.NET Core configuration provider chain: environment variables override appsettings.

`Frontend:BaseUrl` and `Cors:Origins` drive password-reset / confirm-email links and CORS (defaults: `http://localhost:5173`).

### Database seed

Seed runs automatically on API startup when **both** conditions are true:

- `ASPNETCORE_ENVIRONMENT=Development`
- `Dev:Seed=true` in `appsettings.Development.json`

Production never seeds, even if `Dev:Seed` is set accidentally.

How it works (`ApplicationDbContextSeed`, invoked from `Program.cs`):

1. Every startup runs EF Core migrations (`Database.MigrateAsync()`) and seeds the three roles (`Student`, `Instructor`, `Admin`) — regardless of `Dev:Seed`.
2. With seeding enabled, `SeedAsync` fills: users, categories, technologies, courses (with modules and lessons), enrollments, reviews, lesson progress, user statistics, learning paths and business plans.
3. Every step is idempotent — it skips when data already exists (e.g. courses seed only into an empty `Courses` table), so restarting the API never duplicates data. It also means the seed **won't refresh existing rows** — to get fresh seed data you must wipe the database first.

Test accounts created by the seed:

| Role | Email | Password |
| ---- | ----- | -------- |
| Admin | `admin@courseplatform.com` | `Admin123!` |
| Instructor | `instructor@courseplatform.com` | `Instructor123!` |
| Instructor | `anna.kowalska@courseplatform.com` | `Instructor123!` |
| Instructor | `marcin.wisniewski@courseplatform.com` | `Instructor123!` |
| Student | `piotr.nowak@courseplatform.com` | `Student123!` |
| Student | `karolina.zielinska@courseplatform.com` | `Student123!` |
| Student | `tomasz.wojcik@courseplatform.com` | `Student123!` |
| Student | `monika.kaminska@courseplatform.com` | `Student123!` |
| Student | `jakub.lewandowski@courseplatform.com` | `Student123!` |

#### Re-running the seed manually

Recreate the database and restart the API — migrations and seed run on startup:

```bash
docker compose exec db psql -U postgres -c "DROP DATABASE courseplatform WITH (FORCE);"
docker compose exec db psql -U postgres -c "CREATE DATABASE courseplatform;"
docker compose restart api
```

Full reset including the Docker volume (also wipes pgAdmin-style local state):

```bash
docker compose down
docker volume rm course-platform_pgdata
docker compose up -d
```

Seeded courses have empty `ThumbnailObjectKey` — the frontend shows a gradient fallback until an instructor uploads a real thumbnail (files live in MinIO, not in the seed).

## Security

### Headers (API)

`SecurityHeadersMiddleware` sets CSP per environment from config:

- **Development** — allows `localhost:8080`, `localhost:5173`, Swagger inline scripts
- **Production** — strict whitelist (`placeholder.local` for seeded learning-path thumbnails; no blanket `https:`)

Additional directives: `frame-ancestors 'none'`, `object-src 'none'`, `base-uri 'self'`, `form-action 'self'`.

CSP on API responses is defence-in-depth (Swagger, error pages, iframe protection). The Vue SPA will need its own CSP headers when deployed behind nginx/Nuxt.

### JWT validation at startup

`Program.cs` fails fast if `Jwt:Key` is missing, too short (< 32 bytes), still set to the `.env.example` placeholder, or if `Jwt:Issuer` / `Jwt:Audience` are missing.

## File storage — MinIO (Etap 2)

Course thumbnails, lesson videos, and **lesson download materials** (PDF, ZIP, source archives, etc.) are stored in MinIO (S3-compatible object storage), never in the database. The bucket is private (`mc anonymous set none`) — every read and write goes through **presigned URLs** issued by the API after authorization checks.

### Services

| Service | Purpose | Port |
| ------- | ------- | ---- |
| `minio` | S3 API used by backend and browser | 9000 |
| `minio` (console) | Web UI, login with `MINIO_ROOT_USER` / `MINIO_ROOT_PASSWORD` | 9001 |
| `minio_init` | One-shot init: creates the bucket, disables anonymous access | — |

Configuration lives in `.env` (`MINIO_ROOT_USER`, `MINIO_ROOT_PASSWORD`, `MINIO_BUCKET`, `MINIO_PUBLIC_ENDPOINT`). The API talks to MinIO internally via `http://minio:9000` and presigns URLs against the public endpoint (`http://localhost:9000` in dev) so the browser can use them directly.

### Upload flows (instructor/admin only)

- **Thumbnail** — `POST /api/courses/{id}/thumbnail/presign` → browser `PUT`s the file straight to MinIO → `POST .../thumbnail/confirm` verifies the object (max 5 MB) and stores the object key.
- **Video (multipart)** — initiate upload → presign each 15 MB part → `PUT` parts to MinIO → complete (max 200 MB; oversized objects are deleted server-side). On failure the frontend aborts the multipart upload.
- Allowed content types: `image/jpeg`, `image/png`, `image/webp` for thumbnails; `video/mp4`, `video/webm`, `video/quicktime` for videos.
- Object keys are deterministic (`courses/{courseId}/thumbnail/source`, `courses/{courseId}/lessons/{lessonId}/video/source`) and are **never accepted from the client**.

### Read access

- **Thumbnails** — course DTOs return a presigned `thumbnailUrl` (15 min expiry); `null` when no thumbnail was uploaded.
- **Videos** — `GET /api/courses/{courseId}/lessons/{lessonId}/video` returns a presigned URL (6 h expiry) only for enrolled students, active All-access subscribers, the course owner or an admin (resource-based authorization + progress gates when applicable).
- **Lesson resources** — list/download endpoints under `/api/courses/{courseId}/lessons/{lessonId}/resources` (same access rules as learning content).

### Exporting stored files (e.g. sharing a lesson video)

Objects in the MinIO volume are not plain files on disk (each object is a directory with `xl.meta` and `part.N` chunks), so don't copy them from the volume directly — download them through MinIO, which reassembles the object into a regular file.

**Step 1 — find the object key.** Every lesson row stores its key in `VideoObjectKey`:

```bash
docker compose exec db psql -U postgres -d courseplatform -c \
  "SELECT l.\"Id\", l.\"Title\", l.\"VideoObjectKey\" FROM \"Lessons\" l WHERE l.\"VideoObjectKey\" IS NOT NULL;"
```

**Step 2 — download the file.** Run from the directory where you want the file saved (uses the `minio/mc` image already present in the stack; credentials are your `.env` values):

```bash
docker run --rm --network course-platform_default -v .:/out --entrypoint sh minio/mc -c "
  mc alias set local http://minio:9000 <MINIO_ROOT_USER> <MINIO_ROOT_PASSWORD> &&
  mc cp local/course-platform/<VideoObjectKey> /out/lesson-video.mp4"
```

Example key: `courses/<courseId>/lessons/<lessonId>/video/source`. Thumbnails work the same way (`courses/<courseId>/thumbnail/source`).

**Alternative (GUI):** open the MinIO console at `http://localhost:9001`, log in with `MINIO_ROOT_USER` / `MINIO_ROOT_PASSWORD`, browse the `course-platform` bucket following the object key path and click **Download**.

The result is a standard media file (e.g. `video/mp4`) — share it like any other file. Note that this path bypasses application authorization (it uses storage root credentials), so it is a dev/ops tool, not something to expose to users.

### Incomplete upload cleanup

`MINIO_API_STALE_UPLOADS_EXPIRY: 72h` on the `minio` service — multipart uploads abandoned mid-way (e.g. closed browser tab) are purged automatically after 3 days.

### CSP note

Production CSP must include the MinIO/public CDN origin in `img-src`, `media-src` and `connect-src` (browser `PUT`s uploads via `fetch`).

## Payments — Stripe test mode (Etap 3 + All-access)

Paid courses are purchased through **Stripe Checkout** in test mode. The enrollment is created **only by the webhook** (`checkout.session.completed`) after Stripe confirms the payment — never from the browser redirect. Free courses (price 0) enroll directly without Stripe.

**All-access** is a separate monthly Stripe Subscription (~399 PLN, configurable via `Subscription:MonthlyPricePln`). It unlocks all **Published** courses without creating per-course enrollments. Access is driven by webhook events (`checkout.session.completed` for subscription mode, plus `customer.subscription.*` / `invoice.paid`). Public price: `GET /api/payments/subscription/offer`. Manage billing: Stripe Customer Portal via `POST /api/payments/subscription/portal`.

Admin coupons can discount single-course checkout (preview + apply at `POST /api/payments/checkout`).

### Flow (single course)

1. `POST /api/payments/checkout` (authorized) — verifies the course and price server-side, stores a `Payment` row (`Pending`) and returns the Stripe Checkout URL.
2. The browser is redirected to Stripe; the user pays with a test card.
3. Stripe calls `POST /api/payments/webhook` — signature is verified, the amount is compared against the stored payment, the payment becomes `Completed` and the enrollment is created (idempotent; `checkout.session.expired` marks abandoned sessions `Expired`).
4. The browser lands on `/payment/success`, which polls `GET /api/payments/{sessionId}` until the webhook finishes.

Direct enrollment (`POST /api/enrollments`) rejects paid courses, so payment cannot be bypassed.

### Flow (All-access)

1. `POST /api/payments/subscription/checkout` (authorized) — creates a Stripe Checkout session in `subscription` mode.
2. After payment, the webhook activates the local `Subscription` row (`Status` + `CurrentPeriodEnd`); success page polls `GET /api/payments/subscription/me`.

### Setup (dev)

1. Create a free Stripe account and copy the **test** secret key (`sk_test_...`) from [dashboard.stripe.com/test/apikeys](https://dashboard.stripe.com/test/apikeys) into `.env` as `STRIPE_SECRET_KEY`.
2. Start the webhook forwarder (separate compose profile, so the stack runs without it):

   ```bash
   docker compose --profile stripe up -d stripe_cli
   docker compose logs stripe_cli   # prints: Your webhook signing secret is whsec_...
   ```

3. Copy the printed `whsec_...` into `.env` as `STRIPE_WEBHOOK_SECRET` and restart the API:

   ```bash
   docker compose restart api
   ```

4. Test with card `4242 4242 4242 4242`, any future expiry date and any CVC.

Without the keys the API responds to checkout attempts with a validation error ("Płatności są chwilowo niedostępne") — the rest of the app works normally.

## Development commands

All commands run inside Docker containers (except Playwright browsers on the host):

```bash
docker compose exec api dotnet build
docker compose exec api dotnet test
docker compose exec frontend npm run type-check
docker compose exec frontend npm run lint
docker compose exec frontend npm run test          # Vitest unit tests
docker compose exec frontend npm run build
```

Playwright smoke (host, stack must be up):

```bash
cd frontend
npx playwright install
npm run test:e2e
```

See `IMPLEMENTATION_PLAN.md` for architecture details and historical roadmap. Status of stages **1–13: done** (domain MVP, MinIO, Stripe, Q&A, quizzes, coupons, player UX, progress gates, lesson materials, wishlist/waitlist, Q&A moderation, All-access subscription, analytics — plus certificates, search, realtime, auth recovery, error pages, tests).

### Waitlist note

- **Wishlist** — Published courses only.
- **Waitlist** — unpublished courses (`Draft` via shareable teaser URL; `Hidden` also appears in the public catalog as “coming soon”). Joining requires login; publishing a course emails waitlisted users (MailHog in dev).

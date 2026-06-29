# Course Platform

Online course platform — ASP.NET Core 9 (Clean Architecture) + Vue 3 + TypeScript.

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

Official local dev path is **Docker Compose**. `launchSettings.json` ports (5089/7079) are for optional IDE runs outside containers.

## Configuration

### Secrets

JWT signing key is **not** stored in committed config files.

| Source | Purpose |
|--------|---------|
| `.env` (gitignored) | `JWT_KEY` — interpolated into `docker-compose.yml` as `Jwt__Key` |
| `appsettings.Development.json` (gitignored) | Connection string, `Dev:Seed`, dev CSP |
| `appsettings.Development.json.example` | Template copied on first setup |

ASP.NET Core configuration provider chain: environment variables override appsettings.

### Database seed

Seed runs only when **both** conditions are true:

- `ASPNETCORE_ENVIRONMENT=Development`
- `Dev:Seed=true` in `appsettings.Development.json`

Production never seeds, even if `Dev:Seed` is set accidentally.

## Security

### Headers (API)

`SecurityHeadersMiddleware` sets CSP per environment from config:

- **Development** — allows `localhost:8080`, `localhost:5173`, Swagger inline scripts
- **Production** — strict whitelist (`placeholder.local` for seed thumbnails; no blanket `https:`)

Additional directives: `frame-ancestors 'none'`, `object-src 'none'`, `base-uri 'self'`, `form-action 'self'`.

CSP on API responses is defence-in-depth (Swagger, error pages, iframe protection). The Vue SPA will need its own CSP headers when deployed behind nginx/Nuxt.

### JWT validation at startup

`Program.cs` fails fast if `Jwt:Key` is missing, too short (< 32 bytes), still set to the `.env.example` placeholder, or if `Jwt:Issuer` / `Jwt:Audience` are missing.

### Future — MinIO (Etap 2)

When object storage is added, extend Production CSP `connect-src` and `img-src` with the MinIO/public CDN origin.

## Development commands

All commands run inside Docker containers:

```bash
docker compose exec api dotnet build
docker compose exec api dotnet test
docker compose exec frontend npm run type-check
docker compose exec frontend npm run lint
docker compose exec frontend npm run build
```

See `IMPLEMENTATION_PLAN.md` for architecture and roadmap.

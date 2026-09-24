# WebAPI

ASP.NET Core 10 host for MyLeague. Controllers translate HTTP to MediatR requests and wrap results in a consistent `ApiResponse` / paged envelope.

See the [root README](../../../README.md) for ports, auth, and Docker. [WebAPIDevelopmentGuide.md](./WebAPIDevelopmentGuide.md) still imports `Application.Commands` / `Application.DTOs`, which are not the current namespaces. Follow this README and `.cursor/skills/create-api-endpoint/SKILL.md`.

## Responsibilities

- Route HTTP to Application commands and queries
- JWT bearer auth (and SignalR query-string tokens)
- FluentValidation error mapping
- CORS (Development only; Azure sets CORS in Bicep)
- Serilog request logging; Application Insights when a connection string is present
- Scalar / OpenAPI in Development
- Health endpoints and a static dashboard (`/health-test.html`; `/health-ui` redirects there)
- Match-event rate limiting (`IMatchEventRateLimiter`)

## Technology

- .NET 10 / ASP.NET Core 10
- MediatR 12.5
- FluentValidation.AspNetCore 11.3
- JWT Bearer 10.0
- Scalar.AspNetCore 1.2
- Serilog (console, file, Seq, Application Insights)
- Static health dashboard in `wwwroot/health-test.html` (the Xabaril HealthChecks.UI package is not referenced)

## Structure

```
WebAPI/
├── Controllers/
│   ├── Auth/
│   ├── Common/              # Clubs, ClubAdmin, Divisions, Persons, Users,
│   │                        # News, Search, Rules, Info pages, Footer contacts,
│   │                        # Site settings, MatchTimer
│   ├── Floorball/           # Teams, players, referees, team managers, seasons,
│   │   └── Match/           #   tournaments, matches, events, officials, roster, lifecycle
│   ├── Football/            # Parallel to floorball
│   ├── Hockey/
│   └── Health/
├── Models/                  # Auth, Common, Floorball, Football, Hockey request types
├── Middlewares/             # ExceptionHandlingMiddleware
├── DependencyInjections/    # OpenAPI + Health Check UI
├── Services/                # Match-event rate limiter
├── appsettings*.json
├── Program.cs
└── Dockerfile
```

## Endpoints

Full route tables live in the [root README](../../../README.md#api-overview). High-level groups:

| Area | Examples |
|------|----------|
| Auth | `/api/auth/login`, `/verify`, `/refresh`, `/logout`, `/me` |
| Common | `/api/clubs`, `/api/club-admin`, `/api/news`, `/api/search`, `/api/site-settings` |
| Floorball | `/api/floorballteam`, `/api/floorball-matches`, `/api/floorball/statistics` |
| Football | `/api/footballteam`, `/api/football-matches`, `/api/football/statistics` |
| Hockey | `/api/hockeyteam`, `/api/hockeymatch`, `/api/HockeyStatistics` |
| Real-time | `/api/hubs/domainevent` |
| Ops | `/health`, `/health/ready`, `/health/live`, `/health-ui` → `/health-test.html`, `/api/health`, `/api/version` |

Scalar UI: `/scalar/v1` (Development). OpenAPI: `/swagger/v1/swagger.json`. Health check names and gaps: [HealthChecks-README.md](./HealthChecks-README.md).

### Response shape

```json
{
  "success": true,
  "data": { },
  "message": "Operation completed successfully",
  "errors": null
}
```

Paged payloads use `items`, `totalCount`, `page`, `pageSize`, `totalPages`.

## Run

**Docker (recommended):** from repo root, `docker compose up -d` — API at http://localhost:8080.

**Kestrel:**

```bash
cd src/backend/WebAPI
dotnet run
```

| Environment | URLs |
|-------------|------|
| Docker | http://localhost:8080 |
| Development profile | https://localhost:65532 and http://localhost:65533 |

`appsettings.Development.json` uses local PostgreSQL and `LoginCode:AutoFillLoginCode=true`. JWT lifetimes are longer in Development (60 minutes / 30 days) than the production defaults in `appsettings.json` (15 minutes / 7 days).

Seed admin: `test@myleague.local` on first local startup. Docker override uses `test@myleague.fi`.

## Configuration (essentials)

```json
{
  "ConnectionStrings": { "DefaultConnection": "Host=localhost;Database=myleague;Username=postgres;Password=postgres;Port=5432" },
  "Jwt": { "Issuer": "MyLeague", "Audience": "MyLeague", "AccessTokenExpirationMinutes": 15, "RefreshTokenExpirationDays": 7 },
  "LoginCode": { "ExpirationMinutes": 10, "CodeLength": 6, "MaxAttempts": 5, "AutoFillLoginCode": false },
  "Seed": { "AdminEmail": "" },
  "Frontend": { "BaseUrl": "http://localhost:5173" }
}
```

`LoginCode__AutoFillLoginCode` must stay `false` on any publicly reachable environment. See [`infra/README.md`](../../../infra/README.md).

## Auth flow

1. `POST /api/auth/login` `{ "email": "user@example.com" }`
2. Code is emailed (or logged / auto-filled in Development)
3. `POST /api/auth/verify` `{ "email": "...", "code": "123456" }` → `{ accessToken, refreshToken, expiresAt }`
4. `Authorization: Bearer <accessToken>`; SignalR uses `?access_token=`

## Logging

- Console and rolling files under `logs/myleague-api-{date}.log`
- Seq at http://localhost:5341 when Compose is up
- Application Insights when `APPLICATIONINSIGHTS_CONNECTION_STRING` is set (Azure). Registration is skipped locally so startup does not fail.

## Contributing

- Keep controllers thin: map request → command/query → `ApiResponse`.
- Document actions with XML comments (feeds OpenAPI).
- Add tests in `tests/backend/WebAPI.UnitTests/`.

This project is part of MyLeague. See the [root README](../../../README.md).

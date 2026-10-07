# WebAPI

ASP.NET Core 10 host. Controllers turn HTTP requests into MediatR commands and queries and wrap the `Result<T>` in an `ApiResponse` envelope. The project also hosts the SignalR hub, health endpoints, rate limiting, output caching, OpenAPI, and Scalar.

References Application and Infrastructure. Controllers never touch repositories or a `DbContext`.

Conventions: [backend rules](../../../.claude/rules/backend.md#webapi). New or changed endpoints: [create-api-endpoint](../../../.claude/skills/create-api-endpoint/SKILL.md).

## Folder map

```
WebAPI/
├── Controllers/
│   ├── Auth/                AuthController
│   ├── Common/              BaseApiController, ApiErrorHttpMapper, ClubAdmin, Clubs, DataSubjectRights,
│   │                        Divisions, FooterContact, InfoPageContent, MatchTimer, News, Persons,
│   │                        RulesSection, Search, SiteSettings, Users
│   ├── Floorball/           Player, Referee, Season, Statistics, Team, TeamManager, Tournament
│   │   └── Match/           Matches, Events, Lifecycle, Officials, Roster, Scorekeepers
│   ├── Football/            same as Floorball
│   │   └── Match/           Matches, Events, Lifecycle, Lineup, Officials, Scorekeepers
│   ├── Hockey/              Competition, Match, Official, Player, Season, Statistics, Team, Tournament
│   └── Health/              HealthController (/api/health, /api/version)
├── Models/                  Request records: Auth/ Common/ Floorball/ Football/ Hockey/
│   └── Common/              ApiResponse, Pagination/ (PaginatedApiResponse, PaginationMetadata, PagedRequestBase)
├── Middlewares/             ExceptionHandlingMiddleware
├── DependencyInjections/    AddOpenApiConfiguration, AddPublicTrafficProtection
├── Services/                IMatchEventRateLimiter, MatchEventRateLimiter, MatchEventRateLimits
├── wwwroot/                 health-test.html, uploads/ (local image storage)
├── Program.cs
└── appsettings*.json, Dockerfile
```

## Request pipeline

`Program.cs` registers services in this order: Application Insights (only when a connection string is set), Serilog, controllers (camelCase JSON, enums as strings), OpenAPI, CORS policy, options, JWT bearer, `IMatchEventRateLimiter`, `AddPublicTrafficProtection`, `AddApplication`, `AddInfrastructure`.

Middleware order:

1. OpenAPI at `/swagger/v1/swagger.json` and Scalar at `/scalar/v1` (Development only)
2. `UseForwardedHeaders`
3. `ExceptionHandlingMiddleware`
4. `UseSerilogRequestLogging`, `UseHttpsRedirection`
5. `UseCors("Development")` (Development only; Azure sets CORS on the App Service in Bicep)
6. `UseStaticFiles`, `UseAuthentication`, `UseAuthorization`, `UseRateLimiter`, `UseOutputCache`
7. `MapControllers`, `MapHub<DomainEventHub>("/api/hubs/domainevent")`, health endpoints

## Key types

| Type | Location | Purpose |
|------|----------|---------|
| `BaseApiController` | `Controllers/Common/` | Base class for every controller |
| `ApiErrorHttpMapper` | `Controllers/Common/` | Picks the status code and safe message for a failed `Result` |
| `ApiResponse`, `ApiResponse<T>` | `Models/Common/ApiResponse.cs` | Envelope: `success`, `message`, `errors`, `data` |
| `PaginatedApiResponse<T>` | `Models/Common/Pagination/` | `data` is the item list; `pagination` holds the metadata |
| `PagedRequestBase` | `Models/Common/Pagination/` | `Page` (≥ 1), `PageSize` (0–100, 0 = configured default) |
| `ExceptionHandlingMiddleware` | `Middlewares/` | Turns unhandled exceptions into an `ApiResponse` |
| `PublicTrafficProtectionExtensions` | `DependencyInjections/` | Forwarded headers, rate-limit policies, output-cache policy |

`BaseApiController` helpers:

| Helper | Use |
|--------|-----|
| `HandleResult(result, successMessage, defaultErrorMessage)` | Single payload |
| `HandlePaginatedResult(...)` | `Result<PagedResult<T>>` → `PaginatedApiResponse<T>` |
| `HandleListResult(...)` | `Result<IEnumerable<T>>` → `ApiResponse<List<T>>` |
| `HandleVoidResult(...)` | Success without a body (delete, logout) |
| `ToErrorResponse(result, defaultMessage)` | Failure only, for example after a custom success path such as `CreatedAtAction` |
| `IncludeDrafts(requested)` | `true` only when requested by a SystemAdmin |
| `IncludePrivateData` | `true` only for a SystemAdmin. Others must not get birth dates, addresses, contact details, or licence numbers |
| `SanitizeForLog(value)` | Strips newlines from user input before logging |

```csharp
Result<ClubDto> result = await _mediator.Send(new GetClubByIdQuery(id));
return HandleResult(result, "Club retrieved successfully", "Club not found");
```

## Responses and errors

```json
{ "success": true, "message": "Club retrieved successfully", "errors": [], "data": { } }
```

```json
{
  "success": true,
  "message": "Hockey teams retrieved successfully",
  "errors": [],
  "data": [ ],
  "pagination": {
    "currentPage": 1, "pageSize": 15, "totalCount": 42, "totalPages": 3,
    "hasNextPage": true, "hasPreviousPage": false, "startItem": 1, "endItem": 15
  }
}
```

Failed `Result` (via `ApiErrorHttpMapper`):

| Condition | Status |
|-----------|--------|
| `ErrorKind == NotFound` | 404 |
| `ErrorKind == Validation` | 400 |
| Message contains "not found" (fallback for old handlers) | 404 |
| Text contains `DbUpdateException:`, `PostgresException:`, and similar, outside Development | 500 with a generic message |
| Anything else | 400 |

Unhandled exceptions (`ExceptionHandlingMiddleware`):

| Exception | Status |
|-----------|--------|
| `ArgumentException`, `InvalidOperationException` | 400 |
| `KeyNotFoundException` | 404 |
| `UnauthorizedAccessException` | 401 |
| `DbUpdateException` | 409, names the unique constraint if there is one |
| Anything else | 500; the exception text is shown only in Development |

## Auth

- JWT bearer. Issuer, audience, and key come from the `Jwt` section. Startup fails if `Jwt:SecretKey` is shorter than 32 bytes, or if it is empty outside Development. An empty key in Development falls back to a built-in key.
- SignalR sends the token as `?access_token=` on paths under `/api/hubs`.
- There is no fallback authorization policy. An action without `[Authorize]` is public. Writes use `[Authorize(Roles = AuthRoles.AdminOnly)]` or `[Authorize(Roles = AuthRoles.ClubAdminOrAdmin)]`.
- Sign-in flow: `POST /api/auth/login` `{ "email" }` → `POST /api/auth/verify` `{ "email", "code" }` → `{ accessToken, refreshToken, expiresAt }`. Then `POST /api/auth/refresh`, `POST /api/auth/logout`, `GET /api/auth/me`.
- With `LoginCode:AutoFillLoginCode` on (Development only), `/api/auth/login` returns the code in `data.autoFillCode`. Never enable it on a public environment.

## Rate limiting and caching

| Mechanism | Where | Limit |
|-----------|-------|-------|
| Policy `auth` | `/api/auth/login`, `/verify`, `/refresh`, `/verify-admin-email` | 10 requests per minute per client IP, 429 when exceeded |
| Policy `public-stats` | `all-time` and `all-time/teams` in the floorball, football, and hockey statistics controllers | 60 per minute per client IP |
| Output cache `public-stats-cache` | Same actions | 60 s, varies by every query parameter |
| `IMatchEventRateLimiter` | Floorball and football match-event controllers | 429 for a repeat of the same match + event + player within 50 ms (goal, penalty) or 250 ms (single floorball save). Requests can set `SkipRateLimit` |

The client IP comes from the last `X-Forwarded-For` entry (`ForwardLimit = 1`), which App Service appends. All of this is per process.

## Endpoints

Full route list: [root README](../../../README.md#api-overview). Route prefixes keep their historical style; do not rename them.

| Area | Examples |
|------|----------|
| Auth | `/api/auth/login`, `/verify`, `/refresh`, `/logout`, `/verify-admin-email`, `/me` |
| Common | `/api/clubs`, `/api/club-admin`, `/api/persons`, `/api/users`, `/api/divisions`, `/api/news`, `/api/search`, `/api/site-settings`, `/api/data-subject-rights`, `/api/matches/{matchId}/timer` |
| Floorball | `/api/floorballteam`, `/api/floorballseason`, `/api/floorball-matches`, `/api/floorball/statistics` |
| Football | `/api/footballteam`, `/api/footballseason`, `/api/football-matches`, `/api/football/statistics` |
| Hockey | `/api/hockeyteam`, `/api/hockeyseason`, `/api/hockeymatch`, `/api/HockeyStatistics` |
| Real-time | `/api/hubs/domainevent` |
| Ops | `/health`, `/health/ready`, `/health/live`, `/health-ui`, `/api/health`, `/api/version` |

## Health checks

| Endpoint | Runs | Response |
|----------|------|----------|
| `/health/ready` | Checks tagged `ready`: PostgreSQL and the four DbContexts | `Healthy` / `Unhealthy` text, 503 when unhealthy |
| `/health` | Every check, including memory, disk, row counts, and service resolution | JSON, 503 when unhealthy. Anonymous callers see only names and statuses; site admins also see descriptions and data |
| `/health/live` | Nothing | `Alive` |
| `/health-ui` | Redirects to `/health-test.html`, a static page that polls `/health` | HTML |

Azure App Service (`healthCheckPath`), the production availability test, and the deploy workflows use `/health/ready`. Check names, tags, thresholds, and `/api/health/*`: [HealthChecks-README.md](./HealthChecks-README.md).

## Configuration

Main sections in `appsettings.json`:

| Section | Notes |
|---------|-------|
| `ConnectionStrings:DefaultConnection` | `appsettings.json` points at host `postgres` (Compose); `appsettings.Development.json` at `localhost:5432` |
| `Jwt` | `Issuer`, `Audience`, `SecretKey`, `AccessTokenExpirationMinutes` (15), `RefreshTokenExpirationDays` (7). Development: 60 min and 30 days |
| `LoginCode` | `ExpirationMinutes` (10), `CodeLength` (6), `MaxAttempts` (5), `AutoFillLoginCode` (false; true in Development) |
| `Pagination` | `Global` and per-resource `Resources:<ResourceKey>` page sizes |
| `PeriodDurations`, `Seed:AdminEmail`, `Frontend:BaseUrl`, `App:BaseUrl`, `AzureCommunicationServices` | |
| `ConnectionStrings:AzureBlobStorage`, `AzureStorage:ContainerName` | Image storage outside Development |

Once an admin saves site settings, the `SiteSettings` row overrides the `Jwt` lifetimes and the `LoginCode` expiry and attempt limit. The `HealthChecks` section in `appsettings.json` is not read by any code.

## Run locally

Follow the [run-local skill](../../../.claude/skills/run-local/SKILL.md): PostgreSQL and Seq in Compose, the API on the host at port 8080, the UI with Vite on the host. Do not run the Compose `webapi` service at the same time; it also uses port 8080.

```bash
dotnet run --project src/backend/WebAPI/WebAPI.csproj --urls http://localhost:8080
```

`launchSettings.json` uses `https://localhost:65532` and `http://localhost:65533`; the local setup above does not use those ports. At startup the API applies migrations for all four contexts. In Development it seeds the sign-in users `test@myleague.local` (SystemAdmin) and `clubadmin@myleague.local` (ClubAdmin). It also seeds `Seed:AdminEmail` when set; `docker-compose.override.yml` sets it to `test@myleague.fi`. Sport data comes from `src/tools/Seeder`.

## Logging

- Serilog to the console and to `logs/myleague-api-<date>.log` (daily, 30 files kept).
- Seq: `docker-compose.override.yml` adds the sink for the Compose `webapi` service through `Serilog__WriteTo__2__*` variables. An API started on the host logs to console and file only, unless you set the same variables with `serverUrl` `http://localhost:5341`.
- Application Insights is registered only when `APPLICATIONINSIGHTS_CONNECTION_STRING` or `ApplicationInsights:ConnectionString` is set.

## Rules and pitfalls

- Keep controllers thin. Map the request record field by field into the command; do not pass request models into Application.
- Put XML `<summary>` comments on controllers and actions. They feed OpenAPI and Scalar.
- Validation runs in the MediatR `ValidationBehavior`, not in ASP.NET Core model validation.
- Rate limits, the output cache, `IMatchEventRateLimiter`, and the match timer are all in-memory. They assume one API instance.

## Tests

`tests/backend/WebAPI.UnitTests/WebApiTestProject`: `Controllers/<Area>/` (result mapping, including `ApiErrorHttpMapperTests`) and `Services/MatchEventRateLimiterTests.cs`. See [testing rules](../../../.claude/rules/testing.md).

```bash
dotnet test tests/backend/WebAPI.UnitTests/WebApiTestProject
```

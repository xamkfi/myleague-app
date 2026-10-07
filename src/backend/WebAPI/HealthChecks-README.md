# Health checks

Reference for the API health endpoints and checks. Overview: [WebAPI README](./README.md#health-checks).

| Piece | File |
|-------|------|
| Check registration (`AddMyLeagueHealthChecks`, `ReadyTag`) | `Infrastructure/HealthChecks/HealthCheckExtensions.cs` |
| Custom checks | `Infrastructure/HealthChecks/DatabaseHealthCheck.cs`, `ApplicationServicesHealthCheck.cs` |
| `/health`, `/health/ready`, `/health/live`, `/health-ui` | `WebAPI/Program.cs` |
| `/api/health/*`, `/api/version` | `WebAPI/Controllers/Health/HealthController.cs` |
| Dashboard | `WebAPI/wwwroot/health-test.html` |

`AddInfrastructure` calls `AddMyLeagueHealthChecks`, so the checks exist wherever the API runs.

## Endpoints

| Endpoint | Checks run | Response | Status codes |
|----------|-----------|----------|--------------|
| `GET /health/ready` | Tagged `ready` | `Healthy` or `Unhealthy` (text/plain) | 200; 503 when any ready check is unhealthy |
| `GET /health` | All | JSON report (below) | 200 for Healthy or Degraded; 503 for Unhealthy |
| `GET /health/live` | None | `Alive` | 200 |
| `GET /health-ui` | None | Redirect to `/health-test.html` | 302 |
| `GET /api/health` | All | JSON report | 200 only when Healthy; 503 for Degraded or Unhealthy; 500 if the check run throws |
| `GET /api/health/tag/{tag}` | Checks with that tag | JSON report plus `tag` | Same as `/api/health` |
| `GET /api/health/ready` | Tagged `ready` | `"Healthy"` / `"Unhealthy"` as JSON strings | 200 / 503 |
| `GET /api/health/live` | None | `"Alive"` | 200 |
| `GET /api/version` | None | `{ "version": "..." }`: build date and short git SHA, set by the `SetBuildVersion` target in `WebAPI.csproj` | 200 |

`/health/ready` is the probe. Azure App Service uses it as `healthCheckPath` (`infra/provision/modules/app-service.bicep`), the production availability test and health alert watch it (`monitoring-alerts.bicep`), and the deploy and release workflows smoke-test it. Memory, disk, row-count, and service-resolution checks are deliberately left out of it, so they cannot take the instance out of rotation.

`/health` JSON as a `SystemAdmin` sees it. Anonymous callers get the same shape, but each check has only `name` and `status`:

```json
{
  "status": "Unhealthy",
  "duration": 15078.27,
  "checkedAt": "2025-05-29T17:41:20.4231028Z",
  "checks": [
    { "name": "self", "status": "Healthy", "description": "API is running", "duration": 0.06, "data": {}, "tags": [] },
    { "name": "postgresql-connection", "status": "Unhealthy", "description": "Name or service not known", "duration": 7856.89, "data": {}, "tags": ["database", "postgresql", "ready"] }
  ]
}
```

## Checks

| Name | Tags | What it does | In `ready` |
|------|------|--------------|------------|
| `self` | none | Always Healthy | no |
| `postgresql-connection` | `database`, `postgresql`, `ready` | Opens a raw Npgsql connection with `DefaultConnection` | yes |
| `common-database` | `database`, `ef-core`, `common`, `ready` | `AddDbContextCheck<CommonDbContext>` | yes |
| `floorball-database` | `database`, `ef-core`, `floorball`, `ready` | `AddDbContextCheck<FloorballDbContext>` | yes |
| `football-database` | `database`, `ef-core`, `football`, `ready` | `AddDbContextCheck<FootballDbContext>` | yes |
| `hockey-database` | `database`, `ef-core`, `hockey`, `ready` | `AddDbContextCheck<HockeyDbContext>` | yes |
| `database-operations` | `database`, `custom` | `CanConnectAsync` on all four contexts, then counts clubs, floorball players, football teams, and hockey teams | no |
| `application-services` | `services`, `dependencies` | Resolves `IClubRepository`, `IPersonRepository`, the floorball player, team, match, and competition repositories, and `IUnitOfWork`. Degraded if any fail | no |
| `memory-usage` | `system`, `memory` | Process allocated memory above 1000 MB is Unhealthy | no |
| `private-memory` | `system`, `memory` | Private memory above 1.5 GB is Unhealthy | no |
| `disk-storage` | `system`, `storage` | Less than 1000 MB free on `C:\` (Windows) or `/` (Linux) is Unhealthy | no |

The thresholds are hardcoded in `HealthCheckExtensions.cs`. The `HealthChecks` section in `appsettings.json` is not read by any code; changing it has no effect.

## Dashboard

`/health-test.html` is a static page. It fetches `/health` (falling back to `localhost:8080`, `65533`, and `65532`) and refreshes every 30 seconds. There is no history. The page sends no token, so it shows only check names and statuses. The Xabaril `HealthChecks.UI` package is not referenced, because it pulls in `KubernetesClient` (GHSA-w7r3-mgwf-4mqq).

## Examples

```bash
curl http://localhost:8080/health/ready
curl http://localhost:8080/health
curl http://localhost:8080/api/health/tag/database
```

## Troubleshooting

| Symptom | Cause |
|---------|-------|
| `postgresql-connection` says "Name or service not known" | `DefaultConnection` points at host `postgres` (the `appsettings.json` default) while the API runs on the host. Use `ASPNETCORE_ENVIRONMENT=Development` so `localhost:5432` applies |
| `/health` is 503 but `/health/ready` is 200 | A diagnostic check failed (memory, disk). Readiness is unaffected |
| `/api/health` is 503 but `/health` is 200 | `application-services` is Degraded. `/api/health` treats Degraded as a failure; `/health` does not |
| `/health-test.html` returns 404 | `app.UseStaticFiles()` is missing from `Program.cs` or `wwwroot` was not published |

The custom checks log at Debug under `MyLeague.Infrastructure.HealthChecks`.

## Security

All health endpoints are anonymous, because CI, the Docker healthcheck, and the deploy smoke tests call `/health` without a token. `/health`, `/api/health`, and `/api/health/tag/{tag}` return each check's description, duration, data, and tags only to a `SystemAdmin` (send the JWT as a bearer token). Everyone else gets the overall status plus each check's name and status, so row counts, memory use, and exception messages stay private (`WebAPI/Controllers/Health/HealthReportResponse.cs`). Probes and external monitors should still call `/health/ready` or `/health/live`.

## Gaps

- `application-services` checks only common and floorball repositories; football and hockey repositories are not resolved.
- `database-operations` counts rows in one table per context only.
- Thresholds cannot be changed through configuration.

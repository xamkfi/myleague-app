# Running MyLeague with Docker Compose

The recommended local setup runs only PostgreSQL and Seq in Docker and runs the API and UI on the host. See [Quick start](../../README.md#quick-start) in the root README. This page covers the alternative: the whole stack in containers, from the command line or from Visual Studio.

All commands run from the repository root. Use `docker compose` (Compose v2), not the old `docker-compose`.

## Compose files

| File | Purpose |
|------|---------|
| [`docker-compose.yml`](../../docker-compose.yml) | Base services: `postgres`, `seq`, `webapi`, `frontend`, the `myleague-network` network, and the `postgres_data` and `seq_data` volumes |
| [`docker-compose.override.yml`](../../docker-compose.override.yml) | Local settings, merged automatically: Development environment, connection string, Seq sink, dev JWT key, port 8080, volume mounts, and the frontend dev server |
| [`docker-compose.ci.yml`](../../docker-compose.ci.yml) | CI overlay. Pass it explicitly with the base file so the local override is not used |
| [`docker-compose.dcproj`](../../docker-compose.dcproj) | Visual Studio project for the Compose stack |
| [`WebAPI/Dockerfile`](WebAPI/Dockerfile) | Multi-stage .NET 10 build of the API |
| [`../frontend/Dockerfile`](../frontend/Dockerfile) | Node 22 + pnpm image that runs the Vite dev server |

## Services

| Service | Container | Host port | Notes |
|---------|-----------|-----------|-------|
| `postgres` | `myleague-postgres` | 5432 | PostgreSQL 16, database `myleague`, user and password `postgres` |
| `seq` | `myleague-seq` | 5341 | Seq UI and log ingestion, no authentication |
| `webapi` | generated | 8080 | Built from `src/backend/WebAPI/Dockerfile`, runs with `ASPNETCORE_ENVIRONMENT=Development` |
| `frontend` | `myleague-frontend` | 5173 | Vite dev server with the source mounted from `src/frontend` |

## Run the full stack

```bash
docker compose up -d --build
```

On startup the API applies the migrations for all four DbContexts and creates the dev users. Then seed sample data from the host while the API is running:

```bash
dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=all
```

Press Enter at the URL prompt to use `http://localhost:8080/`. `--scope=all` seeds floorball; use `--sport=football --scope=all` for football and `--scope=hockey` for ice hockey. See the [Seeder README](../tools/Seeder/README.md).

| Service | URL |
|---------|-----|
| UI | http://localhost:5173 |
| API | http://localhost:8080 |
| Scalar API docs | http://localhost:8080/scalar/v1 |
| Health | http://localhost:8080/health, `/health/ready`, `/health/live` |
| Health dashboard | http://localhost:8080/health-ui |
| Seq | http://localhost:5341 |

The browser calls the API at `http://localhost:8080/api` (from `src/frontend/.env.development`), so the UI works the same as with the host setup.

Do not run the host API (`dotnet run --urls http://localhost:8080`) at the same time as the `webapi` container. Both use port 8080.

## Sign in

Because the container runs in Development, it creates `test@myleague.local` (site admin) and `clubadmin@myleague.local` (club admin). The override also sets `Seed__AdminEmail=test@myleague.fi`, which creates a third admin.

No email is sent. The UI fills in the login code because `LoginCode:AutoFillLoginCode` is true in Development. The code is also in the API log:

```bash
docker compose logs -f webapi    # look for "LOGIN CODE for"
```

## Common commands

```bash
docker compose ps                       # status and health
docker compose logs -f webapi           # follow one service (webapi, postgres, seq, frontend)
docker compose up -d --build webapi     # rebuild and restart the API after code changes
docker compose restart webapi
docker compose stop frontend            # use the host Vite server instead
docker compose down                     # stop and remove containers, keep data
docker compose down -v                  # also delete the database and Seq volumes
```

## Data and mounts

| What | Where |
|------|-------|
| PostgreSQL data | Volume `<project>_postgres_data`. The project name comes from the folder name, for example `myleague-app-xamkfi_postgres_data` |
| Seq data | Volume `<project>_seq_data` |
| API log files | `src/backend/WebAPI/logs/` on the host |
| Uploaded images | `src/backend/WebAPI/wwwroot/uploads/` on the host |
| .NET user secrets | `${APPDATA}/Microsoft/UserSecrets`, mounted read-only. This path is for Windows; on macOS or Linux, set `APPDATA` or adjust the mount in your local copy |

## Configuration

`docker-compose.override.yml` sets these for `webapi`:

| Variable | Value |
|----------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Development`, so `appsettings.Development.json` applies |
| `ASPNETCORE_URLS` | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | `Host=postgres;...` (the Compose service name) |
| `Serilog__WriteTo__2__Name` / `__Args__serverUrl` | Adds the Seq sink at `http://seq:80` |
| `Jwt__SecretKey`, `Jwt__Issuer`, `Jwt__Audience` | Dev-only signing settings |
| `Seed__AdminEmail` | `test@myleague.fi` |
| `AzureCommunicationServices__ConnectionString` / `__SenderAddress` | Empty. Development always logs codes instead of sending email |

Other settings you can override with environment variables (`Section__Key`):

| Variable | `appsettings.json` | Development |
|----------|--------------------|-------------|
| `Jwt__AccessTokenExpirationMinutes` | 15 | 60 |
| `Jwt__RefreshTokenExpirationDays` | 7 | 30 |
| `LoginCode__ExpirationMinutes` | 10 | 10 |
| `LoginCode__CodeLength` | 6 | 6 |
| `LoginCode__MaxAttempts` | 5 | 5 |
| `LoginCode__AutoFillLoginCode` | false | true |
| `Frontend__BaseUrl` | `http://localhost:5173` | `http://localhost:5173` |

Never set `LoginCode__AutoFillLoginCode=true` on a public environment. Azure settings are described in [infra/README.md](../../infra/README.md).

## Migrations

The API runs `Database.Migrate()` for all four contexts at startup, so a new empty volume is migrated on first boot. The runtime image has no .NET SDK, so do not run `dotnet ef` inside the container. To create a migration, run `dotnet ef` on the host from `src/backend/Infrastructure`; see the [Infrastructure README](Infrastructure/README.md) and [.claude/skills/ef-migration/SKILL.md](../../.claude/skills/ef-migration/SKILL.md).

## CI

GitHub Actions starts the API from the base file plus the CI overlay. This skips the local override, with its volume mounts and Windows paths:

```bash
docker compose -f docker-compose.yml -f docker-compose.ci.yml up -d --build webapi
docker compose -f docker-compose.yml -f docker-compose.ci.yml down -v
```

## Visual Studio

Visual Studio can run and debug the Compose stack. You need Visual Studio 2026 with the **ASP.NET and web development** workload (it includes the container tools), and Docker Desktop running (the WSL 2 backend is recommended on Windows).

1. Open `MyLeague.sln`.
2. Right-click the `docker-compose` project and choose **Set as Startup Project**. The launch profile is **Docker Compose**.
3. Press F5. Visual Studio builds the images, starts all four services, attaches the debugger to `webapi`, and opens `http://localhost:8080/scalar/v1`.

Breakpoints and Hot Reload work in the container. Use **View > Other Windows > Containers** to see logs, environment variables, and files for each container.

To debug the API on the host instead:

- Start it with `dotnet run --project src/backend/WebAPI/WebAPI.csproj --urls http://localhost:8080` (with `ASPNETCORE_ENVIRONMENT=Development`) and use **Debug > Attach to Process** on `WebAPI`.
- The `WebAPI` launch profile in `Properties/launchSettings.json` listens on `https://localhost:65532` and `http://localhost:65533`. The UI expects the API on port 8080, so use that profile only for API-only work such as Scalar.

## Troubleshooting

| Problem | What to check |
|---------|---------------|
| `webapi` does not start | `docker compose ps` (it waits for `postgres` to be healthy), then `docker compose logs webapi` |
| Port 8080 already in use | A host API from `dotnet run` is probably running. Stop one of them |
| Port 5432 already in use, or password `postgres` is rejected | A local PostgreSQL owns the port. See the workaround in [.claude/skills/run-local/SKILL.md](../../.claude/skills/run-local/SKILL.md#port-5432-already-taken), or change the host port in a local copy of the Compose file |
| No logs in Seq | Seq takes up to a minute to start. Check http://localhost:5341, then `docker compose restart webapi` |
| UI changes not picked up | The `frontend` container polls for file changes. Restart it with `docker compose restart frontend`, or run Vite on the host |
| Empty lists in the UI | The database has no data yet. Run the Seeder |

# MyLeague

League management for floorball, football, and ice hockey: clubs, teams, players, officials, seasons, tournaments, live matches, and statistics. It has a public site, an admin area, and a club-admin area for all three sports.

[![Backend CI](https://github.com/xamkfi/myleague-app/actions/workflows/backend-ci.yaml/badge.svg)](https://github.com/xamkfi/myleague-app/actions/workflows/backend-ci.yaml)
[![Frontend CI](https://github.com/xamkfi/myleague-app/actions/workflows/frontend-ci.yaml/badge.svg)](https://github.com/xamkfi/myleague-app/actions/workflows/frontend-ci.yaml)

The backend is .NET 10 with Clean Architecture and CQRS (MediatR) on PostgreSQL. The frontend is a React 18 SPA built with Vite. Azure hosts staging and production; see [infra/README.md](infra/README.md).

## Features

- **Three sports as peers:** floorball, football, and ice hockey each have public pages, admin tools, and club-admin tools.
- **Seasons and tournaments:** groups, playoffs, and a lifecycle from draft to completed. Both are stored as one `*Competition` hierarchy, so matches and statistics use a single `competitionId`.
- **Live matches:** goals, penalties, saves, lineups, and a match timer. Floorball and football pages update over SignalR. The hockey match page polls the REST API.
- **Statistics:** standings, top scorers, team and player season stats, and all-time stats per sport.
- **Content:** news with a hero carousel and tags, editable rules and info pages, MAHL pages, age groups, an event calendar, and search.
- **Club admin:** club-scoped rosters and match-day tools.
- **Auth:** passwordless email login code, then a JWT access token with refresh-token rotation. Roles are `SystemAdmin` and `ClubAdmin`.
- **Privacy:** anonymous reads do not expose private person data. Admins can handle data-subject requests (access, export, erasure).
- **Languages:** Finnish (default) and English through i18next.
- **Observability:** Serilog to console and files, Seq locally, Application Insights in Azure.
- **Dev data:** an HTTP seeder that creates clubs, teams, players, seasons, tournaments, and simulated matches.

## Tech stack

| Area | Stack |
|------|-------|
| Backend | .NET 10, ASP.NET Core 10, EF Core 10 (Npgsql), MediatR 12.5, FluentValidation, Serilog, SignalR, Scalar/OpenAPI |
| Frontend | React 18.3, TypeScript 5.8, Vite 6.4, SCSS (Tailwind 4 is installed but barely used), React Router 7, i18next, SignalR client |
| Data | PostgreSQL 16, four EF Core DbContexts (`Common`, `Floorball`, `Football`, `Hockey`) in one database |
| Local tooling | Docker Compose (PostgreSQL, Seq), Node 22, pnpm 10 |
| Cloud | Azure App Service, Static Web Apps, PostgreSQL Flexible Server, Blob Storage, Communication Services Email, Application Insights; Bicep and GitHub Actions |

There is no AutoMapper, no event sourcing, and no Redis. Mappers are static classes next to each feature.

## Repository layout

```
MyLeague.sln                solution: backend, tests, tools
src/backend/Domain          entities, value objects, enums, repository interfaces
src/backend/Application     CQRS feature slices: Features/{Auth,Common,Floorball,Football,Hockey}
src/backend/Infrastructure  EF Core contexts, repositories, migrations, auth, email, images, SignalR
src/backend/WebAPI          thin controllers, middleware, OpenAPI, health checks, Dockerfile
src/frontend                React SPA (Vite, pnpm)
src/tools                   Seeder, importers, TournamentExporter
tests/backend               Domain, Application, WebAPI unit tests; Infrastructure integration tests
tests/tools                 importer unit tests
infra                       Bicep templates and deployment docs
docker-compose*.yml         local stack (postgres, seq, webapi, frontend) and CI overlay
```

## Quick start

### Prerequisites

- .NET 10 SDK
- Node.js 22 and [pnpm](https://pnpm.io/) 10
- Docker Desktop
- Git

### Run locally

PostgreSQL and Seq run in Docker. The API and the UI run on your machine, so you always run your current working tree.

1. Start PostgreSQL and Seq from the repository root:

   ```bash
   docker compose up -d postgres seq
   ```

2. Start the API on port 8080 in Development mode. `appsettings.Development.json` already points at `localhost:5432`. Startup applies the migrations for all four DbContexts and creates the dev users.

   ```bash
   # Bash
   ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/backend/WebAPI/WebAPI.csproj --urls http://localhost:8080
   ```

   ```powershell
   # PowerShell
   $env:ASPNETCORE_ENVIRONMENT = 'Development'; dotnet run --project src/backend/WebAPI/WebAPI.csproj --urls http://localhost:8080
   ```

3. Start the UI in another terminal:

   ```bash
   cd src/frontend
   pnpm install
   pnpm dev
   ```

   Keep `src/frontend/.env.development` at `VITE_API_URL=http://localhost:8080/api`.

4. Seed sample data while the API is running. The seeder asks for the API URL; press Enter to accept `http://localhost:8080/`.

   ```bash
   dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=all
   ```

   `--scope=all` seeds floorball. Use `--sport=football --scope=all` for football and `--scope=hockey` for ice hockey. See the [Seeder README](src/tools/Seeder/README.md) for all options.

| Service | URL |
|---------|-----|
| UI | http://localhost:5173 |
| API | http://localhost:8080 |
| API docs (Scalar, Development only) | http://localhost:8080/scalar/v1 |
| Health | http://localhost:8080/health (also `/health/ready`, `/health/live`) |
| Seq | http://localhost:5341 |
| PostgreSQL | `localhost:5432`, database `myleague`, user `postgres`, password `postgres` |

The API run this way logs to the console and to `src/backend/WebAPI/logs/`. To also send logs to Seq, set `Serilog__WriteTo__2__Name=Seq` and `Serilog__WriteTo__2__Args__serverUrl=http://localhost:5341` before `dotnet run`.

If `localhost:5432` rejects the password `postgres`, another PostgreSQL already owns that port. The workaround (move the container to port 5433) is in [.claude/skills/run-local/SKILL.md](.claude/skills/run-local/SKILL.md#port-5432-already-taken).

To run the whole stack in containers instead, see [src/backend/README-Docker.md](src/backend/README-Docker.md).

### Sign in

Development creates two users on startup:

| Email | Role |
|-------|------|
| `test@myleague.local` | Site admin (`SystemAdmin`) |
| `clubadmin@myleague.local` | Club admin |

Enter the email at `/admin/login` or `/club-admin/login`. In Development no email is sent. The login code is written to the API log (`LOGIN CODE for ...`), and `LoginCode:AutoFillLoginCode=true` in `appsettings.Development.json` returns it to the UI so it fills in. Never enable `AutoFillLoginCode` on a public environment.

Login codes have 6 digits, expire after 10 minutes, and lock after 5 failed attempts. Access tokens last 15 minutes and refresh tokens 7 days (60 minutes and 30 days in Development). Reusing a revoked refresh token revokes all of that user's tokens. More detail: [WebAPI README](src/backend/WebAPI/README.md#auth-flow).

## Tests and quality gates

CI runs these on pushes and pull requests to `development` and `master`. Run them before you open a PR:

```bash
dotnet build MyLeague.sln -c Release
dotnet test MyLeague.sln
```

```bash
cd src/frontend
pnpm lint
pnpm build    # also type-checks with tsc
```

There is no frontend test runner. Infrastructure integration tests use the EF Core InMemory provider, so `dotnet test` does not need a database.

Run `npm install` once in the repository root to install the husky pre-commit hook. It lints staged frontend files.

## Branching and releases

- Branch from `development` and open pull requests into `development`.
- Only `development` may merge into `master`. The `protect-master.yml` workflow fails any other pull request into `master`.
- A merge into `development` deploys to staging automatically.
- A merge into `master` starts the MAHL production release (`release-mahl-production.yml`), which waits for one approval on the `mahl-prod` GitHub environment.
- XAMK prod (`release-production.yml`) is no longer released automatically. Run it manually from `master` when needed; it waits for approval on `prod`.

Workflows, environments, OIDC setup, and costs are documented in [infra/README.md](infra/README.md).

## API overview

Scalar UI: `/scalar/v1`. OpenAPI JSON: `/swagger/v1/swagger.json`. Both exist only in Development. Responses use the `ApiResponse` envelope described in the [WebAPI README](src/backend/WebAPI/README.md#response-shape).

| Area | Base routes |
|------|-------------|
| Auth | `/api/auth/login`, `/verify`, `/refresh`, `/logout`, `/verify-admin-email`, `/me` |
| Organization | `/api/clubs`, `/api/divisions`, `/api/persons`, `/api/users`, `/api/club-admin` |
| Content | `/api/news`, `/api/search`, `/api/rulessection`, `/api/infopagecontent`, `/api/footercontact` |
| Admin | `/api/site-settings` (includes `POST /player-licence-reset`), `/api/data-subject-rights` |
| Match timer | `/api/matches/{matchId}/timer` |
| Real-time | `/api/hubs/domainevent` (SignalR; JWT in the `access_token` query parameter) |
| System | `/health`, `/health/ready`, `/health/live`, `/health-ui`, `/api/health`, `/api/version` |

| Sport | Teams and people | Competitions | Matches | Statistics |
|-------|------------------|--------------|---------|------------|
| Floorball | `/api/floorballteam`, `/api/floorballplayer`, `/api/floorballreferee`, `/api/floorballteammanager` | `/api/floorballseason`, `/api/floorballtournament` | `/api/floorball-matches` | `/api/floorball/statistics` |
| Football | `/api/footballteam`, `/api/footballplayer`, `/api/footballreferee`, `/api/footballteammanager` | `/api/footballseason`, `/api/footballtournament` | `/api/football-matches` | `/api/football/statistics` |
| Hockey | `/api/hockeyteam`, `/api/hockeyplayer`, `/api/hockeyofficial` | `/api/hockeyseason`, `/api/hockeytournament`, `/api/hockeycompetition` | `/api/hockeymatch` | `/api/HockeyStatistics` |

For floorball and football, match events, officials, scorekeepers, rosters or lineups, and lifecycle actions are nested under the match route, for example `/api/floorball-matches/{matchId}/events`.

## Documentation

| Topic | Document |
|-------|----------|
| Domain layer | [src/backend/Domain/README.md](src/backend/Domain/README.md) |
| Domain glossary | [src/backend/Domain/DomainGlossary.md](src/backend/Domain/DomainGlossary.md) |
| Application layer | [src/backend/Application/README.md](src/backend/Application/README.md) |
| Infrastructure layer | [src/backend/Infrastructure/README.md](src/backend/Infrastructure/README.md) |
| WebAPI layer | [src/backend/WebAPI/README.md](src/backend/WebAPI/README.md) |
| Health checks | [src/backend/WebAPI/HealthChecks-README.md](src/backend/WebAPI/HealthChecks-README.md) |
| Frontend | [src/frontend/README.md](src/frontend/README.md) |
| Docker Compose and Visual Studio | [src/backend/README-Docker.md](src/backend/README-Docker.md) |
| Azure infrastructure and CI/CD | [infra/README.md](infra/README.md) |
| Hockey domain diagrams | [docs/hockey/mermaid/](docs/hockey/mermaid/01-competitions.md) |

Tools:

| Tool | Purpose |
|------|---------|
| [Seeder](src/tools/Seeder/README.md) | Development and test dataset through the HTTP API |
| [JoomleagueImporter](src/tools/JoomleagueImporter/README.md) | JoomLeague SQL dump to floorball, football, or hockey |
| [TournamentExporter](src/tools/TournamentExporter/README.md) | Export live floorball tournaments as import JSON |

Admins can also import seasons from JSON (all three sports) and floorball tournaments (one per file) in the admin UI.


## AI agents

[AGENTS.md](AGENTS.md) is the entry point for coding agents: conventions, non-negotiables, and the checks to run. [CLAUDE.md](CLAUDE.md) loads it for Claude Code, and `.cursor/rules/myleague.mdc` loads it for Cursor. Detailed rules are in [.claude/rules/](.claude/rules/) and step-by-step workflows (new feature, new endpoint, EF migration, run locally, code review) are in [.claude/skills/](.claude/skills/). Edit them there, not in tool-specific copies.

# MyLeague

League management for floorball, football, and ice hockey.

[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-blue.svg)](https://docs.microsoft.com/en-us/aspnet/core/)
[![React](https://img.shields.io/badge/React-18.3-blue.svg)](https://reactjs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.8-blue.svg)](https://www.typescriptlang.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue.svg)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue.svg)](https://www.docker.com/)
[![Backend CI](https://github.com/xamkfi/myleague-app/actions/workflows/backend-ci.yaml/badge.svg)](https://github.com/xamkfi/myleague-app/actions/workflows/backend-ci.yaml)
[![Frontend CI](https://github.com/xamkfi/myleague-app/actions/workflows/frontend-ci.yaml/badge.svg)](https://github.com/xamkfi/myleague-app/actions/workflows/frontend-ci.yaml)

## Overview

MyLeague is a sports league management system for clubs, teams, players, matches, seasons, and tournaments. The public site and admin tools are built around **floorball** and **football**. **Ice hockey** has a full backend and seeder; the public hockey UI is not enabled yet.

The backend follows Clean Architecture with CQRS (MediatR). Seasons and tournaments share a sport-specific `*Competition` base and are stored with EF Core Table-Per-Hierarchy (TPH), so matches, statistics, and standings use the same `competitionId` for both league seasons and tournaments.

### Key features

- **Multi-sport** — Floorball and football in the UI; ice hockey API and seed data
- **Seasons and tournaments** — Groups, playoffs, lifecycle (draft → registration → group stage → playoff → completed)
- **Live matches** — Goals, penalties, saves/lineups, match timer, and SignalR updates
- **Statistics** — Standings, top scorers, team/player season stats
- **News and info pages** — Hero carousel, tagged articles, editable rules/info content
- **Event calendar** — Upcoming and past matches across sports
- **Club admin** — Club-scoped roster and match-day tools
- **Passwordless auth** — Email login code, JWT access token, refresh-token rotation
- **Finnish / English** — i18next on the frontend
- **Observability** — Serilog, Seq locally, Application Insights in Azure
- **Dev dataset** — HTTP seeder for clubs, teams, players, seasons, tournaments, and simulated matches

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation                             │
│  ┌─────────────┐  ┌─────────────┐                           │
│  │   WebAPI    │  │   React     │                           │
│  │             │  │  Frontend   │                           │
│  └─────────────┘  └─────────────┘                           │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                    Application                              │
│  Feature slices (Auth, Common, Floorball, Football, Hockey) │
│  Commands / Queries / Handlers / DTOs / Validators          │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                   Infrastructure                            │
│  EF Core (PostgreSQL) · Auth · Images · SignalR · Seeding   │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                      Domain                                 │
│  Entities · Value objects · Enums · Repository contracts    │
└─────────────────────────────────────────────────────────────┘
```

Azure hosting (staging + prod) is described in [`infra/README.md`](infra/README.md).

## Technology stack

| Area | Stack |
|------|--------|
| Backend | .NET 9, ASP.NET Core 9, EF Core 9, MediatR 12.5, FluentValidation 12, Serilog 9, Scalar/OpenAPI |
| Frontend | React 18.3, TypeScript 5.8, Vite 6.3, Tailwind CSS 4.1, SCSS, React Router 7, i18next, SignalR client |
| Data | PostgreSQL 16 |
| Local ops | Docker Compose, Seq, pnpm 10, Node 22 |
| Cloud | Azure App Service, Static Web Apps, PostgreSQL Flexible Server, Blob Storage, ACS Email, Application Insights |

There is no AutoMapper. Feature mappers live next to the handlers they serve.

## Project structure

```
myleague-app/
├── src/
│   ├── backend/
│   │   ├── Domain/                 # Entities, value objects, enums, repository interfaces
│   │   ├── Application/            # CQRS feature slices (Auth, Common, Floorball, Football, Hockey)
│   │   ├── Infrastructure/         # EF Core contexts, auth, images, SignalR, health checks
│   │   └── WebAPI/                 # Controllers, middleware, OpenAPI, Docker image
│   ├── frontend/                   # React SPA (Vite)
│   └── tools/
│       ├── Seeder/                 # HTTP seeder (floorball, football, hockey)
│       ├── FloorballPlayerImporter/
│       ├── DataImporter/           # Legacy .jlg person import
│       ├── JoomleagueImporter/     # JoomLeague SQL dump → floorball/football
│       ├── MahlImporter/           # Scrape historical MAHL data
│       └── TournamentExporter/     # Export/import floorball tournaments as JSON
├── tests/backend/
│   ├── Domain.UnitTests/
│   ├── Application.UnitTests/
│   ├── WebAPI.UnitTests/
│   └── Infrastructure.IntegrationTests/
├── infra/                          # Bicep + GitHub Actions deploy docs
├── docker-compose.yml
├── docker-compose.override.yml
└── MyLeague.sln
```

Layer guides: [Domain](src/backend/Domain/README.md) · [Application](src/backend/Application/README.md) · [Infrastructure](src/backend/Infrastructure/README.md) · [WebAPI](src/backend/WebAPI/README.md) · [Frontend](src/frontend/README.md)

## Getting started

### Prerequisites

- .NET 9 SDK
- Node.js 22+ and [pnpm](https://pnpm.io/)
- Docker Desktop (recommended) or a local PostgreSQL 16
- Visual Studio 2022, VS Code, or Rider
- Git

### Quick start with Docker

1. Clone and start services:

   ```bash
   git clone https://github.com/xamkfi/myleague-app.git
   cd myleague-app
   docker compose up -d
   ```

   In Visual Studio you can also open `MyLeague.sln`, set the Docker Compose project as startup, and press F5.

2. Seed a development dataset (optional, recommended):

   ```bash
   dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=all
   ```

   For football as well: `--sport=all`. See [Database seeding](#database-seeding).

3. Open:

   | Service | URL |
   |---------|-----|
   | Frontend | http://localhost:5173 |
   | API docs (Scalar) | http://localhost:8080/scalar/v1 |
   | Health | http://localhost:8080/health |
   | Health UI | http://localhost:8080/health-ui |
   | Seq | http://localhost:5341 |
   | PostgreSQL | `localhost:5432` — database `myleague`, user/password `postgres` / `postgres` |

### Local backend (without Docker for the API)

1. Start PostgreSQL (or keep the Compose `postgres` service running).
2. `src/backend/WebAPI/appsettings.Development.json` already points at `localhost:5432`.
3. Apply migrations (the API also applies them on startup):

   ```bash
   cd src/backend/Infrastructure
   dotnet ef database update --context CommonDbContext
   dotnet ef database update --context FloorballDbContext
   dotnet ef database update --context FootballDbContext
   dotnet ef database update --context HockeyDbContext
   ```

4. Run the API:

   ```bash
   cd src/backend/WebAPI
   dotnet run
   ```

   Local Kestrel ports are `https://localhost:65532` and `http://localhost:65533`.

5. Frontend (from `src/frontend`):

   ```bash
   pnpm install
   pnpm dev
   ```

   `src/frontend/.env.development` defaults to `VITE_API_URL=http://localhost:8080/api` (Docker). For a local `dotnet run` API, change it to `http://localhost:65533/api` and restart Vite.

## API overview

Interactive docs: `/scalar/v1` (Development). OpenAPI JSON: `/swagger/v1/swagger.json`.

### Auth (`/api/auth`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | Request a 6-digit login code |
| POST | `/api/auth/verify` | Exchange email + code for JWT + refresh token |
| POST | `/api/auth/refresh` | Rotate refresh token |
| POST | `/api/auth/logout` | Revoke refresh token |
| GET | `/api/auth/me` | Current user |

### Common

| Resource | Base URL |
|----------|----------|
| Clubs | `/api/clubs` |
| Club admin | `/api/club-admin` |
| Divisions | `/api/divisions` |
| Persons | `/api/persons` |
| Users | `/api/users` |
| News | `/api/news` |
| Search | `/api/search` |
| Rules | `/api/rulessection` |
| Info pages | `/api/infopagecontent` |
| Match timer | `/api/matches/{matchId}/timer` |

### Sports

| Sport | Teams / people | Competitions | Matches | Statistics |
|-------|----------------|--------------|---------|------------|
| Floorball | `/api/floorballteam`, `/api/floorballplayer`, `/api/floorballreferee` | `/api/floorballseason`, `/api/floorballtournament` | `/api/floorball-matches` | `/api/floorball/statistics` |
| Football | `/api/footballteam`, `/api/footballplayer`, `/api/footballreferee` | `/api/footballseason`, `/api/footballtournament` | `/api/football-matches` | `/api/football/statistics` |
| Hockey | `/api/hockeyteam`, `/api/hockeyplayer`, `/api/hockeyofficial` | `/api/hockeyseason`, `/api/hockeytournament` | `/api/hockeymatch` | `/api/HockeyStatistics` |

Match events, officials, lineups/rosters, and lifecycle sit under the match routes (for example `/api/floorball-matches/{id}/events`).

### System

| Endpoint | Description |
|----------|-------------|
| `/health` | Detailed health (JSON) |
| `/health/ready` | Readiness (includes database) |
| `/health/live` | Liveness |
| `/health-ui` | Health Checks UI |
| `/api/version` | Build date + git SHA |
| `/api/hubs/domainevent` | SignalR hub (JWT via `access_token` query) |

## Database seeding

[`src/tools/Seeder`](src/tools/Seeder/README.md) calls the running WebAPI over HTTP. It is idempotent and is the fastest way to get a usable database after `docker compose up` or a volume reset.

```bash
# Floorball (default)
dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=all

# Football 5v5 hobby set
dotnet run --project src/tools/Seeder/Seeder.csproj -- --sport=football --scope=all

# Hockey pipeline
dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=hockey
```

Without `--scope`, the tool prompts for phases (persons, clubs, teams, seasons, matches, tournaments, …) and resolves dependencies. It authenticates through the Development login flow.

Single-tournament import in production (no console tool): **Admin → Floorball → Tournaments → Import from JSON**.

### Other tools

| Tool | Purpose |
|------|---------|
| [Seeder](src/tools/Seeder/README.md) | Dev/test dataset via HTTP |
| [FloorballPlayerImporter](src/tools/FloorballPlayerImporter/README.md) | Roster JSON → players and teams |
| [DataImporter](src/tools/DataImporter/README.md) | Persons from legacy `.jlg` XML |
| [JoomleagueImporter](src/tools/JoomleagueImporter/README.md) | JoomLeague SQL dump → floorball or football |
| [MahlImporter](src/tools/MahlImporter/README.md) | Scrape and import historical MAHL data |
| [TournamentExporter](src/tools/TournamentExporter/README.md) | Pull live floorball tournaments into import JSON |

## Testing

```bash
dotnet test
dotnet test --collect:"XPlat Code Coverage"
dotnet test tests/backend/Domain.UnitTests/
```

Frontend: `pnpm lint` and `pnpm build` in `src/frontend`.

## Docker

```bash
docker compose up -d
docker compose logs -f webapi
docker compose down
docker compose down -v          # wipe the database volume
docker compose up --build
```

Services: `webapi` (8080), `frontend` (5173), `postgres` (5432), `seq` (5341).

## Authentication

No passwords are stored.

1. `POST /api/auth/login` with an email.
2. A 6-digit code is generated (10 minute lifetime, locked after 5 failed attempts).
   - **Local `dotnet run`:** code is written to the console. Development also sets `LoginCode:AutoFillLoginCode` so the login response can include the code.
   - **Docker:** find the code in `docker compose logs -f webapi`. Seed admin email is `test@myleague.fi`.
   - **Azure:** Azure Communication Services Email. Keep `LoginCode__AutoFillLoginCode=false` on any public environment.
3. `POST /api/auth/verify` returns a JWT and a refresh token.
4. Send `Authorization: Bearer <accessToken>` on protected routes. SignalR uses `?access_token=`.
5. Refresh rotates the refresh token; reuse of a revoked token revokes all tokens for that user.

Default seed users:

| Environment | Email | Notes |
|-------------|-------|--------|
| Local Development | `test@myleague.local` | Created on first startup (Admin) |
| Docker | `test@myleague.fi` | `Seed__AdminEmail` in compose override |
| Azure | `SEED_ADMIN_EMAIL` env var | Optional; created if missing |

Production JWT defaults: 15 minute access token, 7 day refresh token. Development uses longer lifetimes (60 minutes / 30 days).

## Azure and CI/CD

Staging deploys automatically from `development`. Production deploys are manual and gated by reviewers.

| Workflow | Role |
|----------|------|
| `backend-ci.yaml` / `frontend-ci.yaml` | Build, test, lint on `master` and `development` |
| `protect-master.yml` | PRs to `master` must come from `development` |
| `infra-deploy.yml` | Bicep provision (OIDC) |
| `deploy-backend.yml` / `deploy-frontend.yml` | App deploy + smoke tests |

Full environment map, costs, OIDC setup, and alerts: [`infra/README.md`](infra/README.md).

Release path: feature branch → PR into `development` → PR from `development` into `master`.

## Internationalization

Frontend locales: **Finnish** (default) and **English** under `src/frontend/src/i18n/locales/`. Add a language by adding a JSON file and registering it in the i18n setup.

## Contributing

1. Branch from `development`.
2. Follow the layer development guides next to each backend README.
3. Keep new work in the existing feature-slice folders.
4. Add tests for domain rules and handlers.
5. Open a PR into `development`.

## Roadmap

Done:

- Clean Architecture backend with CQRS
- Floorball and football public + admin UI
- Hockey domain, API, and seeder
- Passwordless auth and refresh-token rotation
- Live match flow with SignalR
- News, calendar, statistics, tournament import
- Docker Compose, Seq, Azure infra, GitHub Actions CI/CD

Next:

- Public ice hockey UI
- Scale-out SignalR (Redis or Azure SignalR) if the App Service plan goes beyond one instance
- Richer reporting / analytics
- Rate limiting beyond match-event limits

## Resources

- [Domain](src/backend/Domain/README.md)
- [Application](src/backend/Application/README.md)
- [Infrastructure](src/backend/Infrastructure/README.md)
- [WebAPI](src/backend/WebAPI/README.md)
- [Frontend](src/frontend/README.md)
- [Infrastructure (Azure)](infra/README.md)
- [Seeder](src/tools/Seeder/README.md)

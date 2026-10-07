# MyLeague agent guide

League management for floorball, football, and ice hockey. Clean Architecture + CQRS (MediatR) on .NET 10 + PostgreSQL. React 18 + Vite frontend.

Branch from `development`. PRs target `development`. Only `development` may merge into `master` (enforced by CI).

This file is the shared entry point for every coding agent. Claude Code loads it through `CLAUDE.md`, and Cursor loads it through `.cursor/rules/myleague.mdc`. Detailed rules and workflows live in `.claude/`, the single source of truth. Edit them there, not in tool-specific copies.

| Task | Read |
|------|------|
| Any change | `.claude/rules/architecture.md` |
| C# in `src/backend` or `src/tools` | `.claude/rules/backend.md` |
| EF Core / PostgreSQL | `.claude/rules/database.md` |
| React / TypeScript / i18n | `.claude/rules/frontend.md` |
| Tests | `.claude/rules/testing.md` |
| New vertical slice | `.claude/skills/create-feature/SKILL.md` |
| New or changed HTTP endpoint | `.claude/skills/create-api-endpoint/SKILL.md` |
| Schema change / migration | `.claude/skills/ef-migration/SKILL.md` |
| Run the app locally | `.claude/skills/run-local/SKILL.md` |
| Review / PR feedback | `.claude/skills/review-code/SKILL.md` |

Layer details: [Domain](src/backend/Domain/README.md) · [Application](src/backend/Application/README.md) · [Infrastructure](src/backend/Infrastructure/README.md) · [WebAPI](src/backend/WebAPI/README.md) · [Frontend](src/frontend/README.md).

## Layout

```
MyLeague.sln                solution (backend, tests, tools)
src/backend/Domain          entities, value objects, enums, repository interfaces, PagedResult
src/backend/Application     Features/<Area>/<Feature>/{Commands,Queries,Handlers,DTOs,Mappings,Validators}
src/backend/Infrastructure  four DbContexts, repositories, Fluent configs, migrations, SignalR
src/backend/WebAPI          thin controllers → IMediator → ApiResponse
src/frontend                React 18 + Vite + SCSS + i18next (fi + en), pnpm
src/tools                   Seeder (calls the HTTP API), importers, TournamentExporter
tests/backend               Domain / Application / WebAPI unit tests + Infrastructure integration (EF InMemory)
tests/tools                 importer unit tests
```

## Non-negotiables

- Business rules live on domain entities. Controllers map HTTP → command/query → `Handle*Result`. Handlers orchestrate and do not own invariants.
- Vertical slices go under `Application/Features/{Auth,Common,Floorball,Football,Hockey}/`. Shared DTOs go in `Features/Common/Shared`.
- No AutoMapper. Use static mappers next to the feature (`ToDto`, `ToDtos`, `ToEntity`, `UpdateFromCommand`).
- Return `Result<T>` / `PagedResult<T>`. The payload is `.Data`, not `.Value`. Do not throw for expected failures. Use `Result<T>.NotFound(...)` for missing entities, which maps to 404.
- Catch explicit exception types only. Rethrow `OperationCanceledException`. No `catch (Exception)` and no bare `catch`.
- Four DbContexts, one database. Cross-context links are Guid FKs with ignored navigations. Seasons and tournaments are TPH on `*Competition` (`CompetitionType`). Matches and stats key on `competitionId`.
- Writes require `[Authorize(Roles = AuthRoles.AdminOnly)]` or `AuthRoles.ClubAdminOrAdmin`. Public GETs stay anonymous and must not expose private person data.
- The three sports are peers with public, admin, and club-admin UI. Keep them in step.
- C#: explicit types, no `var` in new code (`.editorconfig`). TypeScript: no `React.FC`, no `any`. User-facing strings go through i18next in both `fi` and `en`. Call the API with `authFetch`.
- Never set `LoginCode:AutoFillLoginCode` (or `LoginCode__AutoFillLoginCode`) to true outside local development.
- Match timer state is in memory. Do not assume scale-out without a Redis or Azure SignalR backplane.

## Checks before you say "done"

CI runs these; run the ones for the side you changed:

```bash
dotnet build MyLeague.sln -c Release
dotnet test MyLeague.sln
cd src/frontend && pnpm lint && pnpm build
```

The husky pre-commit hook runs `lint-frontend.mjs` on staged frontend files.

## Keep docs current

Docs are part of the change. If your change makes a README, rule, or skill wrong or incomplete, update that file in the same change. Do this without being asked. At the end, tell the user which docs you updated, or that none needed it.

| If you change… | Check and update |
|----------------|------------------|
| Setup, ports, compose services, env vars, the tech stack, branching | [README.md](README.md), `.claude/skills/run-local/SKILL.md`, this file |
| Domain entities, value objects, enums, repository interfaces | [Domain README](src/backend/Domain/README.md), [DomainGlossary.md](src/backend/Domain/DomainGlossary.md) |
| Feature areas, `Result` / pipeline behaviors, validation, services | [Application README](src/backend/Application/README.md) |
| DbContexts, configurations, repositories, migrations workflow, SignalR, DI | [Infrastructure README](src/backend/Infrastructure/README.md), `.claude/rules/database.md`, `.claude/skills/ef-migration/SKILL.md` |
| Controllers, auth, middleware, rate limits, health checks, error mapping | [WebAPI README](src/backend/WebAPI/README.md), `.claude/skills/create-api-endpoint/SKILL.md` |
| Frontend structure, scripts, routing, i18n, auth flow, styling | [Frontend README](src/frontend/README.md), `.claude/rules/frontend.md` |
| A tool's arguments, config, or behavior | that tool's `src/tools/<Tool>/README.md` |
| Azure resources, deploy workflows, secrets, environments | [infra/README.md](infra/README.md) |
| A convention or a recurring review finding | the matching `.claude/rules/*.md` or skill |

How to write docs:

- Write only what is true now, and check it against the code. Do not add roadmap items, "coming soon" notes, or a changelog.
- Keep each fact in one place and link to it from elsewhere. Rules and skills hold conventions; READMEs hold orientation, setup, and structure.
- Never put secret values in docs. Refer to config keys and secrets by name.

## Running locally

Postgres + Seq run in Docker (`docker compose up -d postgres seq`). The API runs on the host at `:8080` and the UI with Vite at `:5173`. Never run the Compose `webapi`/`frontend` services for local testing. For the full procedure, the port-5432 workaround, and seeding, see `.claude/skills/run-local/SKILL.md`.

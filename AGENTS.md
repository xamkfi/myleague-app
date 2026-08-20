# MyLeague agent guide

League management for floorball, football, and ice hockey. Clean Architecture + CQRS (MediatR). Branch from `development`; PRs target `development`, not `master`.

Before implementing, load the matching file:

| Task | Read |
|------|------|
| Any change | `.cursor/rules/architecture.mdc` |
| Domain / Application / WebAPI / Infrastructure C# | `.cursor/rules/backend.mdc` |
| React / TypeScript / i18n | `.cursor/rules/frontend.mdc` |
| Tests | `.cursor/rules/testing.mdc` |
| EF Core / PostgreSQL / migrations | `.cursor/rules/database.mdc` |
| New vertical slice | `.cursor/skills/create-feature/SKILL.md` |
| New or changed HTTP endpoint | `.cursor/skills/create-api-endpoint/SKILL.md` |
| Schema change | `.cursor/skills/ef-migration/SKILL.md` |
| Review / PR feedback | `.cursor/skills/review-code/SKILL.md` |

Layer details: [Domain](src/backend/Domain/README.md) · [Application](src/backend/Application/README.md) · [Infrastructure](src/backend/Infrastructure/README.md) · [WebAPI](src/backend/WebAPI/README.md) · [Frontend](src/frontend/README.md)

Prefer those READMEs over the older `*DevelopmentGuide.md` files. The guides still mention event sourcing, AutoMapper, and `Result.Value`; the code does not.

## Layout

```
src/backend/Domain          entities, value objects, enums, repository interfaces
src/backend/Application     Features/<Area>/<Feature>/{Commands,Queries,Handlers,DTOs,Mappings,Validators}
src/backend/Infrastructure  four DbContexts, repositories, Fluent configs, migrations, SignalR
src/backend/WebAPI          thin controllers → IMediator, ApiResponse
src/frontend                React 18 + Vite + Tailwind 4 + i18next (fi default)
src/tools                   Seeder (HTTP), importers, TournamentExporter
tests/backend               Domain / Application / WebAPI unit tests + Infrastructure integration
```

## Non-negotiables

- Keep business rules on domain entities. Controllers map HTTP → command/query → `HandleResult` / `HandlePaginatedResult`. Handlers orchestrate; they do not own invariants.
- Vertical slices under `Application/Features/{Auth,Common,Floorball,Football,Hockey}/`. Shared DTOs go in `Features/Common/Shared`.
- No AutoMapper. Static mappers next to the feature (`ToDto`, `ToDtos`, `ToEntity`, `UpdateFromCommand`).
- Return `Result<T>` / `PagedResult<T>`. Payload is `.Data`, not `.Value`. Do not throw for expected business failures. Use `Result<T>.NotFound(...)` so WebAPI can map to 404.
- Four DbContexts, one PostgreSQL database. Cross-context navigations are ignored; store foreign key Guids only. Seasons/tournaments are TPH on `*Competition` (`CompetitionType` discriminator). Matches and stats use `competitionId` for both.
- Auth: `[Authorize(Roles = AuthRoles.AdminOnly)]` or `AuthRoles.ClubAdminOrAdmin`. Public GETs stay anonymous unless the sibling sport already requires auth.
- Frontend: no `React.FC`. Explicit types, no `any` / `var` in new C#. User-facing strings go through i18next (`fi` + `en`). Call the API with `authFetch`.
- Ice hockey: backend + seeder exist; do not enable a public hockey UI unless the user asks.
- Never set `LoginCode:AutoFillLoginCode` (or `LoginCode__AutoFillLoginCode`) to true on a public environment.
- Match timer state is in-memory. Do not add scale-out assumptions without a Redis / Azure SignalR backplane.

## Commands

```bash
# API (Docker): http://localhost:8080  Scalar: /scalar/v1
docker compose up -d
dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=all

# API (Kestrel): https://localhost:65532  http://localhost:65533
# then set VITE_API_URL=http://localhost:65533/api in src/frontend/.env.development

dotnet test
cd src/frontend && pnpm lint && pnpm build
```

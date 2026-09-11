---
name: create-feature
description: >-
  Adds a MyLeague vertical slice across Domain, Application CQRS, Infrastructure,
  WebAPI, tests, and optionally the React UI. Use when creating a new feature,
  resource, sport entity, admin page, or when the user says add a feature or
  new slice (club, team, season, tournament, match event, news, and similar).
---

# Create a MyLeague feature

Copy an existing sibling in the same sport (floorball team, football season, hockey match) rather than inventing a new folder shape.

## Progress

```
- [ ] 1. Domain entity + glossary
- [ ] 2. Repository interface
- [ ] 3. Application CQRS slice
- [ ] 4. EF config + repository + migration
- [ ] 5. WebAPI controller + request models
- [ ] 6. Tests
- [ ] 7. Frontend (if the feature is user-visible)
```

## 1. Domain

Place the type under `src/backend/Domain/Entities/{Common|Floorball|Football|Hockey}/`.

- Inherit `BaseEntity`
- Private setters, private parameterless ctor for EF, public constructor with invariants
- Mutate through named methods (`UpdateName`, `Complete`, …), not public setters
- Enums: `Domain/Enums/{area}`; value objects: `Domain/ValueObjects/{area}`
- Competitions: extend that sport's `*Competition` (TPH). Do not create a parallel season-only hierarchy
- Add the term to `src/backend/Domain/DomainGlossary.md`
- Repository contract: `Domain/Repositories/{area}/I{Entity}Repository.cs`

Do **not** add event-sourced aggregates or an `EventSourcing` folder.

## 2. Application

Create `src/backend/Application/Features/{Area}/{Feature}/`:

| Folder | Files |
|--------|--------|
| `Commands/` | `CreateXCommand`, `UpdateXCommand`, `DeleteXCommand` as records `IRequest<Result<T>>` |
| `Queries/` | `GetXByIdQuery`, `GetAllXQuery` (`Page`, `PageSize`) |
| `DTOs/` | `XDto`, `XSummaryDto` as records |
| `Mappings/` | static `XMapper` (`ToDto`, `ToDtos`, `ToEntity`, `UpdateFromCommand`) |
| `Validators/` | FluentValidation for every command/query |
| `Handlers/` | one handler per command/query |

Handler outline:

1. Load required aggregates; `return Result<T>.NotFound(...)` or `Failure(...)` when missing
2. Call domain methods / mapper `ToEntity`
3. `await repository.AddAsync` (or update)
4. `await unitOfWork.SaveChangesAsync(cancellationToken)` — `IUnitOfWork` (common) or `IFloorballUnitOfWork` / `IFootballUnitOfWork` / `IHockeyUnitOfWork`
5. `return Result<T>.Success(mapper.ToDto(...))`
6. Catch unexpected exceptions, log, return `Failure` with a generic message

No AutoMapper. No extra DI registration for handlers or validators.

## 3. Infrastructure

- `IEntityTypeConfiguration<T>` in `Persistence/Configurations/{area}/`
- Ignore cross-context navigations; store `Guid` FKs
- Implement the repository; register `AddScoped<I{Entity}Repository, {Entity}Repository>()` in `Infrastructure/DependencyInjections/DependencyInjection.cs`
- Add `DbSet<T>` on the correct context only
- Then follow `.cursor/skills/ef-migration/SKILL.md`

## 4. WebAPI

Follow `.cursor/skills/create-api-endpoint/SKILL.md`. New resource: new controller under `Controllers/{area}/` inheriting `BaseApiController`.

## 5. Tests

| Project | Add |
|---------|-----|
| Domain.UnitTests | entity invariants |
| Application.UnitTests | handler success/failure + validator theories + mapper |
| WebAPI.UnitTests | controller mapping if the action is non-trivial |

See `.cursor/rules/testing.mdc`.

## 6. Frontend (public or admin UI)

Skip for hockey public pages. Otherwise:

- `src/frontend/src/api/{area}/{entity}Service.ts` using `authFetch` + `parseErrorResponse`
- Page under `src/frontend/src/pages/...` with colocated SCSS and `components/`
- Keys in **both** `i18n/locales/fi/translation.json` and `en/translation.json`
- Lazy route in `src/frontend/src/router/routes.tsx` (`lazyWithRetry`). Protect admin/club-admin with `ProtectedRoute`
- Function component with explicit props type — no `React.FC`

## Sport checklist

Adding something that already exists for another sport: copy that sport's slice (names, routes, TPH, match events) and swap the sport prefix. Do not invent a third pattern.

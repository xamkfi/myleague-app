---
name: review-code
description: >-
  Reviews MyLeague diffs against Clean Architecture, CQRS, EF, React, and test
  conventions. Use when reviewing a pull request, examining uncommitted changes,
  or when the user asks for a code review, review comments, or merge feedback.
---

# Review MyLeague code

Read the diff first (`git diff` / PR files). Then walk the layers that actually changed. Cite file paths. Do not nitpick historical route names or existing `React.FC` unless the change touches that file.

## Severity

- **Critical** — must fix before merge (wrong layer, data loss, auth hole, broken TPH)
- **Suggestion** — should fix (inconsistent slice, missing validator/test)
- **Nice to have** — optional

## Architecture

- Controllers/handlers do not reference EF types or other context's entities
- New code sits in `Features/{Auth|Common|Floorball|Football|Hockey}/`, not a new top-level folder
- No AutoMapper, no fifth DbContext, no event-sourcing store
- Seasons/tournaments still share `*Competition` TPH and `competitionId`
- Cross-context relations are Guids + `Ignore` navigations, not `Include` across contexts
- A feature added to one sport is either mirrored to the other two or the gap is deliberate
- `LoginCode:AutoFillLoginCode` remains false outside local/dev

## Domain / Application

- Invariants on the entity, not only in FluentValidation
- Commands vs queries split; `IRequest<Result<T>>`; payload `.Data`
- Static mapper next to the feature; related entities passed in (not assumed loaded)
- Expected failures via `Result.Failure` / `NotFound`, not thrown exceptions. Missing entities use `NotFound` so they return 404
- No `catch (Exception)` or bare `catch`. `OperationCanceledException` is rethrown
- Sport unit of work matches the context (`IFloorballUnitOfWork` vs `IUnitOfWork`)
- Glossary updated for new ubiquitous-language terms

## WebAPI

- Inherits `BaseApiController` and uses Handle* helpers
- Mutations authorized with `AuthRoles.AdminOnly` or `ClubAdminOrAdmin`. A plain `[Authorize]` on a write is a finding
- Anonymous GETs do not leak private person data (see `IncludePrivateData` on `BaseApiController`)
- Paged lists use `PaginatedApiResponse` + page/pageSize bounds
- XML comments + `ProducesResponseType`
- User input in logs goes through `SanitizeForLog`

## Database

- Configuration in the matching `Persistence/Configurations/{area}` folder
- Migration `--context` and `--output-dir` pair is correct (`FloorBallDb` spelling). No new files at the root of `Migrations/`
- `Up` does not drop unrelated tables; discriminator values stay `"Season"` / `"Tournament"`

## Frontend

- No new `React.FC` or `any`
- `authFetch` + `parseErrorResponse`
- i18n keys in **both** `fi` and `en`
- New screens lazy-loaded in `router/routes.tsx`; admin routes behind `ProtectedRoute`

## Tests

- Domain rules and handler success/failure covered when behavior changed
- Validator theories for new input rules
- Application tests mock repositories, not DbContext

## Docs

- READMEs, rules, and skills affected by the change are updated (see "Keep docs current" in `AGENTS.md`). A stale command, path, setting, or type name in docs is a **Suggestion**
- New domain terms are in `DomainGlossary.md`
- No secret values in docs

## Output format

```markdown
## Summary
[one paragraph: what the change does and whether it matches MyLeague patterns]

## Findings
- Critical: `path` — issue — what to do instead
- Suggestion: `path` — issue
- Nice to have: `path` — issue

## What looks good
- [specific files/patterns]
```

If there are no issues, say so and list what was checked. Do not invent problems.

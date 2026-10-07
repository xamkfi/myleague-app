---
paths:
  - "src/backend/**/*.cs"
  - "src/tools/**/*.cs"
---

# Backend

.NET 10, nullable reference types on. Use explicit types, not `var` (`.editorconfig` sets `csharp_style_var_*` to `false`; the build does not enforce it). Do not rewrite `var` in lines you are not otherwise changing. Handlers, validators, and MediatR behaviors are assembly-scanned. Do not register each handler by hand.

## Application slice

```
Features/<Area>/<Feature>/
  Commands/  Queries/  Handlers/  DTOs/  Mappings/  Validators/
```

- Command or query: `record ... : IRequest<Result<TDto>>` (or `Result<PagedResult<TDto>>`)
- `Result<T>` / `Result` are in `Application/Common/Result.cs`. Payload is `.Data`. Factories: `Success`, `Failure`, `ValidationFailure`, `NotFound(entityName, key)`
- `PagedResult<T>` is in `Domain/Common/PagedResult.cs` (`PagedResult.Create`, `PagedResult.Empty`)
- Mapper: static `ToDto` / `ToDtos` / `ToEntity` / `UpdateFromCommand`. Pass related aggregates as arguments, because navigations may be unloaded
- Validator: FluentValidation on every command or query that takes input
- Handler: load via repository → domain method → `IUnitOfWork` / `IFloorballUnitOfWork` / `IFootballUnitOfWork` / `IHockeyUnitOfWork` → map → `Result<T>.Success`
- Missing entity: `Result<T>.NotFound(...)`. It sets `ResultErrorKind.NotFound`, which WebAPI maps to 404. A plain `Failure` maps to 400

## Exceptions

Do not throw for expected business failures. Return a `Result`. Catch only the types the code can actually throw. GitHub code quality flags `catch (Exception)` and bare `catch`.

```csharp
// BAD
try { await work(); }
catch (Exception ex) { return Result<T>.Failure(ex.Message); }

// GOOD
try { await work(); }
catch (OperationCanceledException) { throw; }
catch (InvalidOperationException ex) { return Result<T>.Failure(ex.Message); }
catch (ArgumentException ex) { return Result<T>.Failure(ex.Message); }

try { JsonSerializer.Serialize(input); }
catch (JsonException) { fallback(); }
```

- Rethrow cancellation. Do not map it to `Result.Failure`
- Use `catch (Exception ex) when (...)` only if you cannot list the types
- Let unexpected exceptions (for example EF `DbUpdateException`) propagate to the API exception middleware

## Domain

Entities inherit `BaseEntity` (`Id`, `CreatedAt`, `UpdatedAt`). Use private setters, a private ORM constructor, and a public constructor plus named methods for state changes. Value objects are immutable records with validation. Repository interfaces live in `Domain/Repositories/{Common|Floorball|Football|Hockey}`. Add new terms to `Domain/DomainGlossary.md`.

## WebAPI

Inherit `Controllers/Common/BaseApiController`. Map request → MediatR → `HandleResult` / `HandlePaginatedResult` / `HandleListResult` / `HandleVoidResult` (failures go through `ToErrorResponse`). Wrap payloads in `ApiResponse<T>` / `PaginatedApiResponse<T>`. Mutations use `[Authorize(Roles = AuthRoles.AdminOnly)]` or `AuthRoles.ClubAdminOrAdmin`. Put XML comments on actions (OpenAPI). Log with structured properties, and pass user strings through `SanitizeForLog`.

```csharp
Result<ClubDto> result = await _mediator.Send(new GetClubByIdQuery(id));
return HandleResult(result, "Club retrieved successfully", "Club not found");
```

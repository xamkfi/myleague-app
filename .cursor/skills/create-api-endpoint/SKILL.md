---
name: create-api-endpoint
description: >-
  Adds or changes a MyLeague ASP.NET Core endpoint: controller action, request
  model, MediatR command/query, ApiResponse mapping, and auth. Use when adding
  an HTTP route, REST action, controller method, or when the user mentions
  GET/POST/PUT/DELETE, Scalar, or OpenAPI.
---

# Create an API endpoint

Controllers are HTTP adapters. They must not contain business rules or EF calls.

Reference implementations: `ClubsController`, `FloorballTeamController`, `FootballMatchesController`.

## Progress

```
- [ ] Command/query + validator + handler exist (or created)
- [ ] Request record in WebAPI/Models/{area}/
- [ ] Action on the right controller (or new controller)
- [ ] Auth attribute chosen
- [ ] OpenAPI attributes + XML comments
- [ ] Controller unit test if mapping is non-trivial
```

## Placement and routes

| Area | Folder | Typical route |
|------|--------|----------------|
| Auth | `Controllers/Auth` | `/api/auth/...` |
| Common | `Controllers/Common` | `/api/clubs`, `/api/news`, … |
| Floorball | `Controllers/Floorball` (+ `Match/`) | `/api/floorballteam`, `/api/floorball-matches` |
| Football | `Controllers/Football` (+ `Match/`) | `/api/footballteam`, `/api/football-matches` |
| Hockey | `Controllers/Hockey` | `/api/hockeyteam`, `/api/hockeymatch` |

Keep the existing route style for that resource (some are `[controller]`, some are kebab-case literals). Do not "fix" historical names.

Inherit `WebAPI.Controllers.Common.BaseApiController`.

## Action pattern

```csharp
[HttpGet("{id:guid}")]
[ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status404NotFound)]
public async Task<ActionResult<ApiResponse<ClubDto>>> GetClubById(Guid id)
{
    _logger.LogInformation("Getting club with ID: {ClubId}", id);
    Result<ClubDto> result = await _mediator.Send(new GetClubByIdQuery(id));
    return HandleResult(result, "Club retrieved successfully", "Club not found");
}
```

| Situation | Helper |
|-----------|--------|
| Single payload | `HandleResult` |
| Paged list | `HandlePaginatedResult` → `PaginatedApiResponse<T>` |
| Unpaged sequence | `HandleListResult` |
| Delete / no body | `HandleVoidResult` |
| Create | on success `CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, ApiResponse<T>.SuccessResponse(...))`; on failure `ToErrorResponse` |

404 vs 400 is derived from whether the result error contains `"not found"` (see `Result<T>.NotFound`). Do not return 500 for domain/query failures.

List endpoints take `Page` / `PageSize` (`[Range]`, page size 0–100). Default page size 25 unless the sibling endpoint differs.

## Request models

Records in `WebAPI/Models/{Auth|Common|Floorball|Football|Hockey}/`. DataAnnotations for API-boundary checks (`[Required]`, `[StringLength]`, `[Range]`). Map field-by-field into the Application command/query — do not pass the request type into Application.

## Auth

- Public reads: no `[Authorize]` (clubs, news, public match/season GETs)
- Site admin writes: `[Authorize(Roles = AuthRoles.AdminOnly)]`
- Club-scoped writes: `[Authorize(Roles = AuthRoles.ClubAdminOrAdmin)]` plus `IClubAdminAccessService` when the sibling does
- Hockey mutating endpoints today use `[Authorize]` — match that when extending hockey

JWT bearer. SignalR uses `access_token` query string, not this skill.

## Logging and docs

- Structured logs: `"Creating club: {ClubName}"`, not interpolated strings as the template
- User-controlled strings: `SanitizeForLog(request.Name)`
- XML `<summary>` on the controller and each action (OpenAPI / Scalar)

## Frontend caller

If the UI must consume it: add or extend `src/frontend/src/api/...` with `authFetch` and `parseErrorResponse`. Expect `{ success, data, message, errors }` or the paginated envelope (`items` / `data` + pagination metadata).

# Application layer

CQRS orchestration between WebAPI and Domain. Commands and queries go through MediatR. Handlers load aggregates through repository interfaces, call domain methods, save through a unit of work, map to DTOs, and return `Result<T>`.

The project references Domain only. It also references `Microsoft.EntityFrameworkCore`, which handlers use to catch `DbUpdateException`; handlers never use a `DbContext`.

Conventions: [backend rules](../../../.claude/rules/backend.md#application-slice) and [architecture rules](../../../.claude/rules/architecture.md). There is no AutoMapper.

## Folder map

```
Application/
├── Behaviors/                LoggingBehavior, ValidationBehavior (ValidationBehaviors.cs)
├── Common/                   Result.cs, BasePagedQueryHandler, ExceptionExtensions (Flatten),
│                             PublicCompetitionVisibility, logo URL and UTC helpers
├── Configuration/            Jwt, LoginCode, AzureCommunicationServices, Seed, Frontend,
│                             Pagination, PeriodDuration
├── Constants/                FloorballNotificationEvents, FootballNotificationEvents (SignalR event names)
├── DTOs/Common/              FooterContactDto, InfoPageContentDto, RulesSectionDto (older shared DTOs)
├── DependencyInjections/     AddApplication()
├── Interfaces/
│   ├── Auth/                 IEmailService, IJwtTokenService
│   └── Common/               IImageStorageService, INotificationSenderService, IMatchNotification,
│                             IPersonNameProvider, ISiteSettingsProvider
├── Services/Common/          IPaginationService, IClubAdminAccessService (+ implementations)
└── Features/
    ├── Auth/                 Commands, DTOs, Handlers, Validators (no queries)
    ├── Common/
    │   ├── Organization/     ClubAdmin, Clubs, DataSubjectRights, Deletion, Divisions,
    │   │                     Persons, PlayerLicences, Users
    │   ├── Content/          FooterContacts, Images, InfoPageContent, News, RulesSection
    │   ├── CrossCutting/     MatchTimer, Search
    │   ├── SiteSettings/
    │   ├── Statistics/       AllTimePlayerStatistics (shared all-time sorting and paging)
    │   └── Shared/DTOs/      AddressDto, ContactInfoDto, MatchPersonDto, ...
    ├── Floorball/            Competitions, Matches, Players, Referees, Seasons, Statistics,
    │                         TeamManagers, Teams, Tournaments
    ├── Football/             same slices as Floorball
    └── Hockey/               Competitions, Matches, Officials, Players, Seasons, Statistics,
                              Teams, Tournaments
```

A slice uses whichever of `Commands/`, `Queries/`, `Handlers/`, `DTOs/`, `Mappings/`, `Validators/` it needs. A few older Common slices put the handler in the command or query file.

## Key types

| Type | Location | Notes |
|------|----------|-------|
| `Result<T>`, `Result` | `Common/Result.cs` | Payload is `.Data`. Factories: `Success`, `Failure`, `ValidationFailure`, `NotFound(entityName, key)`. Also `IsSuccess`, `Error`, `ErrorKind`, `GetAllErrors()` |
| `ResultErrorKind` | `Common/Result.cs` | `None`, `Failure`, `NotFound`, `Validation`. WebAPI maps `NotFound` to 404 and `Validation` to 400 |
| `PagedResult<T>` | `Domain/Common/PagedResult.cs` | Lives in Domain so repositories can return it |
| `BasePagedQueryHandler<TQuery, TResult>` | `Common/BasePagedQueryHandler.cs` | Validates page and page size against `IPaginationService` |
| `ExceptionExtensions.Flatten()` | `Common/ExceptionExtensions.cs` | Turns an exception chain into `"Type: message"` strings for `Result.Failure(msg, errors)` |
| `IPaginationService` | `Services/Common/` | Reads `Pagination:Global` and `Pagination:Resources:<ResourceKey>` from appsettings. Page size 0 means "use the default" |
| `IClubAdminAccessService` | `Services/Common/` | `CanManageClubAsync(personId, clubId)`: is this person an active manager of the club |
| `IMatchTimerService`, `ITimerStore` | `Features/Common/CrossCutting/MatchTimer/Services/` | Timer logic; the store is in-memory (Infrastructure) |

## Pipeline and registration

`AddApplication()` in `DependencyInjections/DependencyInjection.cs`:

- `AddMediatR` scans the assembly for handlers and adds two behaviors in this order: `LoggingBehavior`, then `ValidationBehavior`.
- `AddValidatorsFromAssembly` registers every FluentValidation validator.
- Scoped services: `IPaginationService`, `IClubAdminAccessService`, `IPersonDeletionGuard`, `IMatchTimerService`.

`ValidationBehavior` runs all validators for the request. On failure it returns `Result<T>.ValidationFailure(...)` (or `Result.ValidationFailure`) without calling the handler. It throws `ValidationException` only when the response type is not a `Result`.

`LoggingBehavior` logs start, end, and duration, and warns when a request takes more than 1 s. At Debug level it also serializes the request and response. `appsettings.json` pins `Application.Behaviors.LoggingBehavior` to Information so payloads stay out of the logs.

## Example slice

```csharp
// Features/Common/Organization/Clubs/Queries/GetClubByIdQuery.cs
public record GetClubByIdQuery(Guid ClubId) : IRequest<Result<ClubDto>>;

// Features/Common/Organization/Clubs/Handlers/GetClubByIdHandler.cs
public async Task<Result<ClubDto>> Handle(GetClubByIdQuery request, CancellationToken cancellationToken)
{
    Club? club = await _clubRepository.GetByIdAsync(request.ClubId);
    if (club == null)
    {
        return Result<ClubDto>.NotFound("Club", request.ClubId);
    }

    ClubDto clubDto = ClubMapper.ToDto(club);
    return Result<ClubDto>.Success(clubDto);
}
```

Paged queries declare a resource key that matches an `appsettings.json` entry:

```csharp
// Features/Floorball/Teams/Queries/GetAllFloorballTeamsQuery.cs (filters trimmed)
public record GetAllFloorballTeamsQuery(
    int Page = 1,
    int PageSize = 0, // 0 means use default from configuration
    Guid? ClubId = null,
    string? SearchTerm = null
) : IRequest<Result<PagedResult<FloorballTeamDto>>>
{
    public const string ResourceKey = "FloorballTeams"; // Pagination:Resources:FloorballTeams
}
```

## Adding a feature

Use [create-feature](../../../.claude/skills/create-feature/SKILL.md). Copy the closest sibling slice in the same sport. For a mirrored sport feature, copy the floorball or football slice and keep names parallel.

## Auth slice

| Command | What it does |
|---------|--------------|
| `RequestLoginCodeCommand` | Creates a login code (`LoginCode:CodeLength`, default 6) and sends it through `IEmailService`. With `LoginCode:AutoFillLoginCode` on, returns the code instead |
| `VerifyLoginCodeCommand` | Checks the code, counts attempts in the database, issues an access token and a refresh token |
| `RefreshTokenCommand` | Rotates the refresh token. Reusing a revoked token revokes every token for that user |
| `RevokeTokenCommand` | Logout |
| `VerifyAdminEmailCommand` | Activates an invited admin from the email token |

Token lifetimes, login-code expiry, and maximum attempts come from `ISiteSettingsProvider`. It uses the `SiteSettings` row when one exists, and the `Jwt` and `LoginCode` configuration sections otherwise.

## Rules and pitfalls

- Business rules belong on the entity. Handlers orchestrate; validators check input shape.
- Return `Result<T>.NotFound(...)` for a missing entity. A plain `Failure` maps to 400, and matching "not found" in the text is only a fallback for old handlers.
- Many older handlers wrap everything in `catch (Exception)`. Do not copy that. Catch the specific types and rethrow `OperationCanceledException` ([backend rules](../../../.claude/rules/backend.md#exceptions)).
- Navigations across DbContexts are not loaded. Pass related aggregates (club, person) into mappers as arguments.
- Live updates for floorball and football go through `INotificationSenderService` with event names from `Constants/`. Hockey match handlers send none; the hockey live pages poll REST.

## Tests

`tests/backend/Application.UnitTests/ApplicationTestProject`: `Handlers/` (by area), `Validators/`, `Mappings/`, `Features/`. Mock repositories and unit-of-work interfaces with Moq. See [testing rules](../../../.claude/rules/testing.md).

```bash
dotnet test tests/backend/Application.UnitTests/ApplicationTestProject
```

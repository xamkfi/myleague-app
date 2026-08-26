# Application layer

CQRS orchestration for MyLeague. This layer sits between WebAPI and Domain: it validates input, runs commands and queries through MediatR, and returns `Result<T>` / `PagedResult<T>`.

See the [root README](../../../README.md) and [ApplicationDevelopmentGuide.md](./ApplicationDevelopmentGuide.md).

## Design

Code is organized as **vertical slices**. Each feature folder owns its Commands, Queries, Handlers, DTOs, Mappings, and Validators. Shared types live under `Features/Common/Shared`. Cross-cutting MediatR behaviors live at the project root.

There is no AutoMapper. Mappings are explicit classes next to the feature.

## Technology

- .NET 9
- MediatR 12.5
- FluentValidation 12
- Microsoft.Extensions.Logging / DependencyInjection

## Structure

```
Application/
├── Features/
│   ├── Auth/                 # Login code, verify, refresh, revoke
│   ├── Common/
│   │   ├── Clubs / ClubAdmin / Divisions / Persons / Users
│   │   ├── News / Images / Search
│   │   ├── InfoPageContent / RulesSection / FooterContacts
│   │   ├── MatchTimer / TeamLeader
│   │   └── Shared/           # PagedResult and other shared DTOs
│   ├── Floorball/            # Matches, Players, Referees, Seasons,
│   │                         # Statistics, TeamManagers, Teams, Tournaments
│   ├── Football/             # Same slice as floorball
│   └── Hockey/               # Competitions, Matches, Officials, Players,
│                             # Seasons, Statistics, Teams, Tournaments
├── Behaviors/                # Validation + logging pipelines
├── Common/                   # Result pattern
├── Configuration/            # Jwt, LoginCode, ACS, Seed, Frontend, Pagination
├── DependencyInjections/
├── Interfaces/               # IEmailService, IJwtTokenService, image storage, …
└── Application.csproj
```

Not every feature has every folder (Auth has no queries; Search is query-only).

## CQRS sketch

```csharp
public record CreateClubCommand(string Name, string Description, Address Address)
    : IRequest<Result<ClubDto>>;

public record GetClubByIdQuery(Guid ClubId) : IRequest<Result<ClubDto>>;

public class CreateClubCommandHandler : IRequestHandler<CreateClubCommand, Result<ClubDto>>
{
    public async Task<Result<ClubDto>> Handle(CreateClubCommand request, CancellationToken cancellationToken)
    {
        // Create domain entity → persist via repository → map to DTO → Result.Success
    }
}
```

Registration:

```csharp
builder.Services.AddApplication();
```

Controllers send requests through `IMediator`; they do not call repositories.

## Auth commands

| Command | Role |
|---------|------|
| `RequestLoginCodeCommand` | Generate a 6-digit code and send it via `IEmailService` |
| `VerifyLoginCodeCommand` | Validate code (max 5 attempts) and issue JWT + refresh token |
| `RefreshTokenCommand` | Rotate refresh token; reuse of a revoked token revokes all tokens for the user |
| `RevokeTokenCommand` | Logout |

Configuration types: `JwtConfiguration`, `LoginCodeConfiguration`, `AzureCommunicationServicesConfiguration`, `SeedConfiguration`.

## Testing

Unit tests live in `tests/backend/Application.UnitTests/`. Cover handlers, validators, and mapping.

## Contributing

1. Add or extend `Features/<Area>/<FeatureName>/`.
2. Put shared DTOs in `Features/Common/Shared/`.
3. Use FluentValidation on every command/query that accepts input.
4. Return `Result<T>` or `PagedResult<T>` — do not throw for expected business failures.

This project is part of MyLeague. See the [root README](../../../README.md).

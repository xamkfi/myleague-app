# Domain layer

The business model for MyLeague: entities, value objects, enums, domain services, and repository interfaces for floorball, football, and ice hockey. The three sports are peers with the same shape.

`Domain.csproj` references no other project and no runtime packages. It only adds `Microsoft.CodeAnalysis.NetAnalyzers` with `AnalysisMode=All` and `EnforceCodeStyleInBuild=true`. Application and Infrastructure depend on this project, never the other way round.

Conventions: [backend rules](../../../.claude/rules/backend.md#domain) and [architecture rules](../../../.claude/rules/architecture.md). Terms: [DomainGlossary.md](./DomainGlossary.md).

## Folder map

```
Domain/
├── Common/            PagedResult<T>, all-time stat rows, PlayerLicenceCalendar, CurrentRosterTeamPicker
├── Constants/         AuthRoles, loan player / loan goalkeeper names and jersey numbers
├── Entities/
│   ├── BaseEntity.cs
│   ├── Common/        Club, ClubManager, Division, FooterContact, InfoPageContent, NewsArticle,
│   │                  Person, RefreshToken, RulesSection, SiteSettings, TimerState, User
│   ├── Floorball/     Competitions/  Matches/ (+ Events/)  Officials/  Statistics/  Teams/
│   ├── Football/      Competitions/  Matches/  Officials/  Statistics/  Teams/
│   └── Hockey/        Competitions/  Matches/ (+ Events/)  Officials/  Statistics/  Teams/
├── Enums/             Common/  Floorball/  Football/  Hockey/{Competitions,Matches,Statistics,Teams}
├── ValueObjects/      Common/ (Address, ContactInfo, EmailAddress)  Floorball/  Football/
│                      Hockey/{Common,Matches,Rules}
├── Repositories/      Common/  Floorball/  Football/  Hockey/   (interfaces + unit-of-work interfaces)
├── Services/          Common/ (standings order)  Floorball/ (standing points)
│                      Hockey/ (validation, statistics, coach challenge)
└── DomainGlossary.md
```

Namespaces follow folders, for example `Domain.Entities.Hockey.Teams`. Sport types carry the sport prefix (`FloorballTeam`, `FootballMatch`, `HockeyMatchStatus`). Football match events (`FootballGoal`, `FootballCard`, `FootballSubstitution`) sit directly in `Entities/Football/Matches/`; the other two sports use a `Matches/Events/` subfolder.

## Key types

| Type | Location | Notes |
|------|----------|-------|
| `BaseEntity` | `Entities/BaseEntity.cs` | `Id` (Guid, set in the constructor), `CreatedAt`, `UpdatedAt?`, `SetUpdatedAt` |
| `PagedResult<T>`, `PagedResult.Create`, `PagedResult.Empty` | `Common/PagedResult.cs` | Record with `Items`, `TotalCount`, `Page`, `PageSize`, `TotalPages` |
| `AuthRoles` | `Constants/AuthRoles.cs` | `SystemAdmin`, `ClubAdmin`, `AdminOnly`, `ClubAdminOrAdmin` |
| `FloorballCompetition` / `FootballCompetition` / `HockeyCompetition` | `Entities/<Sport>/Competitions/` | Base of `<Sport>Season` and `<Sport>Tournament`, stored as TPH |
| `SiteSettings` | `Entities/Common/SiteSettings.cs` | Single row: token and login-code timings, yearly player-licence cutoff |
| `IUnitOfWork`, `IFloorballUnitOfWork`, `IFootballUnitOfWork`, `IHockeyUnitOfWork` | `Repositories/<Area>/` | One per DbContext |
| `I<Entity>Repository` | `Repositories/<Area>/` | Implemented in `Infrastructure/Persistence/Repositories/<Area>/` |

Matches, standings, and statistics take a `competitionId`, so the same code serves league seasons and tournaments.

## Sports

| Sport | Shape |
|-------|-------|
| Floorball | Periods, goals, penalties, saves, referees, scorekeepers, active roster, seasons and tournaments with playoffs |
| Football | Configurable rules (`FootballMatchRules`: 1–4 halves of 1–60 min, 5–11 players on field, substitutions, extra time); goals, cards, substitutions, lineups; `FootballStandingRules` defaults 3–1–0 |
| Ice hockey | Lines, on-ice changes, faceoffs, shots, penalties, goalie changes, shootouts, video review, coach challenges, playoff series, staff |

## Adding to the domain

Follow step 1 of [create-feature](../../../.claude/skills/create-feature/SKILL.md). In short:

1. Put the entity under `Entities/<Area>/` and inherit `BaseEntity`.
2. Use private setters, a private parameterless constructor for EF, and a public constructor that checks invariants. Change state through named methods.
3. Add enums to `Enums/<Area>/` and value objects to `ValueObjects/<Area>/` (immutable, validated in the constructor).
4. Add the repository interface to `Repositories/<Area>/`.
5. Add the term to [DomainGlossary.md](./DomainGlossary.md).
6. If the sport has siblings, check whether the other two sports need the same change.

## Rules and pitfalls

- No EF Core, ASP.NET Core, MediatR, or DTO types in this project.
- Broken invariants throw `ArgumentException` (or a subclass) or `InvalidOperationException`. Handlers catch those types and return a `Result` failure.
- Cross-context references are Guids (`ClubId`, `PersonId`, `DivisionId`). Some entities also expose a navigation to another context, for example `FloorballTeam.Club` and `FloorballPlayer.Person`. EF ignores these, so they are null after a load. Load the related aggregate through its own repository instead.
- There is no event sourcing, `AggregateRoot`, or `EventSourcing` folder. Match events are ordinary persisted entities.
- Do not add a new competition hierarchy. New competition kinds extend the sport's `*Competition`.

## Tests

`tests/backend/Domain.UnitTests/DomainTestProject`: `Common/`, `Floorball/`, `Football/`, and `Hockey/` folders plus a few cross-sport files at the root. Cover constructors, invariants, and state transitions. See [testing rules](../../../.claude/rules/testing.md).

```bash
dotnet test tests/backend/Domain.UnitTests/DomainTestProject
```

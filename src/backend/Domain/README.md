# Domain layer

Core business model for MyLeague. This project has no infrastructure or UI dependencies. It defines entities, value objects, enums, and repository contracts used by Application and Infrastructure.

See the [root README](../../../README.md) for how this layer fits the solution, and [FeatureDevelopmentGuide.md](./FeatureDevelopmentGuide.md) when adding a feature.

## Design

- **Entities** — Identity and lifecycle (clubs, persons, teams, matches, competitions)
- **Value objects** — Immutable descriptors (addresses, match rules, standing rules)
- **Aggregates** — Cluster related objects; persist through repository interfaces
- **Enums** — Sport-specific statuses, positions, event types, lifecycle states

Seasons and tournaments inherit a sport-specific `*Competition` base (`FloorballCompetition`, `FootballCompetition`, `HockeyCompetition`) and are stored with EF Core TPH. Matches and statistics reference a `CompetitionId`, so the same query stack works for league seasons and tournaments.

Match **events** (goals, penalties, cards, shots, and so on) are persisted entities, not an event-sourced store. There is no `EventSourcing` folder in this project.

## Technology

- .NET 9 / C# 13
- Nullable reference types
- Microsoft.CodeAnalysis analyzers

## Structure

```
Domain/
├── Entities/
│   ├── Common/          # Person, Club, User, Division, News, Rules, Info pages, FooterContact, …
│   ├── Floorball/       # Flat layout: teams, matches, competitions, stats
│   ├── Football/        # Competitions, Matches, Teams, Statistics
│   └── Hockey/          # Competitions, Matches, Teams, Statistics
├── ValueObjects/        # Common + per-sport (rules, addresses, match values)
├── Enums/               # Common + per-sport
├── Repositories/        # Interfaces only
├── Services/            # Domain service contracts (hockey and shared)
├── Constants/
└── DomainGlossary.md    # Ubiquitous language
```

Hockey types use the `Hockey` prefix (`HockeyTeam`, `HockeyMatchStatus`). Namespaces follow folders, for example `Domain.Entities.Hockey.Teams`.

## Sports

| Sport | Status in domain | Notes |
|-------|------------------|--------|
| Floorball | Primary | Full match events, periods, referees, TPH competitions |
| Football | Hobby / complete slice | Configurable half length and players-on-field (5v5–11v11); cards, substitutions, extra time, shootouts; `FootballStandingRules` (default 3–1–0) |
| Ice hockey | Complete backend model | Lines, on-ice, faceoffs, shots, penalties, goalie tracking; public UI is not enabled yet |

### Typical aggregate roots

- **Club**, **Person**, **User**, **Division**
- **FloorballSeason** / **FloorballTournament**, **FloorballMatch**, **FloorballTeam**, **FloorballPlayer**, **FloorballReferee**
- Parallel football and hockey competition, match, team, and player roots

## Build and test

```bash
dotnet build
dotnet test tests/backend/Domain.UnitTests/
```

## Contributing

1. Keep business rules on entities; do not leak EF or HTTP types here.
2. Update [DomainGlossary.md](./DomainGlossary.md) when you add terms.
3. Add unit tests for state transitions and invariants.
4. Follow the existing per-sport folder layout.

This project is part of MyLeague. See the [root README](../../../README.md).

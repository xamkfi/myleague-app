---
paths:
  - "src/backend/Infrastructure/**"
---

# Database

PostgreSQL 16 via Npgsql. One database, four contexts. Map with the Fluent API only (`IEntityTypeConfiguration<T>` under `Persistence/Configurations/{Common,Floorball,Football,Hockey}`). Do not use data annotations for mapping.

| Context | Migrations folder | Typical tables |
|---------|-------------------|----------------|
| `CommonDbContext` | `Migrations/CommonDb` | Person, Club, User, RefreshToken, Division, News, Rules, Info pages |
| `FloorballDbContext` | `Migrations/FloorBallDb` | TPH competitions, teams, matches, stats |
| `FootballDbContext` | `Migrations/FootballDb` | same shape as floorball |
| `HockeyDbContext` | `Migrations/HockeyDb` | same shape as floorball |

Never put a migration at the root of `Migrations/`. Two old CommonDb migrations sit there (`AddUserRole`, `AddEmailVerificationToUser`). Leave them where they are.

## Mapping rules

- `HasKey`, max lengths, conversions (`Uri` → string; enums → string where existing configs do)
- TPH: `HasDiscriminator<string>("CompetitionType").HasValue<FloorballSeason>("Season").HasValue<FloorballTournament>("Tournament")`
- Owned value objects: `OwnsOne` with explicit column names (`MatchRules_NumberOfPeriods`)
- Cross-context: `builder.Ignore` navigations, and persist `ClubId` / `PersonId` / `DivisionId` as Guids
- Apply only that sport's configurations inside each `OnModelCreating`
- Register repositories in `DependencyInjections/DependencyInjection.cs`

At startup, `AddInfrastructure` runs `Database.Migrate()` for all four contexts. To create a migration, use the `ef-migration` skill (`.claude/skills/ef-migration/SKILL.md`).

---
name: ef-migration
description: >-
  Creates and applies Entity Framework Core migrations for MyLeague's four
  PostgreSQL DbContexts. Use when adding or changing entities, Fluent API
  mappings, columns, indexes, TPH discriminators, or when the user mentions
  migration, EF Core, schema, or database update.
---

# EF Core migration

Four contexts, **one** PostgreSQL database (`DefaultConnection`). Never mix entities across contexts. Never generate a migration without `--context`.

## Which context?

| Change lives in | `--context` | `--output-dir` |
|-----------------|-------------|----------------|
| Person, Club, User, RefreshToken, Division, News, Rules, Info pages, ClubManager, TimerState | `CommonDbContext` | `Migrations/CommonDb` |
| Floorball competitions (TPH), teams, matches, stats, referees | `FloorballDbContext` | `Migrations/FloorBallDb` |
| Football competitions, teams, matches, stats | `FootballDbContext` | `Migrations/FootballDb` |
| Hockey competitions, teams, matches, stats | `HockeyDbContext` | `Migrations/HockeyDb` |

Folder name `FloorBallDb` is historical — keep it.

## Before generating

1. Put the entity on the correct `DbSet`
2. Add `IEntityTypeConfiguration<T>` under `Persistence/Configurations/{area}/`
3. Call `ApplyConfiguration` only from that context's `OnModelCreating`
4. Cross-context navigations: `builder.Ignore(...)` and a `Guid` FK
5. TPH: discriminator `CompetitionType` with values `"Season"` / `"Tournament"` on the sport `*Competition` configuration
6. Owned value objects: `OwnsOne` + explicit column names (`MatchRules_*`)

Design-time factories: `Persistence/Contexts/*DbContextFactory.cs` (Npgsql). Run commands from `src/backend/Infrastructure` so the factory can resolve `appsettings`.

## Commands

From `src/backend/Infrastructure`:

```bash
dotnet ef migrations add AddClubManagers --context CommonDbContext --output-dir Migrations/CommonDb --startup-project ../WebAPI/WebAPI.csproj

dotnet ef migrations add AddFloorballPlayoffBracket --context FloorballDbContext --output-dir Migrations/FloorBallDb --startup-project ../WebAPI/WebAPI.csproj

dotnet ef migrations add AddFootballStatisticsNavigations --context FootballDbContext --output-dir Migrations/FootballDb --startup-project ../WebAPI/WebAPI.csproj

dotnet ef migrations add AddHockeyOnIceTracking --context HockeyDbContext --output-dir Migrations/HockeyDb --startup-project ../WebAPI/WebAPI.csproj
```

Name: `PascalCase` verb + thing (`AddClubManagers`, `MakePersonBirthDateNullable`). Do not add `InitialCreate` unless the context has no migrations.

## After generating

1. Read the `Up`/`Down` methods. Reject empty migrations, unexpected drops, or tables from another context
2. Confirm TPH did not create a second competitions table
3. Apply locally:

```bash
dotnet ef database update --context CommonDbContext --startup-project ../WebAPI/WebAPI.csproj
# repeat for FloorballDbContext, FootballDbContext, HockeyDbContext as needed
```

The API also runs `Database.Migrate()` for all four contexts at startup (`AddInfrastructure`). `docker compose up` is enough after a volume that already exists; a new empty volume gets all migrations on first boot.

```bash
dotnet ef migrations remove --context CommonDbContext --startup-project ../WebAPI/WebAPI.csproj --force
```

Only remove a migration that has **not** been applied anywhere shared. Never rewrite applied migration files; add a follow-up migration.

## Do not

- Use `--output-dir Migrations/Floorball` (wrong folder)
- Point two contexts at the same entity type
- Hand-edit snapshots to "fix" cross-context discovery — fix the configuration with `Ignore` instead
- Commit a migration that was generated against the wrong connection string

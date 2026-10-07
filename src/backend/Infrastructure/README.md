# Infrastructure layer

Implements the Domain repository interfaces and the Application service interfaces: EF Core persistence on PostgreSQL, JWT and login-code email, image storage, SignalR notifications, the in-memory match timer store, health checks, startup migrations, and startup seeding.

Root namespace: `MyLeague.Infrastructure`. References Domain and Application.

Conventions: [database rules](../../../.claude/rules/database.md), [backend rules](../../../.claude/rules/backend.md), [architecture rules](../../../.claude/rules/architecture.md).

## Folder map

```
Infrastructure/
├── DependencyInjections/
│   ├── DependencyInjection.cs                 AddInfrastructure(configuration)
│   └── DomainEventServiceCollectionExtensions.cs   AddDomainEvents(): SignalR + INotificationSenderService
├── Persistence/
│   ├── Contexts/          CommonDbContext, FloorballDbContext, FootballDbContext, HockeyDbContext
│   │                      + one IDesignTimeDbContextFactory per context
│   ├── Configurations/    BaseEntityConfiguration.cs + Common/ Floorball/ Football/ Hockey/
│   ├── Repositories/      RepositoryBase.cs + Common/ Floorball/ Football/ Hockey/
│   ├── UnitOfWork/        Common/Floorball/Football/Hockey unit of work + UniqueConstraint
│   ├── Extensions/        DbContextExtensions (audit timestamps on save)
│   └── Seeding/           InfoPageContent, FooterContact, RulesSection, NewsArticle seeders
├── Services/
│   ├── Auth/              JwtTokenService, ConsoleLoginCodeEmailService, AzureCommunicationEmailService
│   ├── Common/            InMemoryTimerStore, TimerNotificationService, SiteSettingsProvider + cache,
│   │                      PersonNameProvider, PlayerLicenceResetBackgroundService, TimerBackgroundService (not registered)
│   ├── ImageStorage/      LocalFileImageStorageService, AzureBlobImageStorageService
│   └── Seeding/           DatabaseSeeder (users)
├── SignalR/               DomainEventHub, DomainEventNotifier, SignalRNotificationSender
├── HealthChecks/          HealthCheckExtensions, DatabaseHealthCheck, ApplicationServicesHealthCheck
└── Migrations/            CommonDb/  FloorBallDb/  FootballDb/  HockeyDb/
```

## Persistence

Four DbContexts share one PostgreSQL database and the `DefaultConnection` connection string.

| Context | Unit of work | Migrations folder |
|---------|--------------|-------------------|
| `CommonDbContext` | `CommonUnitOfWork` : `IUnitOfWork` | `Migrations/CommonDb` |
| `FloorballDbContext` | `FloorballUnitOfWork` : `IFloorballUnitOfWork` | `Migrations/FloorBallDb` |
| `FootballDbContext` | `FootballUnitOfWork` : `IFootballUnitOfWork` | `Migrations/FootballDb` |
| `HockeyDbContext` | `HockeyUnitOfWork` : `IHockeyUnitOfWork` | `Migrations/HockeyDb` |

Which tables belong to which context: see the [database rules](../../../.claude/rules/database.md).

`AddInfrastructure` configures all four contexts the same way:

- Npgsql with `EnableRetryOnFailure` (3 retries, up to 2 s delay).
- `Max Pool Size` capped at 20. Azure PostgreSQL Burstable B1ms allows about 50 connections in total.
- `HockeyDbContext` ignores `PendingModelChangesWarning`, because a nested owned type leaves a permanent snapshot difference.

### Repositories and unit of work

- Repositories derive from `RepositoryBase<TEntity, TContext>` (`GetByIdAsync`, `GetAllAsync`, `FindAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `ExistsAsync`) and implement the Domain interface. `AddAsync`, `UpdateAsync`, and `DeleteAsync` only change tracking; nothing is written until the unit of work saves.
- Every unit of work saves through `UniqueConstraint.SaveChangesAsync`. A PostgreSQL unique violation (23505) becomes an `InvalidOperationException`, which handlers already catch.
- Register each new repository in `DependencyInjections/DependencyInjection.cs`.

### Audit fields

`BaseEntity` sets `Id` and `CreatedAt` in its constructor. All four contexts override `SaveChangesAsync` and call `SaveChangesWithEventsAsync` (`Persistence/Extensions/DbContextExtensions.cs`). That method sets `UpdatedAt` on modified entities and stops `CreatedAt` from changing. Despite the name, it dispatches no events.

`BaseEntityConfiguration<T>` maps `Id`, `CreatedAt`, and `UpdatedAt` and adds audit indexes. Derived configurations override `ConfigureEntity`. About 35 of the 85 configuration files use it; the rest implement `IEntityTypeConfiguration<T>` directly. Either is fine, but copy whatever the sibling configuration in the same sport does.

### Migrations

`AddInfrastructure` builds a temporary service provider and runs `Database.Migrate()` for Common, Floorball, Hockey, and Football, then the seeders. This runs during service registration, so the API does not start while PostgreSQL is unreachable.

To create or apply a migration, use the [ef-migration skill](../../../.claude/skills/ef-migration/SKILL.md). Run the commands from `src/backend/Infrastructure` and always pass `--context`, `--output-dir`, and `--startup-project ../WebAPI/WebAPI.csproj`:

```bash
dotnet ef migrations add AddClubManagers --context CommonDbContext --output-dir Migrations/CommonDb --startup-project ../WebAPI/WebAPI.csproj
dotnet ef database update --context CommonDbContext --startup-project ../WebAPI/WebAPI.csproj
```

The two migrations at the root of `Migrations/` (`AddUserRole`, `AddEmailVerificationToUser`) belong to `CommonDbContext`. Leave them there, and do not add new ones at the root.

## Services

| Interface | Implementation | Selection |
|-----------|----------------|-----------|
| `IEmailService` | `ConsoleLoginCodeEmailService` | Development environment. Logs `LOGIN CODE for <email>` instead of sending |
| `IEmailService` | `AzureCommunicationEmailService` | Any other environment. Reads `AzureCommunicationServices:*` |
| `IImageStorageService` | `LocalFileImageStorageService` | Development and no `ConnectionStrings:AzureBlobStorage`. Writes to `WebAPI/wwwroot/uploads`, builds URLs from `App:BaseUrl` |
| `IImageStorageService` | `AzureBlobImageStorageService` | Otherwise. Needs `ConnectionStrings:AzureBlobStorage` and `AzureStorage:ContainerName` |
| `IJwtTokenService` | `JwtTokenService` | Claims: `sub`, `email`, `jti`, `personId`, role, `personRole`. Refresh tokens are stored as SHA-256 hashes |
| `ISiteSettingsProvider` | `SiteSettingsProvider` | `SiteSettings` row if present, otherwise `Jwt` / `LoginCode` configuration. Cached in the singleton `SiteSettingsCache` |
| `ITimerStore` | `InMemoryTimerStore` | Singleton |
| `INotificationSenderService` | `SignalRNotificationSender` | Sends to the event-type group and, for match payloads, the match group |

Hosted services: `PlayerLicenceResetBackgroundService` runs `ResetExpiredPlayerLicencesCommand` at startup and every 6 hours. `TimerBackgroundService` exists but its registration is commented out.

### Seeding at startup

| Seeder | What it creates |
|--------|-----------------|
| `DatabaseSeeder` | Development: `test@myleague.local` (SystemAdmin) and `clubadmin@myleague.local` (ClubAdmin, linked to Tampere Titans if that club exists). Any environment: a SystemAdmin for `Seed:AdminEmail` if set |
| `InfoPageContentSeeder`, `FooterContactSeeder`, `RulesSectionSeeder`, `NewsArticleSeeder` | Default site content |

Sport data (clubs, teams, seasons, matches) comes from the HTTP seeder in `src/tools/Seeder`, not from here.

## SignalR and the match timer

- Hub: `DomainEventHub`, mapped in WebAPI at `/api/hubs/domainevent`. The JWT arrives in the `access_token` query string.
- Clients join groups with `SubscribeToEventTypeAsync` / `SubscribeToMatchAsync`. Messages are `DomainEvent` (event-type group) and `MatchEvent` (match group).
- `StartTimerAsync`, `StopTimerAsync`, and `ResetTimerAsync` require `AuthRoles.AdminOnly`.
- Live timer state is in `InMemoryTimerStore`; `TimerState` rows in CommonDb are written through `ITimerRepository`. Keep the App Service at one instance until a Redis or Azure SignalR backplane exists. See [infra/README.md](../../../infra/README.md).

## Health checks

`AddMyLeagueHealthChecks` (`HealthChecks/HealthCheckExtensions.cs`) registers every check. Checks tagged `ready` (PostgreSQL plus the four DbContexts) back `/health/ready`. Endpoints and the full check list: [WebAPI health checks](../WebAPI/HealthChecks-README.md).

## Tests

`tests/backend/Infrastructure.IntegrationTests/InfrastructureIntegrationTestProject`. Tests derive from `Common/BaseIntegrationTest` or a per-sport base (`FloorballIntegrationTestBase`, `FootballIntegrationTestBase`, `HockeyIntegrationTestBase`) and live under `Repositories/<Area>/`. They run on EF InMemory with one database per test, so they do not check PostgreSQL constraints or SQL. See [testing rules](../../../.claude/rules/testing.md).

```bash
dotnet test tests/backend/Infrastructure.IntegrationTests/InfrastructureIntegrationTestProject
```

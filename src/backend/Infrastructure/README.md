# Infrastructure layer

Concrete implementations for Domain and Application abstractions: persistence, auth delivery, image storage, SignalR, health checks, and startup seeding.

See the [root README](../../../README.md) and [InfrastructureDevelopmentGuide.md](./InfrastructureDevelopmentGuide.md).

## Technology

- .NET 9
- EF Core 9 + Npgsql (PostgreSQL 16)
- Azure.Communication.Email
- Azure.Storage.Blobs
- ASP.NET Core SignalR
- Health check packages (EF Core, Npgsql, system)

## Structure

```
Infrastructure/
├── Persistence/
│   ├── Contexts/            # Common, Floorball, Football, Hockey + design-time factories
│   ├── Repositories/
│   ├── Configurations/      # Fluent API mappings
│   ├── Extensions/
│   └── UnitOfWork/
├── Services/
│   ├── Auth/                # JWT, console email, Azure ACS email
│   ├── ImageStorage/        # Local files (dev) / Azure Blob (Azure)
│   ├── Common/              # In-memory match timer store + background tick
│   └── Seeding/             # DatabaseSeeder (admin user)
├── SignalR/                 # DomainEventHub, notifier, sender
├── HealthChecks/
├── DependencyInjections/
├── DTOs/
└── Migrations/              # Per-context migration folders
```

## Persistence

Four DbContexts share one PostgreSQL database and one connection string (`DefaultConnection`):

| Context | Typical contents |
|---------|------------------|
| `CommonDbContext` | Person, Club, User, RefreshToken, Division, News, Rules, Info pages |
| `FloorballDbContext` | Floorball competitions (TPH), teams, matches, stats |
| `FootballDbContext` | Football competitions (TPH), teams, matches, stats |
| `HockeyDbContext` | Hockey competitions (TPH), teams, matches, stats |

The API applies pending migrations at startup. Manual update:

```bash
dotnet tool install --global dotnet-ef   # once
cd src/backend/Infrastructure
dotnet ef database update --context CommonDbContext
dotnet ef database update --context FloorballDbContext
dotnet ef database update --context FootballDbContext
dotnet ef database update --context HockeyDbContext
```

## Auth and email

| Service | When |
|---------|------|
| `JwtTokenService` | Access token (userId, email, personId, role) + hashed refresh tokens |
| `ConsoleLoginCodeEmailService` | Development — prints `[LOGIN CODE]` |
| `AzureCommunicationEmailService` | Azure — ACS Email |
| `DatabaseSeeder` | Ensures `test@myleague.local` (SystemAdmin) and `clubadmin@myleague.local` (ClubAdmin of Tampere Titans when that club exists) in Development, plus optional `Seed:AdminEmail` |

Refresh tokens are stored as SHA256 hashes only.

## Real-time and timers

- Hub path: `/api/hubs/domainevent` (JWT via `access_token` query string).
- Match timer state is **in-memory** (`InMemoryTimerStore`). Keep App Service instances at 1 until a Redis / Azure SignalR backplane exists. See [`infra/README.md`](../../../infra/README.md).

## Images

- Local: files under WebAPI `wwwroot` (`LocalFileImageStorageService`)
- Azure: Blob Storage (`AzureBlobImageStorageService`)

## Registration

```csharp
builder.Services.AddInfrastructure(builder.Configuration);
```

## Contributing

1. Implement Domain repository interfaces; do not leak EF types upward.
2. Add migrations with the matching `--context`.
3. Cover persistence in `tests/backend/Infrastructure.IntegrationTests/`.

This project is part of MyLeague. See the [root README](../../../README.md).

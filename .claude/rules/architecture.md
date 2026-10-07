# Architecture

Dependency direction: **WebAPI / Frontend → Application → Domain**. Infrastructure implements Domain/Application interfaces. Controllers never call repositories or DbContexts.

## Feature slices

Put new work next to its sport or shared area:

- `Application/Features/Common/`
  - `Organization/`: clubs, persons, users, divisions
  - `Content/`: news and site pages
  - `CrossCutting/`: search and the match timer
  - `SiteSettings/`, `Statistics/`
  - `Shared/`: DTOs used by more than one area
- `Application/Features/{Floorball,Football,Hockey}/`: teams, players, officials, seasons, tournaments, matches, statistics
- Mirror the same split in `Domain/Entities`, `WebAPI/Controllers`, and `src/frontend/src/{api,pages}`

The three sports are peers with the same shape. When a feature is added to one sport, check whether the other two need it too. Copy the sibling sport's slice instead of inventing a new pattern.

Seasons and tournaments inherit a sport-specific `*Competition` and persist with **TPH**. Matches, standings, and stats take a `competitionId` that works for both league seasons and tournaments.

## Persistence

Four EF Core contexts share one PostgreSQL database (`DefaultConnection`): `CommonDbContext`, `FloorballDbContext`, `FootballDbContext`, `HockeyDbContext`. Do not add a navigation that would pull another context's entity into the model. Use `Guid` FKs and `builder.Ignore(...)`.

## Real-time and auth

Live match updates go through the SignalR hub `/api/hubs/domainevent` (JWT via `access_token`). Hockey live pages poll REST every 3–5 s instead. Sign-in is a passwordless email code, then JWT with refresh-token rotation. Roles are `SystemAdmin` and `ClubAdmin` (`Domain.Constants.AuthRoles`). Club-admin APIs live under `/api/club-admin/...`.

Match timer state is in memory. Do not assume more than one API instance unless a Redis or Azure SignalR backplane is added.

## Do not

- Introduce AutoMapper, a fifth DbContext, or event sourcing
- Put business rules that belong on the entity into controllers or FluentValidation

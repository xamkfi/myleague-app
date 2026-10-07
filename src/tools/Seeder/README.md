# Seeder

Fills an empty development database with floorball, football, and ice hockey test data through the HTTP API.

## When to use it

- You need a local or test database with clubs, teams, players, seasons, matches, and tournaments.
- You want the standard dataset that the rest of the team uses.

Use another tool for real data:

- Historical JoomLeague data: [JoomleagueImporter](../JoomleagueImporter/README.md).
- One tournament, season, or roster on a hosted environment: the admin UI import (for example **Admin → Floorball → Tournaments → Import from JSON**). The tournament schema is in `src/frontend/src/types/floorball/tournamentImportTypes.ts`. Sample files are next to it.

## Prerequisites

- .NET 10 SDK.
- WebAPI running with `ASPNETCORE_ENVIRONMENT=Development`. The root [AGENTS.md](../../../AGENTS.md) shows how to start it on `http://localhost:8080`.
- The API must have `LoginCode:AutoFillLoginCode = true`. `src/backend/WebAPI/appsettings.Development.json` already sets it.
- The user `test@myleague.local` must exist. The API creates it at startup in Development.

The Seeder does not touch the database directly. Every write goes through the API. It logs in as `test@myleague.local`: it calls `POST api/auth/login`, reads the code the API returns, and then calls `POST api/auth/verify`. If the API sends the code by email instead, the Seeder stops with "Failed to get dev login code". So it cannot run against a public environment, and that is intended.

## Usage

Run from the repository root.

```bash
# All floorball phases (the usual first run)
dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=all

# Full ice hockey pipeline
dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=hockey

# Floorball and hockey in one run
dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=all,hockey

# Football dataset
dotnet run --project src/tools/Seeder/Seeder.csproj -- --sport=football --scope=all

# Floorball, then football
dotnet run --project src/tools/Seeder/Seeder.csproj -- --sport=all --scope=all

# Only tournaments (prerequisites are added automatically)
dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=tournaments

# Several phases. Both "--scope=x" and "--scope x" work.
dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=persons,clubs,divisions
dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope persons,teams
```

The Seeder always asks for the API URL first. Press Enter to keep the configured default. In a script, pipe an empty line or redirect stdin from `/dev/null`. At end of input the Seeder uses the default.

Without `--scope` it then shows a numbered menu:

| Menu entry | Scope |
|------------|-------|
| `1` | Persons |
| `2` | Clubs |
| `3` | Divisions |
| `4` | PlayersReferees |
| `5` | Teams |
| `6` | Seasons |
| `7` | SeasonMatches |
| `8` | Tournaments |
| `9`, `all`, or blank | All (floorball) |
| `10`, `hockey`, or `hockeyall` | HockeyAll |

You can enter a comma-separated list such as `1,2,5`. After the selection the menu asks `Proceed? (Y/n)`. Three invalid inputs in a row end the run with exit code 2. `--scope` skips both the menu and the confirmation.

## Options

| Option | Values | Default | Meaning |
|--------|--------|---------|---------|
| `--scope=<list>` or `--scope <list>` | Comma-separated tokens, case-insensitive (see below) | Interactive menu | Phases to seed. Prerequisites are added automatically. |
| `--sport=<value>` or `--sport <value>` | `floorball`, `football`, `all` | `floorball` | Floorball and football data sets to run. `all` runs floorball first, then football. |

`--scope` tokens:

| Token | Seeds | Also pulls in |
|-------|-------|---------------|
| `persons` | Base persons from `Persons` | none |
| `clubs` | Clubs | none |
| `divisions` | Divisions | none |
| `playersreferees` | Player, goalie, and referee persons, then sport player and referee records | persons |
| `teams` | Teams and rosters | persons, clubs, divisions, playersreferees |
| `seasons` | Seasons, season intro cards, team-to-season assignment | persons, clubs, divisions, playersreferees, teams |
| `seasonmatches` | Season matches, including scorekeepers | everything up to seasons |
| `tournaments` | Tournaments, groups, and group-stage matches | persons, clubs, divisions, playersreferees, teams |
| `all` | Every floorball/football phase above. No hockey. | none |
| `hockeyplayers` | Hockey roster persons, hockey players, staff persons, and up to four hockey officials | persons |
| `hockeyteams` | Hockey teams, rosters, Line 1, Pair 1, and a head coach | persons, clubs, divisions, hockeyplayers |
| `hockeyseasons` | Hockey seasons and team assignment | everything up to hockeyteams |
| `hockeyseasonmatches` | Hockey season matches, then stats recalculation | everything up to hockeyseasons |
| `hockeytournaments` | Hockey tournaments and group-stage matches | persons, clubs, divisions, hockeyplayers, hockeyteams |
| `hockey`, `hockeyall` | Persons, clubs, divisions, and every hockey phase | none |

Hockey runs only when the scope has a hockey token. It always reads `data/testdata.json`, whatever `--sport` says.

## Configuration

Configuration is read from the build output folder (`bin/...`). Later sources override earlier ones:

1. `appsettings.json` (a small sample dataset and `Seeder:BaseUrl`).
2. `appsettings.Development.json`, if present. It is optional and not in the repo.
3. `data/testdata.json` for floorball and hockey, or `data/testdata-football.json` for football.
4. Environment variables, for example `Seeder__BaseUrl`.

Edit the data files to change what gets seeded. Everything is under the `Seeder` section:

| Key | Content |
|-----|---------|
| `BaseUrl` | API base URL. Default `http://localhost:8080/`. |
| `Persons`, `PlayerPersons`, `GoaliePersons`, `RefereePersons`, `StaffPersons`, `ScorekeeperPersons` | Person lists |
| `Clubs`, `Divisions` | Shared organisation data. Hockey divisions use `"SportType": "Icehockey"`. |
| `FloorballTeams`, `FloorballSeasons`, `FloorballMatches`, `FloorballTournaments` | Floorball data |
| `FootballTeams`, `FootballSeasons`, `FootballMatches`, `FootballTournaments` | Football data (`testdata-football.json`) |
| `HockeyTeams`, `HockeySeasons`, `HockeyMatches`, `HockeyTournaments` | Hockey data |

Seasons can have ordered `ContentBlocks` (intro cards). A season without them gets one "History data" card.

API URL precedence, highest first:

1. The answer to the interactive URL prompt.
2. The `SEEDER_BASEURL` environment variable.
3. A root-level `BaseUrl` key from any source, including a `BaseUrl` environment variable.
4. `Seeder:BaseUrl` (also `Seeder__BaseUrl`).
5. Built-in default `http://localhost:8080/`.

The prompt adds `http://` if you leave out the scheme, and it adds a trailing `/`.

## Input and output

- Input: the JSON files above. Football persons use `@fb.fi` and `@fb-ref.fi` emails, so `--sport=all` does not clash with floorball persons.
- Output: data in the target API, and a summary on the console. A phase outside the scope shows `(skipped)`.
- Match simulation differs by sport and phase:
  - Floorball season matches are created as scheduled. They are not played.
  - Floorball tournament matches dated in the past are played through the live event endpoints and completed. Future-dated ones stay `Scheduled`.
  - Football season and tournament matches dated in the past are played and completed.
  - Hockey matches are created in two steps: `POST api/HockeyMatch`, then `PUT api/HockeyMatch/{id}/teams`. Finished hockey matches get dressed rosters, an official, active goalies, faceoffs, shots, goals, penalties, and period scores. Stats are then recalculated.

Exit codes:

| Code | Meaning |
|------|---------|
| `0` | Success, or you answered `n` at the confirmation |
| `1` | Runtime failure (HTTP error, invalid JSON, login failure) |
| `2` | Invalid `--scope` or `--sport` value, or three invalid menu inputs |

## Caveats

- **Re-running.** Most phases check for existing records before they create anything. Persons are matched by email, or by name and birth date. Clubs are matched by name, divisions by name and sport, seasons by name and first division, tournaments by name, and teams by name, club, and division. Matches are matched by competition and home/away team. A second run should not duplicate them.
- **Team-to-season assignment is not idempotent.** Floorball and football post every season-division and team pair without checking first. If the API rejects a pair that is already assigned, a re-run of `seasons` (or `all`, or any scope that includes it) can stop. Workaround: drop the database and seed again, or use a scope without `seasons`. Hockey checks first, but a failed duplicate add can still stop the run.
- **Development only.** Do not point the Seeder at a shared or production API. It creates large amounts of fake data, and it needs the auto-fill login, which must stay off on public environments.

## Related

- [JoomleagueImporter](../JoomleagueImporter/README.md): historical JoomLeague SQL dumps
- [TournamentExporter](../TournamentExporter/README.md): copy real floorball tournaments between environments
- [Root README](../../../README.md)

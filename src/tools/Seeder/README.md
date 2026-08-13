## Seeder

Run ordered HTTP seeding against the WebAPI to create a complete development / test dataset. Phases can be selected interactively or via the `--scope=...` CLI flag, and dependencies are resolved automatically. Use `--sport=floorball|football|all` to choose which dataset to seed (default `floorball`).

> **Admins:** if you only need a single tournament populated (e.g. in production / Azure where running this console tool is impractical), use **Admin → Floorball → Tournaments → Import from JSON** in the web UI instead. It accepts a JSON file (one tournament per file), can also create player rosters and pre-schedule playoff matches automatically, shows live progress, and offers a one-click revert. A downloadable sample is linked from the modal. See `src/frontend/src/types/floorball/tournamentImportTypes.ts` for the schema; generate the actual import files with an AI prompt that targets that schema.

### Data Pipeline

The seeder creates entities in the following order. Each phase corresponds to a `SeedScope` flag (see [Scope Selection](#scope-selection)):

| # | Phase | `SeedScope` flag | Depends on |
|---|---|---|---|
| 1 | Persons (base + player / goalie / referee persons) | `Persons` | — |
| 2 | Clubs | `Clubs` | — |
| 3 | Divisions | `Divisions` | — |
| 4 | Floorball players + referees (entity registrations from person records) | `PlayersReferees` | `Persons` |
| 5 | Teams + roster assignments | `Teams` | `Persons`, `Clubs`, `Divisions`, `PlayersReferees` |
| 6 | Seasons + team-to-season-division assignments | `Seasons` | `Persons`, `Clubs`, `Divisions`, `PlayersReferees`, `Teams` |
| 7 | Season matches | `SeasonMatches` | all of the above |
| 8 | Tournaments + groups + tournament group-stage matches | `Tournaments` | `Persons`, `Clubs`, `Divisions`, `PlayersReferees`, `Teams` |

`Tournaments` is a single inseparable phase: tournament records and their group-stage matches are always seeded together by `FloorballTournamentMatchesSeeder`. Past-dated tournament matches are simulated through to completion so the tournament page has populated statistics; future-dated matches stay in `Scheduled` state.

### Scope Selection

When the seeder starts (after the URL prompt and before authentication), it shows an interactive menu:

```
==========================================================
Seeder - Scope Selection
==========================================================
What do you want to seed? (auto-resolves dependencies)

 1) Henkilöt (Persons) — base persons + player/goalie/referee persons
 2) Seurat (Clubs)
 3) Divisioonat (Divisions)
 4) Pelaajat ja tuomarit — needs 1
 5) Joukkueet (Teams + rosters) — needs 1, 2, 3, 4
 6) Kaudet (Seasons + team-to-season assignment) — needs 1, 2, 3, 5
 7) Kausi-ottelut (Season matches) — needs 1, 2, 3, 4, 5, 6
 8) Turnaukset ja turnausottelut — needs 1, 2, 3, 4, 5
 9) Kaikki (Everything)

Enter selection: comma-separated numbers (e.g. "1,2,5") or "9" / "all" / blank for all.
> 5

Selected: Teams. Auto-included: Persons, Clubs, Divisions, PlayersReferees.
Proceed? (Y/n):
```

Selecting any phase implicitly pulls in its prerequisites. Phases that aren't in scope appear as `(skipped)` in the final summary.

#### Non-interactive selection (`--scope`)

Pass `--scope=...` to skip both the menu and the proceed-confirmation. This is intended for CI/scripts:

```bash
# All phases (same as choosing "9" / "all" / blank in the menu)
dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=all

# Only seed tournaments (auto-pulls Persons, Clubs, Divisions, PlayersReferees, Teams)
dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=tournaments

# Multiple phases (comma-separated, case-insensitive). Both space and equals forms are accepted.
dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=persons,clubs,divisions
dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope persons,teams
```

Valid tokens (case-insensitive): `persons`, `clubs`, `divisions`, `playersreferees`, `teams`, `seasons`, `seasonmatches`, `tournaments`, `all`. Unknown tokens cause the seeder to exit with code `2`.

#### Sport selection (`--sport`)

`--sport` chooses which HTTP pipeline and data file to run. Default is `floorball`, so existing scripts keep their previous behaviour.

```bash
# Default: floorball from data/testdata.json
dotnet run --project src/tools/Seeder/Seeder.csproj -- --scope=all

# Football 5v5 hobby dataset from data/testdata-football.json
dotnet run --project src/tools/Seeder/Seeder.csproj -- --sport=football --scope=all

# Floorball first, then football (shared Persons/Clubs/Divisions seeders are idempotent)
dotnet run --project src/tools/Seeder/Seeder.csproj -- --sport=all --scope=all

# Space form is also accepted
dotnet run --project src/tools/Seeder/Seeder.csproj -- --sport football --scope tournaments
```

Valid values (case-insensitive): `floorball`, `football`, `all`. Unknown values cause the seeder to exit with code `2`.

Football persons use `@fb.fi` / `@fb-ref.fi` emails so `--sport=all` does not collide with floorball `@teamN.fi` persons.

### Configure

#### Using Test Data File (Recommended)

The seeder automatically loads comprehensive test data from `data/testdata.json` (floorball) and/or `data/testdata-football.json` (football) depending on `--sport`. These files contain persons, clubs, divisions, seasons, teams (with rosters), season matches and tournaments (with groups). They are the canonical data sets for local development and are what the menu/`--scope` selections operate against.

Each file is layered on top of `appsettings.json` and `appsettings.Development.json` via `ConfigurationBuilder`, so anything you put into either of those will be merged with the selected testdata file.

#### Using appsettings.json

You can also configure (or override) via `appsettings.json` / `appsettings.Development.json`:

```json
{
  "Seeder": {
    "BaseUrl": "http://localhost:8080/",
    "Persons": [ ... ],
    "PlayerPersons": [ ... ],
    "GoaliePersons": [ ... ],
    "RefereePersons": [ ... ],
    "Clubs": [ ... ],
    "Divisions": [ ... ],
    "FloorballSeasons": [ ... ],
    "FloorballTeams": [ ... ],
    "FloorballMatches": [ ... ],
    "FloorballTournaments": [ ... ]
  }
}
```

The `BaseUrl` can also be overridden by:
- The `SEEDER_BASEURL` environment variable.
- The interactive URL prompt at startup (press Enter to keep the configured default).

### Run

```bash
dotnet run --project src/tools/Seeder/Seeder.csproj
```

Ensure the WebAPI is running and the `BaseUrl` matches your development port. Authentication is performed automatically via the WebAPI's dev login flow (only available when the API runs in `Development` mode).

### Idempotency

All seeders perform idempotent checks before creating entities, so re-running the seeder (or running it with a partial scope against an existing database) does not create duplicates:

- Persons: checked by email or name + birthdate.
- Clubs: checked by name.
- Divisions: checked by name + sport type.
- Players / referees: checked by person ID.
- Seasons: checked by name + division.
- Teams: checked by name + club + division.
- Season matches: checked by season + home team + away team.
- Tournaments: checked by name; groups and team assignments are checked by name / team ID before insertion.

#### Known caveat

`FloorballTeamsSeeder.AssignTeamsToSeasonsAsync` POSTs every (season-division × team) pair without first checking whether the team is already assigned. If the WebAPI returns a non-2xx response for an already-assigned pair, selectively re-running the `Seasons` (or `all`) scope against an existing database can abort. Workarounds:

- Run a clean seed (drop the database first).
- Or run with a narrower scope that excludes `Seasons`.

### Summary output

When the run finishes, the seeder prints a column-aligned summary. Phases that weren't in scope show `(skipped)`:

```
Summary:
  Persons created:           42
  Clubs created:              4
  Divisions created:          2
  Floorball players created:  100
  Floorball referees created: 8
  Seasons created:            (skipped)
  Teams created:              10
  Matches created:            (skipped)
  Tournaments created:        (skipped)
  Tournament matches created: (skipped)
```

### Exit codes

- `0` — success (or user chose `n` at the proceed prompt).
- `1` — runtime failure (HTTP error, JSON parse error, etc.).
- `2` — invalid CLI args (unknown `--scope` token, unknown `--sport` value, or 3 consecutive invalid menu inputs).

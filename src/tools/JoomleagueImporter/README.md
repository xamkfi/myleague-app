# JoomleagueImporter

Imports historical floorball, football, or ice hockey data from a JoomLeague MySQL dump (`.sql`) into MyLeague through the HTTP API.

## When to use it

- You have a JoomLeague database dump (for example the MAHL site) and want its seasons in MyLeague.
- You need to resume or repair an earlier import.

Do not use it for development test data. Use the [Seeder](../Seeder/README.md) for that.

This tool replaces the older MahlImporter and DataImporter tools, which have been removed.

## Prerequisites

- .NET 10 SDK.
- A JoomLeague SQL dump file (UTF-8).
- A reachable WebAPI. The tool never connects to a database directly.
- One of these ways to log in:
  - **Local API:** run it in Development with `LoginCode:AutoFillLoginCode = true`. The tool logs in as `test@myleague.local`, or as the configured email, which must be an admin user.
  - **Remote API:** an access token and/or refresh token from a SystemAdmin session. Never turn on auto-fill login on a public environment to make this tool work.

## Usage

Run from the repository root.

```bash
# Show what would be imported. No API calls. Writes reports/expected-floorball.json.
dotnet run --project src/tools/JoomleagueImporter/JoomleagueImporter.csproj -- --dump=/path/to/dump.sql --dry-run

# Floorball into the local API (prompts for URL, login email, and confirmation)
dotnet run --project src/tools/JoomleagueImporter/JoomleagueImporter.csproj -- --dump=/path/to/dump.sql

# Football or hockey
dotnet run --project src/tools/JoomleagueImporter/JoomleagueImporter.csproj -- --dump=/path/to/dump.sql --sport=football
dotnet run --project src/tools/JoomleagueImporter/JoomleagueImporter.csproj -- --dump=/path/to/dump.sql --sport=hockey

# Only some JoomLeague projects
dotnet run --project src/tools/JoomleagueImporter/JoomleagueImporter.csproj -- --dump=/path/to/dump.sql --project-id=219,231

# Remote API without prompts. Read the tokens from environment variables. Never put them in a file in the repo.
dotnet run --project src/tools/JoomleagueImporter/JoomleagueImporter.csproj -- \
  --sport=floorball \
  --dump=/path/to/dump.sql \
  --api-url=https://<api-host>/ \
  --refresh-token="$REFRESH_TOKEN" \
  --id-map=/safe/place/id-map-<env>-floorball.json \
  --yes

# Re-send events for matches that were already imported
dotnet run --project src/tools/JoomleagueImporter/JoomleagueImporter.csproj -- --dump=/path/to/dump.sql --repair-matches=1119,1124
```

The run goes in this order:

1. Parse the dump and select projects.
2. Print the selection and write the expected-stats report.
3. Stop here if `--dry-run`.
4. Ask for the API URL, login email, and `Start import? [y/N]`, unless `--yes` is given.
5. Import clubs, persons and players, and teams. Pick or create the referee (hockey: official) used for every match.
6. Import each season and its matches.
7. Set current club memberships.
8. Write the compare report.

## Options

Every option accepts `--name=value` or `--name value`.

| Option | Default | Meaning |
|--------|---------|---------|
| `--sport` | `JoomleagueImporter:Sport`, else `floorball` | `floorball`, `football` (or `jalkapallo`), `hockey` (or `jääkiekko` / `jaakiekko`). Any other value means floorball. |
| `--dump` | `JoomleagueImporter:DumpFilePath` | Path to the SQL dump. The tool exits with code 1 if the file does not exist. |
| `--project-id` | `JoomleagueImporter:ProjectIdFilter` | Comma-separated JoomLeague project ids. Applied on top of the name filters, not instead of them. |
| `--api-url` | `JoomleagueImporter:ApiBaseUrl`, else `http://localhost:8080/` | Target API. Giving it skips the URL prompt. |
| `--access-token`, `--token` | `JoomleagueImporter:AccessToken` | JWT for a remote API. Skips the email login. |
| `--refresh-token` | `JoomleagueImporter:RefreshToken` | Refreshed right away and then kept fresh during the import. Preferred for long imports. |
| `--id-map` | See [Id map files](#id-map-files) | Id map file path. A relative path is relative to the current directory. |
| `--yes`, `-y` | off | No prompts. Uses the configured URL and email (`test@myleague.local` if empty). |
| `--dry-run` | `JoomleagueImporter:DryRun` | Parse, print, and write the expected report. Nothing is sent to the API. |
| `--repair-all` | `JoomleagueImporter:RepairAll` | Re-send events for every match already in the id map. |
| `--repair-matches` | `JoomleagueImporter:RepairMatches` | Comma-separated JoomLeague match ids to repair. Added to the config list. |
| `--concurrency` | `4` | Matches imported in parallel inside one season. |
| `--season-concurrency` | `2` | Seasons imported in parallel. |
| `--person-concurrency` | `8` | Parallel person and player creates. |
| `--club-concurrency` | `8` | Parallel club creates. |
| `--team-concurrency` | `8` | Parallel team creates. Roster adds for one team stay sequential. |

## Configuration

`appsettings.json` in the tool folder is copied to the build output. Environment variables override it. Use `__` for nesting, for example `JoomleagueImporter__ApiBaseUrl` or `JoomleagueImporter__RefreshToken`.

| Key (`JoomleagueImporter:` prefix) | Default when missing | Meaning |
|------------------------------------|----------------------|---------|
| `ApiBaseUrl` | `http://localhost:8080/` | Target API |
| `LoginEmail` | `test@myleague.local` | Email for the Development login. Empty means the default, and the prompt shows it. |
| `DumpFilePath` | none | SQL dump path |
| `Sport` | `floorball` | Default sport |
| `ProjectNameFilter` | `salibandy\|sähly\|sahly\|puuma` | Floorball project name regex (case-insensitive). `puuma` selects PUUMALIIGA, the women's floorball league, which JoomLeague stores as hockey. |
| `ProjectNameExcludeFilter` | none | Floorball exclude regex. The shipped config uses `manager`. |
| `Football:ProjectNameFilter` | `jalkapallo\|football\|futis` | Football include regex |
| `Football:ProjectNameExcludeFilter` | `manager` | Football exclude regex |
| `Hockey:ProjectNameFilter` | `jääkiekko\|jääkiekon\|jaakiekko\|jaakiekon\|hockey` | Hockey include regex. `jääkiekon` selects the JÄÄKIEKON PMT projects. |
| `Hockey:ProjectNameExcludeFilter` | `manager\|jääpallo\|jaapallo\|kaukalo\|nhl` | Hockey exclude regex |
| `ProjectIdFilter` | none | Comma-separated project ids |
| `DryRun` | `false` | Same as `--dry-run` |
| `FillUnknownGoals` | `true` | If the recorded score has more goals than the dump has goal events, add the missing goals to a per-team "Tuntematon" (unknown) player so final scores match |
| `RepairMatches`, `RepairAll` | none, `false` | Same as the CLI flags |
| `IdMapPath`, `Football:IdMapPath`, `Hockey:IdMapPath` | none | Id map path per sport |
| `AccessToken`, `RefreshToken` | none | Set these only through environment variables, never in the committed file |

Environment-variable-only setting:

| Variable | Meaning |
|----------|---------|
| `JoomleagueImporter__TokenCachePath` | If set, the tool writes the latest access and refresh tokens to this file after every refresh. The next run (for example the next sport) can then reuse a session whose refresh token was rotated. The file holds secrets. Keep it outside the repository. |

Do not commit a personal `DumpFilePath` or any token to `appsettings.json`. Pass `--dump` or set environment variables instead.

## Input and output

**Input.** The dump is parsed for these tables: `jos_joomleague_club`, `team`, `person`, `project`, `season`, `project_team`, `team_player`, `round`, `match`, `match_event`, `match_player`, `playground`, `project_position`, `position`, and `eventtype`. Each table name has the `jos_joomleague_` prefix. A project becomes one MyLeague season.

**What gets imported.**

- Clubs, persons, sport players, teams, and rosters.
- Seasons with intro cards. The cards come from project and season text (`description`, `projectinfo`, `extended`, `extension`). A season without text gets one "History data" card.
- Matches with all events in one request: `POST api/floorball-matches/{id}/events/import`, `api/football-matches/{id}/events/import`, or `api/HockeyMatch/{id}/events/import`.
- Games-played stats only for players with a `jos_joomleague_match_player` row or an event in that match.
- After each season, its roster rows are set inactive (hockey: `RosterStatus=Inactive`) and the season is marked completed. Hockey standings are recalculated first.
- After all seasons, each person's latest membership (by project start date) is made active again. That becomes the person's current club.

**Output files.**

| File | Location | Content |
|------|----------|---------|
| `expected-{sport}.json` | `src/tools/JoomleagueImporter/reports/` | Standings and scoring computed from the dump (3/1/0 points). Written on every run, including `--dry-run`. |
| `compare-{sport}.json` | same | Difference between the expected report and what the API returns after the import |
| `import_errors_{timestamp}_{pid}.log` | `Logs/` under the build output folder | Errors and warnings from the import |
| Id map | see below | Mapping from old ids to new ids |

`reports/` and `Logs/` are gitignored. The reports folder is found as three levels up from the build output folder. That is the tool folder when you use `dotnet run` with the default `bin/<Configuration>/net10.0` output.

## Id map files

The id map is a JSON file that maps JoomLeague ids to MyLeague GUIDs:

| Section | Key → value |
|---------|-------------|
| `Persons` | old person id → `{ PersonId, PlayerId }` |
| `Clubs`, `Teams`, `Seasons` | old id → new GUID |
| `ProcessedMatches` | old match id → new match GUID, for matches whose import finished |
| `UnknownPlayers`, `ExtraUnknownPlayers` | old team id → "Tuntematon" placeholder players |

With the id map, a re-run skips work that is already done. Match writes are flushed every 10 changes, so an interrupted run loses at most a few matches. Those are imported again on the next run.

Which file is used:

1. `--id-map`, if given.
2. `IdMapPath` (floorball), `Football:IdMapPath`, or `Hockey:IdMapPath`.
3. Otherwise a file in the build output folder (`bin/...`):
   - Local API (`localhost` or `127.0.0.1`): `id-map.json`, `id-map-football.json`, or `id-map-hockey.json`.
   - Remote API: `id-map-{host}-{sport}.json`, where dots in the host become dashes. A local map is therefore never reused against a remote server.

Rules:

- **One id map per target environment and sport.** A map from one database points to GUIDs that do not exist in another.
- **Keep the map.** Matches are skipped only through `ProcessedMatches`. If you lose the map, a re-run against the same database creates duplicate matches. The default location under `bin/` is lost on `dotnet clean` or a fresh clone, so for any import you care about, pass `--id-map` with a path you control.
- **Never commit id maps for production or other shared environments.** They expose internal GUIDs and record which data was imported where. `.gitignore` has no wildcard for id maps. It lists only some exact names: the three local defaults, the `id-map-local-*` files, the `id-map-smoke-*` files, and two of the `id-map-staging-smoke-*` files. The default maps under `bin/` are ignored with the rest of the build output. Any other name you pass with `--id-map` to a path inside the repository shows up in `git status`. Keep production maps outside the repository, or check `git status` before every commit.

## Caveats

- **Writes to the target.** A run creates real clubs, persons, teams, seasons, and matches. Do a `--dry-run` first, and import into a local or staging API before production.
- **Name matching.** Clubs, teams, and seasons with the same name as an existing record are reused. Persons are reused on first name plus last name, without birth date, so two different people with the same name become one person. If an old project name matches a season that another project already created, the new season is named `<name> [JL<projectId>]`.
- **Referee.** Matches get the first existing referee (hockey: official) in the target. If there is none, an import referee is created.
- **Re-runs skip completed seasons.** The API rejects every change to a completed competition, so a season that is already in the id map and completed in the target is left alone: no team, roster, match, or category updates. Matches still missing from such a season cannot be added. A category mismatch is only logged.
- **Repairs.** `--repair-all` and `--repair-matches` re-send events for matches already in the id map. They need the same id map as the original run.
- **Retries.** Event imports are retried up to twice on unique-constraint races. Live per-event calls are retried up to five times.
- **Exit code.** It is `1` when the dump is missing or the run fails fatally. Otherwise it is `0`, even if some matches failed. Check the summary line, the log file, and `compare-{sport}.json`.
- **Hockey.** Importing hockey data does not turn on a public hockey UI.
- **Sport and category come from the project name.** The importer ignores JoomLeague's `sports_type_id`, which is wrong for some projects (PUUMALIIGA is floorball but stored as hockey). A project or team name with `naiset`, `naisten`, `ladies`, `women`, or `puuma…` gets the Women category. PUUMALIIGA seasons go to the `Salibandy Puumaliiga` division.

## Tests

Unit tests are in `tests/tools/JoomleagueImporter.UnitTests`. They cover jersey number claims, expected-stats calculation, series resolution, JWT expiry parsing, match appearance selection, and latest-team resolution.

```bash
dotnet test tests/tools/JoomleagueImporter.UnitTests/JoomleagueImporter.UnitTests.csproj
```

## Related

- [Seeder](../Seeder/README.md): development test data
- [Root README](../../../README.md)

# JoomleagueImporter

Imports **floorball**, **football**, or **hockey** data from a JoomLeague MySQL dump (`.sql`) into a running WebAPI. It parses clubs, teams, persons, projects (seasons), matches, and match events, then posts them in order. Season intro cards come from project/season text columns (`description`, `projectinfo`, `extended`, `extension`) when the dump has them; otherwise the importer stores a single “History data” card. An on-disk id map makes re-runs idempotent.

Each sport has its own importer pair: `FloorballEntityImporter` / `FloorballMatchImporter`, `Football*`, `Hockey*`. HTTP calls go through `ImportApiClient` (auth, clubs, persons) plus `FloorballApiClient`, `FootballApiClient`, or `HockeyApiClient`.

For day-to-day empty-database setup, prefer the [Seeder](../Seeder/README.md). This tool is for historical dumps.

## Prerequisites

- .NET 10 SDK
- WebAPI reachable (local Docker/Kestrel, or Azure)
- A JoomLeague SQL dump path in `appsettings.json` or `--dump`
- **Local:** Development auto-fill login (`LoginCode:AutoFillLoginCode = true`)
- **Remote (Azure):** a SystemAdmin access token and preferably a refresh token — never enable auto-fill login on a public environment

## Configure

`appsettings.json` (trim to what you need):

```json
{
  "JoomleagueImporter": {
    "ApiBaseUrl": "http://localhost:8080/",
    "LoginEmail": "test@myleague.local",
    "DumpFilePath": "C:\\path\\to\\dump.sql",
    "Sport": "floorball",
    "ProjectNameFilter": "salibandy|sähly|sahly",
    "ProjectNameExcludeFilter": "manager",
    "DryRun": false,
    "FillUnknownGoals": true,
    "Football": {
      "ProjectNameFilter": "jalkapallo|football|futis",
      "ProjectNameExcludeFilter": "manager"
    },
    "Hockey": {
      "ProjectNameFilter": "jääkiekko|jaakiekko|hockey",
      "ProjectNameExcludeFilter": "manager|jääpallo|jaapallo|kaukalo|nhl"
    }
  }
}
```

`LoginEmail` can be empty; the tool will prompt. Filters are regexes over JoomLeague project names.

## Run

```bash
# Floorball against local API (prompts for URL + confirmation, uses Dev auto-fill login)
dotnet run --project src/tools/JoomleagueImporter/JoomleagueImporter.csproj

# Football / hockey
dotnet run --project src/tools/JoomleagueImporter/JoomleagueImporter.csproj -- --sport=football
dotnet run --project src/tools/JoomleagueImporter/JoomleagueImporter.csproj -- --sport=hockey

# Parse and print the selected set only
dotnet run --project src/tools/JoomleagueImporter/JoomleagueImporter.csproj -- --dry-run

# Remote / Azure — pass API URL + tokens (do not commit tokens)
# Tokens can also be env vars: JoomleagueImporter__AccessToken, JoomleagueImporter__RefreshToken, JoomleagueImporter__ApiBaseUrl
dotnet run --project src/tools/JoomleagueImporter/JoomleagueImporter.csproj -- \
  --sport=floorball \
  --api-url=https://myleague-staging-api.azurewebsites.net/ \
  --access-token="$ACCESS_TOKEN" \
  --refresh-token="$REFRESH_TOKEN" \
  --yes
```

CLI (all optional): `--api-url`, `--access-token` / `--token`, `--refresh-token`, `--dump`, `--id-map`, `--sport`, `--project-id`, `--concurrency` (default 4, matches within a season), `--season-concurrency` (default 2, whole seasons in parallel), `--person-concurrency` (default 8), `--club-concurrency` (default 8), `--team-concurrency` (default 8, team creates; roster adds on one team stay sequential), `--yes` / `-y`, `--dry-run`, `--repair-all`, `--repair-matches=1119,1124`.

Finished matches post **one** `POST .../events/import` with all goals/penalties (or cards) after create / goalies / start. Hockey uses `POST /api/HockeyMatch/{id}/events/import`. The live per-event endpoints stay for the scorekeeper UI.

Resume skips persons/teams already in the id map. Clubs, persons, and teams import in parallel (`--club-concurrency`, `--person-concurrency`, `--team-concurrency`); seasons use `--season-concurrency`; matches inside a season use `--concurrency`. Roster adds on one team stay sequential. The id map is flushed every 10 match writes. Event-import unique-constraint races are retried twice.

Without a token the importer prompts for URL and email and uses the Development auto-fill login. With a token it skips that and refreshes the JWT for the length of the import.

Remote hosts get their own id-map file (`id-map-{host}-{sport}.json`) so a local import map is not reused against Azure.

`--repair-all` (or `RepairMatches` / `RepairAll` in config) re-sends match events for already imported matches. `--repair-matches=1119,1124` repairs only those JoomLeague match ids.

Logs go under the tool’s `Logs` folder. The id map path is derived from `--id-map`, config (`IdMapPath` / football / hockey override), or the host/sport default above.

## Related

- [MahlImporter](../MahlImporter/README.md) — scrape mahl.fi instead of a dump
- [DataImporter](../DataImporter/README.md) — `.jlg` persons only
- [Root README](../../../README.md)

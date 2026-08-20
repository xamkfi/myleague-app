# JoomleagueImporter

Imports **floorball** or **football** data from a JoomLeague MySQL dump (`.sql`) into a running WebAPI. It parses clubs, teams, persons, projects (seasons), matches, and match events, then posts them in order. An on-disk id map makes re-runs idempotent.

For day-to-day empty-database setup, prefer the [Seeder](../Seeder/README.md). This tool is for historical dumps.

## Prerequisites

- .NET 9 SDK
- WebAPI running (Development login flow)
- A JoomLeague SQL dump path in `appsettings.json`

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
    }
  }
}
```

`LoginEmail` can be empty; the tool will prompt. Filters are regexes over JoomLeague project names.

## Run

```bash
# Floorball (default, or JoomleagueImporter:Sport)
dotnet run --project src/tools/JoomleagueImporter/JoomleagueImporter.csproj

# Football
dotnet run --project src/tools/JoomleagueImporter/JoomleagueImporter.csproj -- --sport=football

# Parse and print the selected set only
dotnet run --project src/tools/JoomleagueImporter/JoomleagueImporter.csproj -- --dry-run
```

The importer prompts for the API URL and confirmation before writing. `--repair-all` (or `RepairMatches` / `RepairAll` in config) re-sends match events for already imported matches.

Logs go under the tool’s `Logs` folder. The id map path is derived from config (`IdMapPath` / football override) so you can resume a large dump.

## Related

- [MahlImporter](../MahlImporter/README.md) — scrape mahl.fi instead of a dump
- [DataImporter](../DataImporter/README.md) — `.jlg` persons only
- [Root README](../../../README.md)

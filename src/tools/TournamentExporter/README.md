# TournamentExporter

Pulls live **floorball** tournaments from a source API and writes admin-import JSON (`myleague-tournament-import/v1`). Optional second step posts those files into another API (local or staging).

The same format is used by **Admin → Floorball → Tournaments → Import from JSON**. See `src/frontend/src/types/floorball/tournamentImportTypes.ts` for the schema.

## Prerequisites

- .NET 10 SDK
- Readable source API (public tournament + match endpoints)
- For `--import`: target API with Development (or equivalent) login

## Configure

`appsettings.json`:

```json
{
  "TournamentExporter": {
    "SourceApiUrl": "https://myleague-dev-api.azurewebsites.net/",
    "OutputDirectory": "exports",
    "TournamentIds": [
      "00000000-0000-0000-0000-000000000001"
    ]
  }
}
```

CLI flags override config. If you pass no `--id` and the config list is empty, the tool uses two built-in default tournament ids (update those in `Program.cs` or always pass `--id`).

## Run

```bash
# Export to ./exports
dotnet run --project src/tools/TournamentExporter/TournamentExporter.csproj -- --id <guid>

# Export then import into local Docker API
dotnet run --project src/tools/TournamentExporter/TournamentExporter.csproj -- --id <guid> --import --target http://localhost:8080/ --email test@myleague.local

# Replace existing tournaments with the same name on the target
dotnet run --project src/tools/TournamentExporter/TournamentExporter.csproj -- --id <guid> --replace --target http://localhost:8080/
```

| Flag | Meaning |
|------|---------|
| `--api <url>` | Source API (default: Azure Dev URL in config) |
| `--out <dir>` | Output directory (default `./exports`) |
| `--id <guid>` | Tournament id (repeatable) |
| `--category Adult\|Women\|Youth` | Force category on every file |
| `--import` | Import written JSON into `--target` |
| `--replace` | Delete same-name tournaments before import (implies `--import`) |
| `--target <url>` | Destination API (implies `--import`; default `http://localhost:8080/`) |
| `--email <email>` | Login for the target (default `test@myleague.local`) |

Each file includes teams, rosters, group-stage matches (goals / penalties / saves), and playoff slots when present. Category is inferred from the tournament name (`Naiset` → Women) unless the source already sends one or you pass `--category`.

## Related

- [Seeder](../Seeder/README.md) — synthetic tournaments
- [Root README](../../../README.md)

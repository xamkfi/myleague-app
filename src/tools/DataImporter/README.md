# DataImporter

Imports **persons** from legacy JoomLeague `.jlg` XML files into a running WebAPI. Clubs, teams, and matches are not created here — use the [Seeder](../Seeder/README.md) or [JoomleagueImporter](../JoomleagueImporter/README.md) after persons exist.

Stack setup: [root README](../../../README.md).

## Prerequisites

- .NET 10 SDK
- WebAPI running (`http://localhost:8080` by default)
- One or more `.jlg` files in a `DataFiles` folder (next to the project or the working directory)

## Configure

`appsettings.json`:

```json
{
  "BaseUrl": "http://localhost:8080"
}
```

The tool also prompts for the API URL at startup.

## Run

```bash
dotnet run --project src/tools/DataImporter/DataImporter.csproj
```

It scans `DataFiles` for `.jlg` files, posts persons, and prints created / duplicate / skipped / failed counts. Duplicates (same name already in the API) are skipped.

## Related

- [FloorballPlayerImporter](../FloorballPlayerImporter/README.md) — attach persons to team rosters
- [JoomleagueImporter](../JoomleagueImporter/README.md) — full dump import (persons + teams + matches)

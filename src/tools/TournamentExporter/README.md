# TournamentExporter

Exports floorball tournaments from a MyLeague API to JSON files in the admin import format. It can then import those files into another MyLeague API.

## When to use it

- You want a real tournament (teams, rosters, group matches, events, playoff slots) from a hosted environment in your local or staging database.
- You need a `myleague-tournament-import/v1` file for **Admin → Floorball → Tournaments → Import from JSON**. The schema is in `src/frontend/src/types/floorball/tournamentImportTypes.ts`.

The tool handles floorball only.

## Prerequisites

- .NET 10 SDK.
- **Source API:** reachable over HTTP. The tool reads it without logging in. It uses the public endpoints `GET api/FloorballTournament/{id}`, `api/floorball-matches/by-competitionId/{id}`, `api/floorball-matches/by-id/{id}`, and `api/FloorballTeam/{id}`.
- **Target API** (only with `--import`, `--replace`, or `--target`): it must run in Development with `LoginCode:AutoFillLoginCode = true`, and the login email must be an admin user. Token login is not supported, so you cannot import into a public environment with this tool. For that, use the admin UI import with the exported file.

## Usage

Run from the repository root. The output folder is relative to the current directory, so pass `--out` to keep the files in the gitignored tool folder.

```bash
# Export one tournament
dotnet run --project src/tools/TournamentExporter/TournamentExporter.csproj -- \
  --id <tournament-guid> --out src/tools/TournamentExporter/exports

# Export two tournaments and force the team category
dotnet run --project src/tools/TournamentExporter/TournamentExporter.csproj -- \
  --id <guid-1> --id <guid-2> --category Women --out src/tools/TournamentExporter/exports

# Export, then import into the local API
dotnet run --project src/tools/TournamentExporter/TournamentExporter.csproj -- \
  --id <tournament-guid> --out src/tools/TournamentExporter/exports \
  --target http://localhost:8080/ --email test@myleague.local

# Export and replace same-name tournaments on the local API
dotnet run --project src/tools/TournamentExporter/TournamentExporter.csproj -- \
  --id <tournament-guid> --out src/tools/TournamentExporter/exports --replace

# Help
dotnet run --project src/tools/TournamentExporter/TournamentExporter.csproj -- --help
```

## Options

Options take a separate value (`--id <guid>`). The `--id=<guid>` form is not supported. An unknown argument stops the tool with an error before it does anything.

| Option | Default | Meaning |
|--------|---------|---------|
| `--api <url>` | `TournamentExporter:SourceApiUrl` (the Azure dev API in the shipped config) | Source API |
| `--out <dir>` | `TournamentExporter:OutputDirectory`, else `exports` | Output folder. A relative path is resolved from the current directory. |
| `--id <guid>` | See below | Tournament to export. Repeatable. |
| `--category <value>` | Inferred | `Adult`, `Women`, or `Youth` (case-insensitive), forced on every exported file |
| `--import` | off | After the export, import every `*.json` in the output folder into the target |
| `--replace` | off | Delete a tournament with the same name on the target before importing. Implies `--import`. |
| `--target <url>` | `TournamentExporter:TargetApiUrl`, else `http://localhost:8080/` | Target API. Implies `--import`. |
| `--email <email>` | `TournamentExporter:LoginEmail`, else `test@myleague.local` | Login email for the target |
| `--help`, `-h` | | Print usage and exit |

Which tournaments are exported: the ids in `TournamentExporter:TournamentIds` plus every `--id`. The CLI ids are added to the config list. They do not replace it. If both are empty, two built-in ids in `Program.cs` are used. The shipped `appsettings.json` lists those same two ids, so with the shipped config every run exports those two tournaments as well. Empty the config list if you only want your `--id` values.

## Configuration

`appsettings.json` (optional), overridable with environment variables such as `TournamentExporter__SourceApiUrl`:

| Key | Meaning |
|-----|---------|
| `TournamentExporter:SourceApiUrl` | Source API |
| `TournamentExporter:OutputDirectory` | Output folder |
| `TournamentExporter:TournamentIds` | Array of tournament GUIDs |
| `TournamentExporter:TargetApiUrl` | Target API for import |
| `TournamentExporter:LoginEmail` | Login email for the target |

## Input and output

**Export.** One file per tournament, named after a slug of the tournament name (for example `pmt-2026-naiset.json`). Each file holds:

- Clubs and teams, with rosters (player names, positions, jersey numbers).
- Groups and group-stage matches, with goals, penalties, and saves.
- The playoff schedule, if the tournament has one.

The team category comes from, in order:

1. `--category`.
2. The category from the source API.
3. A guess from the tournament name. `naiset`, `women`, and similar words mean `Women`. `junior`, `nuoret`, `u10` to `u21`, and similar mean `Youth`. Anything else means `Adult`.

**Import** (with `--import`). Per file:

1. Skip the file if it has no match events.
2. Skip the file if a tournament with the same name exists, unless `--replace` is given.
3. Reuse clubs, teams, persons, and players by name, or create them.
4. Create the tournament, groups, and matches.
5. Replay the events through the live match endpoints, so statistics are computed by the API.
6. Use the target's first referee, or create an import referee if there is none.

## Caveats

- **`--import` imports the whole folder.** That includes files left over from earlier runs. Clean the folder or use a fresh `--out` if you want only this run's tournaments.
- **`--replace` deletes data.** It calls `DELETE api/floorballtournament/{id}` on the target tournament with the same name before it imports again.
- **Re-running without `--replace`** skips tournaments that already exist, so it is safe to repeat.
- **Name matching.** Persons are matched on first name plus last name. Two people with the same name become one person on the target.
- **Personal data.** Exported files contain player names. `src/tools/TournamentExporter/exports/` is gitignored. Other output folders, such as `exports/` at the repository root, are not. Do not commit exports.
- **Exit code.** `0` on success. `1` when the export or import fails. An invalid argument ends the process with an unhandled exception before any work starts.

## Related

- [Seeder](../Seeder/README.md): synthetic tournaments for development
- [Root README](../../../README.md)

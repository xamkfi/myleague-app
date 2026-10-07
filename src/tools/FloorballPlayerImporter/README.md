# FloorballPlayerImporter

Adds floorball players to team rosters from simple JSON roster files through the HTTP API.

> **Status: does not work and is superseded.** The tool never logs in. The calls it depends on require the Admin role:
>
> - `GET api/persons/search`
> - `POST api/clubs`
> - `POST api/floorballteam`
> - `POST api/floorballplayer`
> - `POST api/floorballteam/{teamId}/players/{playerId}`
>
> Against the current API it finds no persons and cannot create anything. The project still builds and is still in `MyLeague.sln`.
>
> Use one of these instead:
>
> - The roster import in the admin UI: the season pages under **Admin → Floorball / Football / Hockey → Seasons**. It covers floorball, football, and hockey, and it is scoped to a competition.
> - [TournamentExporter](../TournamentExporter/README.md) to copy tournament rosters between environments.
> - [JoomleagueImporter](../JoomleagueImporter/README.md) for historical JoomLeague rosters.

## When to use it

Do not use it. This README describes what the code does, so that someone can fix it or delete it.

## Prerequisites (if fixed)

- .NET 10 SDK.
- A WebAPI, plus code that logs in as an admin before the import. The tool has no login code today.
- The persons must already exist in the API. The tool does not create persons.

## Usage

```bash
dotnet run --project src/tools/FloorballPlayerImporter/FloorballPlayerImporter.csproj
```

There are no command-line options. The tool asks for the API URL and uses the configured value if you press Enter. It adds `http://` if the scheme is missing.

## Configuration

| Key | Default | Meaning |
|-----|---------|---------|
| `BaseUrl` (root level of `appsettings.json`, or the `BaseUrl` environment variable) | `http://localhost:8080` | API base URL, the default answer to the prompt |

## Input and output

**Input.** Every `*.json` file in the first `DataFiles` folder found, in this order:

1. Next to the build output.
2. In the current directory.
3. Up to four levels up from the build output.
4. Next to `FloorballPlayerImporter.csproj`.

Roster files contain personal data. `.gitignore` lists only three file names in this folder. A new file is not ignored, so do not commit it.

Format, one team per file:

```json
{
  "team": "Team Name",
  "players": [
    { "jerseyNumber": 10, "firstName": "First", "lastName": "Last", "position": "Forward" }
  ]
}
```

`position` values are case-insensitive:

| Value | Position |
|-------|----------|
| `Forward` | Forward |
| `Center` | Center |
| `Defender` | Defender |
| `Goalie`, `Goalkeeper` | Goalkeeper |
| anything else | `None` |

**Behavior per file:**

1. Find the floorball team by name. If it does not exist, create a club with the same name (or reuse one) and a team under it. The new team has no division, home arena `TBD`, colors White and Black, and category Adult.
2. For each player, skip them if the jersey number is already taken on the team. Jersey `0` can repeat.
3. Find the person by exact first and last name. If no person matches, skip the player.
4. Reuse the person's floorball player record, or create one.
5. Add the player to the team with the position and jersey number. No competition is given, so this is not a season-scoped roster entry.

**Output.** A console summary: clubs and teams created, players created and assigned, skips, and failures. The exit code is `1` if anything failed, else `0`.

## Caveats

- No authentication, as described above.
- Matching is on name only. Two persons with the same name cannot be told apart.

## Related

- [JoomleagueImporter](../JoomleagueImporter/README.md)
- [TournamentExporter](../TournamentExporter/README.md)
- [Root README](../../../README.md)

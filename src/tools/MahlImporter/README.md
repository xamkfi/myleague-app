# MahlImporter

Scrapes one floorball season from the MAHL JoomLeague website and imports it into MyLeague through the HTTP API.

> **Status: superseded and partly broken.** Use [JoomleagueImporter](../JoomleagueImporter/README.md) instead. It imports the same JoomLeague data from a SQL dump for floorball, football, and hockey, and it is the tool used for the MAHL imports. MahlImporter still builds and is still in `MyLeague.sln`, but match import no longer works. It starts matches with `PUT api/floorballmatch/start-match/{id}`, and the API removed that route when the floorball match controller was split (commit `050e429e`). Every match is therefore left `Scheduled` with no goals or penalties. The logo update mode does not start matches. It may still be useful.

## When to use it

- Only for the logo update mode (operation 2), if you need to move club and team logos that still point to mahl.fi into MyLeague hosted storage.
- Do not use the full import. Use JoomleagueImporter.

## Prerequisites

- .NET 10 SDK.
- Network access to the MAHL site (`MahlImporter:MahlBaseUrl`).
- A WebAPI running in Development with `LoginCode:AutoFillLoginCode = true`. The tool logs in only through the auto-fill login code, and the login email must belong to an admin user. Token login is not supported. That rules out any public environment, where auto-fill must stay off.

## Usage

Run from the repository root:

```bash
dotnet run --project src/tools/MahlImporter/MahlImporter.csproj
```

The tool has no command-line options. It asks these questions in order:

| Prompt | Answers | Default |
|--------|---------|---------|
| Select API environment | `1` local (`http://localhost:8080/`), `2` a hardcoded Azure dev API, `3` custom URL | `1` |
| Login email | any email | `MahlImporter:LoginEmail`, or `test@myleague.local` if empty |
| Select operation | `1` full import, `2` update logos | `1` |
| Use cached data? | `Y` / `n`. Asked only when `ScrapedData/scraped_season.json` exists. | `Y` |

Operations:

1. **Full import.** Steps:
   1. Scrape the schedule, teams, players, and match reports.
   2. Create or reuse clubs, the "LIIGA" division, persons, players, teams, the season, and season rosters.
   3. Create the matches. See the status note: matches stay `Scheduled`.
2. **Update logos.** For every club and floorball team in the target API whose logo is empty or still points to mahl.fi, upload the image through `POST api/clubs/upload-image` and set it as the logo.

## Configuration

`appsettings.json`, overridable with environment variables such as `MahlImporter__ScheduleUrl`:

| Key | Default | Meaning |
|-----|---------|---------|
| `MahlImporter:MahlBaseUrl` | `http://mahl.fi/` | Site to scrape |
| `MahlImporter:ScheduleUrl` | `index.php?option=com_joomleague&view=teamplan&p=219&Itemid=103` | Season schedule page, relative to `MahlBaseUrl`. `p=` is the JoomLeague project id. |
| `MahlImporter:LoginEmail` | empty (falls back to `test@myleague.local`) | Default answer for the login email prompt |
| `MahlImporter:ApiBaseUrl` | n/a | Present in `appsettings.json`, but the code never reads it. The API URL comes only from the prompt. |

## Input and output

- **Scrape cache:** `ScrapedData/scraped_season.json`. The tool looks for `ScrapedData/` next to the build output, then in the current directory, then in `src/tools/MahlImporter/ScrapedData` under the current directory. If none exists, it creates the first one. The cache holds player names. It is not gitignored, so do not commit it.
- **Error log:** `Logs/` next to the `ScrapedData/` folder in use.
- **Target API:** clubs, a division, persons, floorball players, teams, one season, rosters, and matches.

## Caveats

- **Re-running.** Clubs, teams, the division, the season, and persons are looked up by name before the tool creates them. Matches are not checked, and there is no id map, so a second full import creates duplicate matches.
- **Wrong environment.** Option `2` writes to a shared Azure dev API. Use it only if that is what you mean.
- **One season per run.** To import another season, change `ScheduleUrl`.

## Related

- [JoomleagueImporter](../JoomleagueImporter/README.md): the supported way to import JoomLeague data
- [Root README](../../../README.md)

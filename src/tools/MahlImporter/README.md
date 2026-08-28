# MahlImporter

Scrapes a MAHL (JoomLeague) season from [mahl.fi](http://mahl.fi/) and imports clubs, players, teams, a season, and matches into a running WebAPI. Also has a logo-only update mode.

Prefer the [Seeder](../Seeder/README.md) for synthetic dev data, or [JoomleagueImporter](../JoomleagueImporter/README.md) if you already have a SQL dump.

## Prerequisites

- .NET 9 SDK
- WebAPI running (Development login flow)
- Network access to the MAHL site

## Configure

`appsettings.json`:

```json
{
  "MahlImporter": {
    "ApiBaseUrl": "http://localhost:8080/",
    "MahlBaseUrl": "http://mahl.fi/",
    "ScheduleUrl": "index.php?option=com_joomleague&view=teamplan&p=219&Itemid=103",
    "LoginEmail": "test@myleague.local"
  }
}
```

`ScheduleUrl` is the team-plan query for the season you want. Scraped HTML/JSON is cached under `ScrapedData/`.

## Run

```bash
dotnet run --project src/tools/MahlImporter/MahlImporter.csproj
```

The tool asks for the API URL and an operation:

1. **Full import** — scrape → clubs / division / players / teams / season → matches
2. **Update logos** — scrape and fill missing club/team logos only

A default import referee is created if the dump/site has no officials. Errors are written under `Logs/`.

## Related

- [JoomleagueImporter](../JoomleagueImporter/README.md)
- [Seeder](../Seeder/README.md)
- [Root README](../../../README.md)

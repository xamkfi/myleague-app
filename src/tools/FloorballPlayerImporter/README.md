# FloorballPlayerImporter

Console tool that imports floorball players from JSON roster files. It looks up persons by name, creates `FloorballPlayer` records if needed, and assigns jersey numbers and positions. Stack setup: [root README](../../../README.md).

## Features

- **Club Auto-Creation**: Automatically creates clubs if they don't exist (using team name as club name)
- **Team Auto-Creation**: Automatically creates teams if they don't exist (under the auto-created club)
- **Person Lookup**: Finds existing persons in the system by first and last name
- **Player Creation**: Automatically creates FloorballPlayer entities for persons who don't have one
- **Team Assignment**: Adds players to teams with specified jersey numbers and positions
- **Duplicate Prevention**: Skips players with duplicate jersey numbers (except jersey #0)
- **Comprehensive Reporting**: Detailed statistics and error reporting

## Prerequisites

- .NET 10 SDK
- Backend API running (default: http://localhost:8080)
- Persons must already exist in the system

**Note**: Clubs and teams will be created automatically if they don't exist. The club name will be the same as the team name.

## Configuration

Edit `appsettings.json` to configure the backend API URL:

```json
{
  "BaseUrl": "http://localhost:8080"
}
```

## JSON File Format

Create JSON files in the `DataFiles` folder with the following structure:

```json
{
  "team": "Team Name",
  "players": [
    {
      "jerseyNumber": 10,
      "firstName": "John",
      "lastName": "Doe",
      "position": "Forward"
    }
  ]
}
```

### Supported Positions
- `Forward`
- `Center`
- `Defender`
- `Goalie` or `Goalkeeper`

### Jersey Numbers
- All jersey numbers are supported
- Jersey number `0` can be assigned to multiple players
- Other jersey numbers must be unique per team

## Usage

### Build the project

```bash
cd src/tools/FloorballPlayerImporter
dotnet build
```

### Run the importer

```bash
dotnet run
```

The tool will:
1. Scan the `DataFiles` folder for `*.json` files
2. Process each file in the folder
3. Display detailed progress for each player
4. Show a comprehensive summary at the end

## Output Example

```
==========================================================
Floorball Player Importer
==========================================================
Target API: http://localhost:8080/

Using DataFiles folder: C:\...\DataFiles

Found 1 JSON file(s) to process.

Processing file: poyryn-pantterit-roster.json
  Team: Pöyryn Pantterit
  Players in file: 18
  Team not found, creating new team: Pöyryn Pantterit
  Creating new club: Pöyryn Pantterit
  Created new club: Pöyryn Pantterit (ID: 12345678-1234-1234-1234-123456789012)
  Created new team: Pöyryn Pantterit (ID: 87654321-4321-4321-4321-210987654321)
  Using team (ID: 87654321-4321-4321-4321-210987654321)
  Existing jersey numbers on team: 0
  Processing: Antti Pänkäläinen (#3, Goalie)
    Found person (ID: 11111111-1111-1111-1111-111111111111)
    Created new FloorballPlayer (ID: 22222222-2222-2222-2222-222222222222)
    SUCCESS: Added to team as Goalkeeper with jersey #3
  ...

============================================================
Import Summary
============================================================
Clubs created: 1
Teams created: 1
Total players processed: 18
New FloorballPlayers created: 5
Players assigned to teams: 15
Skipped (person not found): 2
Skipped (duplicate jersey): 1
Failed: 0

------------------------------------------------------------
Successfully Assigned Players (15):
------------------------------------------------------------
  ✓ Antti Pänkäläinen (#3)
  ...
```

## Error Handling

The tool handles several scenarios:

- **Club/Team Auto-Creation**: If a club or team doesn't exist, it will be created automatically
  - Club name will be the same as the team name
  - Default values will be used for optional fields (HomeArena: "TBD", Colors: "White"/"Black", Category: Adult)
- **Person Not Found**: If a person doesn't exist, that player is skipped (logged as warning)
- **Duplicate Jersey**: If a jersey number is already in use (except 0), the player is skipped
- **API Errors**: Any API failures are logged with detailed error messages

## Project Structure

```
FloorballPlayerImporter/
├── DataFiles/                      # JSON input files
│   └── poyryn-pantterit-roster.json
├── Models/
│   └── TeamRosterImport.cs        # JSON data models
├── FloorballPlayerImporter.csproj
├── ImportStatistics.cs            # Statistics tracking
├── PlayerImportService.cs         # Core import logic
├── Program.cs                     # Main entry point
├── appsettings.json              # Configuration
└── README.md
```

## Development

### Add Project Reference to Solution

If you have a solution file, add the project:

```bash
dotnet sln add src/tools/FloorballPlayerImporter/FloorballPlayerImporter.csproj
```

### Dependencies

The tool references:
- `Application` - for DTOs and domain models
- `WebAPI` - for API request/response models
- `Microsoft.Extensions.Configuration` - for configuration management
- `System.Net.Http.Json` - for HTTP client functionality

## Troubleshooting

### "Person not found" warnings
- Check that persons are created in the system first
- Verify the first and last names match exactly (case-insensitive)
- Use the DataImporter tool to import persons if needed

### "Duplicate jersey" warnings
- Check the team's current roster for existing jersey numbers
- Update the JSON file to use different jersey numbers
- Only jersey #0 can be assigned to multiple players

## Related tools

- [Seeder](../Seeder/README.md) — persons, clubs, teams, players, matches
- [DataImporter](../DataImporter/README.md) — persons from `.jlg` XML
- [JoomleagueImporter](../JoomleagueImporter/README.md) — JoomLeague SQL dumps
- [Root README](../../../README.md)


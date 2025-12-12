# Floorball Player Importer

A console tool for importing floorball players from JSON files. The tool searches for existing persons by name, creates FloorballPlayer entities if needed, and assigns them to teams with jersey numbers and positions.

## Features

- **Person Lookup**: Finds existing persons in the system by first and last name
- **Player Creation**: Automatically creates FloorballPlayer entities for persons who don't have one
- **Team Assignment**: Adds players to teams with specified jersey numbers and positions
- **Duplicate Prevention**: Skips players with duplicate jersey numbers (except jersey #0)
- **Comprehensive Reporting**: Detailed statistics and error reporting

## Prerequisites

- .NET 9.0 SDK
- Backend API running (default: http://localhost:8080)
- Persons must already exist in the system
- Teams must already exist in the system

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
  Found team (ID: 12345678-1234-1234-1234-123456789012)
  Existing jersey numbers on team: 0
  Processing: Antti Pänkäläinen (#3, Goalie)
    Found person (ID: 87654321-4321-4321-4321-210987654321)
    Created new FloorballPlayer (ID: 11111111-1111-1111-1111-111111111111)
    SUCCESS: Added to team as Goalkeeper with jersey #3
  ...

============================================================
Import Summary
============================================================
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

- **Team Not Found**: If the specified team doesn't exist, the import for that file is skipped
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

### "Team not found" error
- Ensure the team name in the JSON matches exactly (case-insensitive)
- Verify the team exists in the system via the API

### "Person not found" warnings
- Check that persons are created in the system first
- Verify the first and last names match exactly (case-insensitive)
- Use the DataImporter tool to import persons if needed

### "Duplicate jersey" warnings
- Check the team's current roster for existing jersey numbers
- Update the JSON file to use different jersey numbers
- Only jersey #0 can be assigned to multiple players

## Related Tools

- **DataImporter**: Import persons from .jlg XML files
- **Seeder**: Seed initial data including persons, clubs, teams, and players


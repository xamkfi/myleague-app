## Seeder

Run ordered HTTP seeding against the WebAPI (Persons → Clubs → Divisions → Players → Teams).

### Quick Start

**For deployed Azure environments**, use the automated script:

```powershell
# Automatically seeds your dev environment
.\scripts\seed-data.ps1

# Or specify environment explicitly
.\scripts\seed-data.ps1 -Environment dev
```

The script automatically retrieves the backend URL from Terraform and runs the seeder.

### Manual Configuration

Edit `appsettings.json`:
```json
{
  "Seeder": {
    "BaseUrl": "https://localhost:8080/",
    "Persons": [ ... ],
    "Clubs": [ ... ],
    "Divisions": [ ... ]
  }
}
```

### Environment Variable Override

You can override the BaseUrl using environment variables:

```powershell
# PowerShell
$env:SEEDER_BASEURL = "https://your-backend-url.azurecontainerapps.io/"
dotnet run --configuration Release

# Or inline
$env:SEEDER_BASEURL = "https://your-backend-url/"; dotnet run
```

```bash
# Bash/Linux
export SEEDER_BASEURL="https://your-backend-url.azurecontainerapps.io/"
dotnet run --configuration Release
```

### Manual Run (Local Development)

```powershell
# Make sure your backend is running locally first
dotnet run --project src/tools/Seeder/Seeder.csproj
```

### Data Seeded

The seeder creates the following test data:
- **Persons** - Base person records
- **Clubs** - Sports clubs/organizations
- **Divisions** - Competition divisions
- **Floorball Players** - Player profiles (linked to persons)
- **Floorball Referees** - Referee profiles (linked to persons)
- **Floorball Seasons** - Competition seasons
- **Floorball Teams** - Teams with player rosters

All seeding operations are **idempotent** - running multiple times won't create duplicates.


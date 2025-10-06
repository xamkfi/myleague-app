## Seeder

Run ordered HTTP seeding against the WebAPI (Persons → Clubs → Divisions).

### Configure
Edit `appsettings.json`:
```
{
  "Seeder": {
    "BaseUrl": "https://localhost:65532/",
    "Persons": [ ... ],
    "Clubs": [ ... ],
    "Divisions": [ ... ]
  }
}
```

Or override with environment variable `SEEDER_BASEURL`.

### Run
```
dotnet run --project src/tools/Seeder/Seeder.csproj
```

Ensure the WebAPI is running and the BaseUrl matches your development port.


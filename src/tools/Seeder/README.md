## Seeder

Run ordered HTTP seeding against the WebAPI to create a complete test dataset.

### Data Pipeline

The seeder creates entities in the following order:

1. **Persons** - Base persons, players, goalies, and referees
2. **Clubs** - Sports clubs
3. **Divisions** - League divisions
4. **Floorball Players** - Player registrations from person records
5. **Floorball Referees** - Referee registrations from person records
6. **Seasons** - League seasons with division associations
7. **Teams** - Floorball teams with club and division associations
8. **Team-Season Assignments** - Assign teams to their respective seasons
9. **Player-Team Assignments** - Add players to team rosters
10. **Matches** - Create scheduled matches for each season

### Configure

#### Using Test Data File (Recommended)

The seeder automatically loads comprehensive test data from `data/testdata.json` if present. This file contains:
- 44 persons (40 players + 4 referees)
- 4 clubs (Helsinki, Tampere, Oulu, Turku)
- 2 divisions (Premier Division, Division One)
- 4 teams (10 players each, unique per team)
- 2 seasons (one per division)
- 4 matches (home and away games per division)

#### Using appsettings.json

You can also configure via `appsettings.json`:
```json
{
  "Seeder": {
    "BaseUrl": "http://localhost:8080/",
    "Persons": [ ... ],
    "PlayerPersons": [ ... ],
    "GoaliePersons": [ ... ],
    "RefereePersons": [ ... ],
    "Clubs": [ ... ],
    "Divisions": [ ... ],
    "FloorballSeasons": [ ... ],
    "FloorballTeams": [ ... ],
    "FloorballMatches": [ ... ]
  }
}
```

Override base URL with environment variable `SEEDER_BASEURL`.

### Run

```bash
dotnet run --project src/tools/Seeder/Seeder.csproj
```

Ensure the WebAPI is running and the BaseUrl matches your development port.

### Idempotency

All seeders perform idempotent checks before creating entities:
- Persons: Checked by email or name + birthdate
- Clubs: Checked by name
- Divisions: Checked by name + sport type
- Players/Referees: Checked by person ID
- Seasons: Checked by name + division
- Teams: Checked by name + club + division
- Matches: Checked by season + home team + away team

This means you can safely run the seeder multiple times without creating duplicate data.


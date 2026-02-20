using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using MahlImporter.Models;

namespace MahlImporter.Import;

public class EntityImporter
{
    private readonly ApiClient _api;

    public EntityImporter(ApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// Returns mapping: team name -> ClubDto
    /// </summary>
    public async Task<Dictionary<string, ClubDto>> ImportClubsAsync(List<ScrapedTeam> teams)
    {
        Console.WriteLine("--- Importing Clubs ---");
        List<ClubDto> existing = await _api.GetClubsAsync();
        Dictionary<string, ClubDto> map = new(StringComparer.OrdinalIgnoreCase);

        foreach (ScrapedTeam team in teams)
        {
            ClubDto? club = existing.FirstOrDefault(c => string.Equals(c.Name, team.Name, StringComparison.OrdinalIgnoreCase));
            if (club != null)
            {
                Console.WriteLine($"  Club '{team.Name}' already exists.");
                map[team.Name] = club;

                if (!string.IsNullOrEmpty(team.LogoUrl) && NeedsLogoUpload(club.LogoUrl))
                {
                    string? hostedUrl = await _api.UploadClubImageAsync(team.LogoUrl);
                    if (hostedUrl != null)
                    {
                        bool logoOk = await _api.UpdateClubLogoAsync(club.Id, hostedUrl);
                        Console.WriteLine(logoOk
                            ? $"    Updated missing club logo: {hostedUrl}"
                            : $"    WARN: Failed to set club logo for '{team.Name}'");
                    }
                }

                continue;
            }

            club = await _api.CreateClubAsync(team.Name);
            if (club != null)
            {
                Console.WriteLine($"  Created club '{team.Name}'");
                map[team.Name] = club;
                existing.Add(club);
            }
            else
            {
                existing = await _api.GetClubsAsync();
                club = existing.FirstOrDefault(c => c.Name.Contains(team.Name, StringComparison.OrdinalIgnoreCase) ||
                                                    team.Name.Contains(c.Name, StringComparison.OrdinalIgnoreCase));
                if (club != null)
                {
                    Console.WriteLine($"  Found existing club '{club.Name}' for team '{team.Name}'");
                    map[team.Name] = club;
                }
            }

            if (club != null && !string.IsNullOrEmpty(team.LogoUrl))
            {
                string? hostedUrl = await _api.UploadClubImageAsync(team.LogoUrl);
                if (hostedUrl != null)
                {
                    bool logoOk = await _api.UpdateClubLogoAsync(club.Id, hostedUrl);
                    Console.WriteLine(logoOk
                        ? $"    Set club logo: {hostedUrl}"
                        : $"    WARN: Failed to set club logo for '{team.Name}'");
                }
            }
        }

        return map;
    }

    /// <summary>
    /// Returns the LIIGA division, creating it if necessary.
    /// </summary>
    public async Task<DivisionDto> GetOrCreateLiigaDivisionAsync()
    {
        Console.WriteLine("--- Finding LIIGA Division ---");
        List<DivisionDto> divisions = await _api.GetDivisionsAsync();
        DivisionDto? liiga = divisions.FirstOrDefault(d => d.Name.Contains("LIIGA", StringComparison.OrdinalIgnoreCase));
        if (liiga != null)
        {
            Console.WriteLine($"  Found division: {liiga.Name} ({liiga.Id})");
            return liiga;
        }

        liiga = divisions.FirstOrDefault(d => d.Name.Contains("Liiga", StringComparison.OrdinalIgnoreCase));
        if (liiga != null)
        {
            Console.WriteLine($"  Found division: {liiga.Name} ({liiga.Id})");
            return liiga;
        }

        Console.WriteLine("  LIIGA division not found, creating it...");
        liiga = await _api.CreateDivisionAsync("LIIGA", "MAHL LIIGA division", 1, "Floorball");
        if (liiga == null)
            throw new InvalidOperationException("Failed to create LIIGA division via API.");

        Console.WriteLine($"  Created division: {liiga.Name} ({liiga.Id})");
        return liiga;
    }

    /// <summary>
    /// Creates persons and floorball players for all scraped players.
    /// Returns mapping: "FirstName LastName" -> (PersonId, PlayerId)
    /// </summary>
    public async Task<Dictionary<string, (Guid PersonId, Guid PlayerId)>> ImportPlayersAsync(List<ScrapedTeam> teams)
    {
        Console.WriteLine("--- Importing Persons & Floorball Players ---");
        Dictionary<string, (Guid PersonId, Guid PlayerId)> map = new(StringComparer.OrdinalIgnoreCase);

        List<FloorballPlayerDto> existingPlayers = await _api.GetPlayersAsync();
        Dictionary<Guid, FloorballPlayerDto> playerByPersonId = existingPlayers.ToDictionary(p => p.PersonId, p => p);

        HashSet<string> allPlayerNames = [];
        foreach (ScrapedTeam team in teams)
        {
            foreach (ScrapedPlayer sp in team.Players)
            {
                allPlayerNames.Add($"{sp.FirstName} {sp.LastName}");
            }
        }

        int created = 0;
        int skipped = 0;

        foreach (string fullName in allPlayerNames)
        {
            string[] parts = fullName.Split(' ', 2);
            string firstName = parts[0];
            string lastName = parts.Length > 1 ? parts[1] : "";

            if (string.IsNullOrWhiteSpace(lastName))
            {
                continue;
            }

            if (map.ContainsKey(fullName)) continue;

            List<PersonDto> searchResults = await _api.SearchPersonsAsync(fullName);
            PersonDto? person = searchResults.FirstOrDefault(p =>
                string.Equals(p.FirstName, firstName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(p.LastName, lastName, StringComparison.OrdinalIgnoreCase));

            if (person == null)
            {
                person = await _api.CreatePersonAsync(firstName, lastName);
                if (person == null)
                {
                    Console.WriteLine($"  FAIL: Could not create person '{fullName}'");
                    continue;
                }
                created++;
            }
            else
            {
                skipped++;
            }

            FloorballPlayerDto? player = null;
            if (playerByPersonId.TryGetValue(person.Id, out FloorballPlayerDto? existingPlayer))
            {
                player = existingPlayer;
            }
            else
            {
                player = await _api.CreatePlayerAsync(person.Id);
                if (player == null)
                {
                    Console.WriteLine($"  FAIL: Could not create floorball player for '{fullName}'");
                    continue;
                }
            }

            map[fullName] = (person.Id, player.Id);
        }

        Console.WriteLine($"  Persons: {created} created, {skipped} already existed. Total players mapped: {map.Count}");
        return map;
    }

    /// <summary>
    /// Creates floorball teams and adds players. Returns mapping: team name -> FloorballTeamDto
    /// </summary>
    public async Task<Dictionary<string, FloorballTeamDto>> ImportTeamsAsync(
        List<ScrapedTeam> scrapedTeams,
        Dictionary<string, ClubDto> clubMap,
        DivisionDto division,
        Dictionary<string, (Guid PersonId, Guid PlayerId)> playerMap)
    {
        Console.WriteLine("--- Importing Teams ---");
        List<FloorballTeamDto> existing = await _api.GetTeamsAsync();
        Dictionary<string, FloorballTeamDto> map = new(StringComparer.OrdinalIgnoreCase);

        foreach (ScrapedTeam st in scrapedTeams)
        {
            FloorballTeamDto? team = existing.FirstOrDefault(t =>
                string.Equals(t.Name, st.Name, StringComparison.OrdinalIgnoreCase));

            if (team != null)
            {
                Console.WriteLine($"  Team '{st.Name}' already exists.");
                map[st.Name] = team;
            }
            else
            {
                if (!clubMap.TryGetValue(st.Name, out ClubDto? club))
                {
                    Console.WriteLine($"  WARN: No club found for team '{st.Name}', skipping.");
                    continue;
                }

                team = await _api.CreateTeamAsync(st.Name, club.Id, division.Id);
                if (team == null)
                {
                    Console.WriteLine($"  FAIL: Could not create team '{st.Name}'");
                    continue;
                }
                Console.WriteLine($"  Created team '{st.Name}'");
                existing.Add(team);
                map[st.Name] = team;
            }

            int added = 0;
            foreach (ScrapedPlayer sp in st.Players)
            {
                string fullName = $"{sp.FirstName} {sp.LastName}";
                if (!playerMap.TryGetValue(fullName, out (Guid PersonId, Guid PlayerId) ids))
                {
                    continue;
                }

                int position = sp.IsGoalkeeper ? 4 : 1; // 4=Goalkeeper, 1=Forward
                bool ok = await _api.AddPlayerToTeamAsync(team.Id, ids.PlayerId, position, sp.JerseyNumber > 0 ? sp.JerseyNumber : null);
                if (ok) added++;
            }
            Console.WriteLine($"    Added {added}/{st.Players.Count} players to '{st.Name}'");
            if (!string.IsNullOrEmpty(st.LogoUrl) && NeedsLogoUpload(team.LogoUrl))
            {
                string? hostedUrl = await _api.UploadClubImageAsync(st.LogoUrl);
                if (hostedUrl != null)
                {
                    bool logoOk = await _api.UpdateTeamLogoAsync(team.Id, hostedUrl);
                    Console.WriteLine(logoOk
                        ? $"    Set team logo: {hostedUrl}"
                        : $"    WARN: Failed to set team logo for '{st.Name}'");
                }
                else
                {
                    Console.WriteLine($"    WARN: Could not upload team logo for '{st.Name}' from '{st.LogoUrl}'");
                }
            }
        }

        return map;
    }

    /// <summary>
    /// Creates (or finds) a referee to use for all matches.
    /// </summary>
    public async Task<Guid> GetOrCreateImportRefereeAsync(Dictionary<string, (Guid PersonId, Guid PlayerId)> playerMap)
    {
        Console.WriteLine("--- Setting up Import Referee ---");
        List<FloorballRefereeDto> existing = await _api.GetRefereesAsync();
        if (existing.Count > 0)
        {
            Console.WriteLine($"  Using existing referee: {existing[0].Id}");
            return existing[0].Id;
        }

        PersonDto? refPerson = await _api.CreatePersonAsync("Import", "Referee");
        if (refPerson == null)
            throw new InvalidOperationException("Failed to create referee person.");

        FloorballRefereeDto? referee = await _api.CreateRefereeAsync(refPerson.Id);
        if (referee == null)
            throw new InvalidOperationException("Failed to create referee.");

        Console.WriteLine($"  Created import referee: {referee.Id}");
        return referee.Id;
    }

    /// <summary>
    /// Creates the season and adds all teams.
    /// </summary>
    public async Task<FloorballSeasonDto> ImportSeasonAsync(
        string seasonName,
        DivisionDto division,
        Dictionary<string, FloorballTeamDto> teamMap,
        List<ScrapedMatch> matches)
    {
        Console.WriteLine("--- Importing Season ---");
        List<FloorballSeasonDto> existing = await _api.GetSeasonsAsync();
        FloorballSeasonDto? season = existing.FirstOrDefault(s =>
            string.Equals(s.Name, seasonName, StringComparison.OrdinalIgnoreCase));

        if (season != null)
        {
            Console.WriteLine($"  Season '{seasonName}' already exists: {season.Id}");
        }
        else
        {
            List<DateTime> matchDates = matches
                .Where(m => m.OriginalDate != default)
                .Select(m => m.OriginalDate)
                .ToList();

            DateTime startDate = matchDates.Count > 0
                ? matchDates.Min().AddMonths(-1)
                : DateTime.UtcNow.AddMonths(-1);
            DateTime endDate = matchDates.Count > 0
                ? matchDates.Max().AddMonths(1)
                : DateTime.UtcNow.AddMonths(6);

            season = await _api.CreateSeasonAsync(seasonName, division.Id, startDate, endDate);
            if (season == null)
                throw new InvalidOperationException($"Failed to create season '{seasonName}'");

            Console.WriteLine($"  Created season '{seasonName}': {season.Id} ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})");
        }

        Console.Write("  Adding teams to season... ");
        int added = 0;
        foreach (FloorballTeamDto team in teamMap.Values)
        {
            bool ok = await _api.AddTeamToSeasonAsync(season.Id, team.Id);
            if (ok) added++;
        }
        Console.WriteLine($"{added} teams added.");

        Console.Write("  Adding teams to LIIGA division... ");
        int divAdded = 0;
        foreach (FloorballTeamDto team in teamMap.Values)
        {
            bool ok = await _api.AddTeamToSeasonDivisionAsync(season.Id, division.Id, team.Id);
            if (ok) divAdded++;
        }
        Console.WriteLine($"{divAdded} teams added to division.");

        Console.Write("  Activating season... ");
        bool activated = await _api.ActivateSeasonAsync(season.Id);
        Console.WriteLine(activated ? "OK" : "FAILED");

        return season;
    }

    /// <summary>
    /// Fetches all existing clubs and teams from the API and re-uploads any logos
    /// that are missing or still pointing to an external source (mahl.fi).
    /// Uses the scraped team data as a fallback source for clubs/teams with no logo at all.
    /// </summary>
    public async Task UpdateLogosAsync(List<ScrapedTeam> scrapedTeams)
    {
        Dictionary<string, string> scrapedLogoByName = scrapedTeams
            .Where(t => !string.IsNullOrEmpty(t.LogoUrl))
            .ToDictionary(t => t.Name, t => t.LogoUrl!, StringComparer.OrdinalIgnoreCase);

        // ── Clubs ────────────────────────────────────────────────────
        Console.WriteLine("--- Updating Club Logos ---");
        List<ClubDto> clubs = await _api.GetClubsAsync();
        int clubsUpdated = 0;
        int clubsSkipped = 0;

        foreach (ClubDto club in clubs)
        {
            string? sourceUrl = NeedsLogoUpload(club.LogoUrl)
                ? (string.IsNullOrEmpty(club.LogoUrl)
                    ? scrapedLogoByName.GetValueOrDefault(club.Name)
                    : club.LogoUrl)
                : null;

            if (sourceUrl == null)
            {
                clubsSkipped++;
                continue;
            }

            Console.Write($"  Club '{club.Name}'... ");
            string? hostedUrl = await _api.UploadClubImageAsync(sourceUrl);
            if (hostedUrl != null)
            {
                bool ok = await _api.UpdateClubLogoAsync(club.Id, hostedUrl);
                Console.WriteLine(ok ? $"OK -> {hostedUrl}" : "WARN: update failed");
                if (ok) clubsUpdated++;
            }
            else
            {
                Console.WriteLine("WARN: upload failed");
            }
        }

        Console.WriteLine($"  Clubs: {clubsUpdated} updated, {clubsSkipped} already hosted.\n");

        // ── Teams ────────────────────────────────────────────────────
        Console.WriteLine("--- Updating Team Logos ---");
        List<FloorballTeamDto> teams = await _api.GetTeamsAsync();
        int teamsUpdated = 0;
        int teamsSkipped = 0;

        foreach (FloorballTeamDto team in teams)
        {
            string? sourceUrl = NeedsLogoUpload(team.LogoUrl)
                ? (string.IsNullOrEmpty(team.LogoUrl)
                    ? scrapedLogoByName.GetValueOrDefault(team.Name)
                    : team.LogoUrl)
                : null;

            if (sourceUrl == null)
            {
                teamsSkipped++;
                continue;
            }

            Console.Write($"  Team '{team.Name}'... ");
            string? hostedUrl = await _api.UploadClubImageAsync(sourceUrl);
            if (hostedUrl != null)
            {
                bool ok = await _api.UpdateTeamLogoAsync(team.Id, hostedUrl);
                Console.WriteLine(ok ? $"OK -> {hostedUrl}" : "WARN: update failed");
                if (ok) teamsUpdated++;
            }
            else
            {
                Console.WriteLine("WARN: upload failed");
            }
        }

        Console.WriteLine($"  Teams: {teamsUpdated} updated, {teamsSkipped} already hosted.");
    }

    /// <summary>
    /// Returns true when the stored logo URL is missing or still points to an external source
    /// (e.g. mahl.fi) that won't be reachable from the hosted environment.
    /// </summary>
    private static bool NeedsLogoUpload(string? currentLogoUrl)
    {
        if (string.IsNullOrEmpty(currentLogoUrl)) return true;
        return currentLogoUrl.Contains("mahl.fi", StringComparison.OrdinalIgnoreCase);
    }
}

using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Domain.Enums.Common;
using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

/// <summary>
/// Imports clubs, persons, floorball players, teams and rosters through the API,
/// deduplicating by name and recording old-id to new-Guid mappings in the id map.
/// </summary>
public class FloorballEntityImporter
{
    private const int PositionForward = 1;
    private const int PositionGoalkeeper = 4;

    private readonly FloorballApiClient _api;
    private readonly IdMapStore _idMap;
    private readonly ImportLogger _log;

    public FloorballEntityImporter(FloorballApiClient api, IdMapStore idMap, ImportLogger log)
    {
        _api = api;
        _idMap = idMap;
        _log = log;
    }

    // ── Division ─────────────────────────────────────────────

    public async Task<DivisionDto> GetOrCreateImportDivisionAsync()
    {
        Console.WriteLine("--- Division ---");
        const string divisionName = "MAHL Salibandy";

        List<DivisionDto> divisions = await _api.GetDivisionsAsync();
        DivisionDto? division = divisions.FirstOrDefault(d =>
            string.Equals(d.Name, divisionName, StringComparison.OrdinalIgnoreCase));
        if (division != null)
        {
            Console.WriteLine($"  Using existing division '{division.Name}' ({division.Id})");
            return division;
        }

        division = await _api.CreateDivisionAsync(divisionName, "JoomLeague-tuonnin salibandysarjat", 1);
        if (division == null)
            throw new InvalidOperationException($"Failed to create division '{divisionName}'.");
        Console.WriteLine($"  Created division '{division.Name}' ({division.Id})");
        return division;
    }

    // ── Clubs ────────────────────────────────────────────────

    public async Task ImportClubsAsync(FloorballImportSet set, JoomleagueDatabase db)
    {
        Console.WriteLine("--- Clubs ---");
        List<ClubDto> existing = await _api.GetClubsAsync();
        int created = 0, reused = 0;

        foreach (OldTeam team in set.UniqueTeams.Values)
        {
            OldClub? oldClub = team.ClubId.HasValue ? db.Clubs.GetValueOrDefault(team.ClubId.Value) : null;
            int oldClubKey = oldClub?.Id ?? -team.Id; // teams without a club get a synthetic key
            if (_idMap.Clubs.ContainsKey(oldClubKey))
                continue;

            string clubName = !string.IsNullOrWhiteSpace(oldClub?.Name) ? oldClub!.Name : team.Name;

            ClubDto? club = existing.FirstOrDefault(c =>
                string.Equals(c.Name, clubName, StringComparison.OrdinalIgnoreCase));
            if (club == null)
            {
                string city = !string.IsNullOrWhiteSpace(oldClub?.Location) ? oldClub!.Location : "Mikkeli";
                club = await _api.CreateClubAsync(clubName, city);
                if (club == null)
                {
                    // "Name already exists" race with a partial club list; refresh and retry.
                    existing = await _api.GetClubsAsync();
                    club = existing.FirstOrDefault(c =>
                        string.Equals(c.Name, clubName, StringComparison.OrdinalIgnoreCase));
                }
                if (club == null)
                {
                    _log.LogError("CreateClub", new { clubName }, "API returned null.");
                    continue;
                }
                if (!existing.Contains(club))
                    existing.Add(club);
                created++;
            }
            else
            {
                reused++;
            }

            _idMap.Clubs[oldClubKey] = club.Id;
        }

        _idMap.Save();
        Console.WriteLine($"  Clubs: {created} created, {reused} already existed.");
    }

    // ── Persons & players ────────────────────────────────────

    public async Task ImportPersonsAndPlayersAsync(FloorballImportSet set)
    {
        Console.WriteLine("--- Persons & Floorball Players ---");

        List<FloorballPlayerDto> existingPlayers = await _api.GetPlayersAsync();
        Dictionary<Guid, FloorballPlayerDto> playerByPersonId = [];
        foreach (FloorballPlayerDto p in existingPlayers)
            playerByPersonId[p.PersonId] = p;

        int created = 0, reused = 0, failed = 0, done = 0;
        int total = set.UniquePersons.Count;

        foreach (OldPerson oldPerson in set.UniquePersons.Values)
        {
            done++;
            if (_idMap.Persons.ContainsKey(oldPerson.Id))
                continue;

            List<PersonDto> searchResults = await _api.SearchPersonsAsync(oldPerson.FullName);
            PersonDto? person = searchResults.FirstOrDefault(p =>
                string.Equals(p.FirstName, oldPerson.FirstName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(p.LastName, oldPerson.LastName, StringComparison.OrdinalIgnoreCase));

            if (person == null)
            {
                person = await _api.CreatePersonAsync(oldPerson.FirstName, oldPerson.LastName, oldPerson.Birthday);
                if (person == null)
                {
                    // Creation can fail with "name already exists" if the search missed the
                    // person (e.g. paging); retry the search before giving up.
                    searchResults = await _api.SearchPersonsAsync(oldPerson.FullName);
                    person = searchResults.FirstOrDefault(p =>
                        string.Equals(p.FirstName, oldPerson.FirstName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(p.LastName, oldPerson.LastName, StringComparison.OrdinalIgnoreCase));
                }
                if (person == null)
                {
                    _log.LogError("CreatePerson", new { oldPerson.Id, oldPerson.FullName }, "API returned null.");
                    failed++;
                    continue;
                }
                created++;
            }
            else
            {
                reused++;
            }

            FloorballPlayerDto? player = playerByPersonId.GetValueOrDefault(person.Id);
            if (player == null)
            {
                player = await _api.CreatePlayerAsync(person.Id);
                if (player == null)
                {
                    _log.LogError("CreatePlayer", new { oldPerson.Id, oldPerson.FullName, NewPersonId = person.Id }, "API returned null.");
                    failed++;
                    continue;
                }
                playerByPersonId[person.Id] = player;
            }

            _idMap.Persons[oldPerson.Id] = new IdMapStore.PersonMapping { PersonId = person.Id, PlayerId = player.Id };

            if (done % 100 == 0)
            {
                _idMap.Save();
                Console.WriteLine($"  ... {done}/{total} persons processed");
            }
        }

        _idMap.Save();
        Console.WriteLine($"  Persons: {created} created, {reused} already existed, {failed} failed (total {total}).");
    }

    // ── Teams & rosters ──────────────────────────────────────

    public async Task ImportTeamsAsync(FloorballImportSet set, JoomleagueDatabase db, DivisionDto division)
    {
        Console.WriteLine("--- Teams & Rosters ---");
        List<FloorballTeamDto> existing = await _api.GetTeamsAsync();
        int created = 0, reused = 0;

        // Union roster per team across all selected projects. Prefer data from newer projects.
        // Also collect the strongest inferred TeamCategory from projects that include the team.
        Dictionary<int, Dictionary<int, RosterEntry>> rosterByTeam = [];
        Dictionary<int, TeamCategory> categoryByTeam = [];
        foreach (ProjectImport pi in set.Projects.OrderBy(p => p.Project.Id))
        {
            TeamCategory projectCategory = TeamCategoryResolver.InferFromName(pi.Project.Name);
            foreach (ProjectTeamImport pti in pi.Teams.Values)
            {
                if (!rosterByTeam.TryGetValue(pti.Team.Id, out Dictionary<int, RosterEntry>? union))
                {
                    union = [];
                    rosterByTeam[pti.Team.Id] = union;
                }
                foreach (RosterEntry re in pti.Roster)
                    union[re.Person.Id] = re; // later projects overwrite: newest jersey/position wins

                TeamCategory fromTeamName = TeamCategoryResolver.InferFromName(pti.Team.Name);
                TeamCategory combined = TeamCategoryResolver.Prefer(projectCategory, fromTeamName);
                if (categoryByTeam.TryGetValue(pti.Team.Id, out TeamCategory existingCat))
                    categoryByTeam[pti.Team.Id] = TeamCategoryResolver.Prefer(existingCat, combined);
                else
                    categoryByTeam[pti.Team.Id] = combined;
            }
        }

        foreach (OldTeam oldTeam in set.UniqueTeams.Values)
        {
            if (!rosterByTeam.TryGetValue(oldTeam.Id, out Dictionary<int, RosterEntry>? unionRoster) ||
                unionRoster.Count == 0)
            {
                Console.WriteLine($"  SKIP {oldTeam.Name}: 0 roster players");
                continue;
            }

            FloorballTeamDto? team = null;
            TeamCategory teamCategory = categoryByTeam.GetValueOrDefault(oldTeam.Id, TeamCategory.Adult);

            if (_idMap.Teams.TryGetValue(oldTeam.Id, out Guid mappedId))
            {
                team = existing.FirstOrDefault(t => t.Id == mappedId);
            }

            if (team == null)
            {
                team = existing.FirstOrDefault(t =>
                    string.Equals(t.Name, oldTeam.Name, StringComparison.OrdinalIgnoreCase));

                if (team == null)
                {
                    int clubKey = oldTeam.ClubId.HasValue && db.Clubs.ContainsKey(oldTeam.ClubId.Value)
                        ? oldTeam.ClubId.Value
                        : -oldTeam.Id;
                    if (!_idMap.Clubs.TryGetValue(clubKey, out Guid clubId))
                    {
                        _log.LogError("CreateTeam", new { oldTeam.Id, oldTeam.Name }, "No club mapping found.");
                        continue;
                    }

                    team = await _api.CreateTeamAsync(
                        oldTeam.Name, MakeShortName(oldTeam), clubId, division.Id, teamCategory);
                    if (team == null)
                    {
                        _log.LogError("CreateTeam", new { oldTeam.Id, oldTeam.Name }, "API returned null.");
                        continue;
                    }
                    existing.Add(team);
                    created++;
                }
                else
                {
                    reused++;
                }

                _idMap.Teams[oldTeam.Id] = team.Id;
                _idMap.Save();
            }

            // Roster
            int added = 0, rosterTotal = 0;
            if (rosterByTeam.TryGetValue(oldTeam.Id, out Dictionary<int, RosterEntry>? roster) && roster.Count > 0)
            {
                rosterTotal = roster.Count;
                foreach (RosterEntry re in roster.Values)
                {
                    if (!_idMap.Persons.TryGetValue(re.Person.Id, out IdMapStore.PersonMapping? mapping))
                        continue;

                    int position = re.IsGoalkeeper ? PositionGoalkeeper : PositionForward;
                    int? jersey = re.TeamPlayer.JerseyNumber is > 0 and < 100 ? re.TeamPlayer.JerseyNumber : null;
                    bool ok = await _api.AddPlayerToTeamAsync(team.Id, mapping.PlayerId, position, jersey);
                    if (ok) added++;
                }
            }

            Console.WriteLine($"  {oldTeam.Name}: roster {added}/{rosterTotal}");
        }

        _idMap.Save();
        Console.WriteLine($"  Teams: {created} created, {reused} already existed.");
    }

    private static string MakeShortName(OldTeam team)
    {
        string source = !string.IsNullOrWhiteSpace(team.ShortName) ? team.ShortName : team.Name;
        string cleaned = new(source.Where(char.IsLetterOrDigit).ToArray());
        if (cleaned.Length == 0) cleaned = "TEAM";
        return cleaned.Length <= 4 ? cleaned.ToUpperInvariant() : cleaned[..4].ToUpperInvariant();
    }

    // ── Referee ──────────────────────────────────────────────

    public async Task<Guid> GetOrCreateImportRefereeAsync()
    {
        Console.WriteLine("--- Import Referee ---");
        List<FloorballRefereeDto> existing = await _api.GetRefereesAsync();
        if (existing.Count > 0)
        {
            Console.WriteLine($"  Using existing referee: {existing[0].Id}");
            return existing[0].Id;
        }

        List<PersonDto> found = await _api.SearchPersonsAsync("Import Referee");
        PersonDto? refPerson = found.FirstOrDefault(p =>
            string.Equals(p.FirstName, "Import", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(p.LastName, "Referee", StringComparison.OrdinalIgnoreCase));
        refPerson ??= await _api.CreatePersonAsync("Import", "Referee");
        if (refPerson == null)
            throw new InvalidOperationException("Failed to create referee person.");

        FloorballRefereeDto? referee = await _api.CreateRefereeAsync(refPerson.Id);
        if (referee == null)
            throw new InvalidOperationException("Failed to create referee.");

        Console.WriteLine($"  Created import referee: {referee.Id}");
        return referee.Id;
    }

    // ── Unknown scorer players ───────────────────────────────

    /// <summary>
    /// Gets (creating lazily) a per-team "Tuntematon" player used to attribute goals whose
    /// scorer is not present in the old data, so that final scores stay correct.
    /// </summary>
    public async Task<Guid?> GetOrCreateUnknownPlayerAsync(OldTeam oldTeam, Guid newTeamId)
    {
        if (_idMap.UnknownPlayers.TryGetValue(oldTeam.Id, out Guid cached))
            return cached;

        string firstName = "Tuntematon";
        string lastName = $"({oldTeam.Name})";

        List<PersonDto> found = await _api.SearchPersonsAsync($"{firstName} {lastName}");
        PersonDto? person = found.FirstOrDefault(p =>
            string.Equals(p.FirstName, firstName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(p.LastName, lastName, StringComparison.OrdinalIgnoreCase));
        person ??= await _api.CreatePersonAsync(firstName, lastName);
        if (person == null)
        {
            found = await _api.SearchPersonsAsync($"{firstName} {lastName}");
            person = found.FirstOrDefault(p =>
                string.Equals(p.FirstName, firstName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(p.LastName, lastName, StringComparison.OrdinalIgnoreCase));
        }
        if (person == null)
        {
            _log.LogError("CreateUnknownPlayer", new { oldTeam.Id, oldTeam.Name }, "Person creation failed.");
            return null;
        }

        FloorballPlayerDto? player = await _api.CreatePlayerAsync(person.Id);
        if (player == null)
        {
            // The player may already exist for the person; try to find it.
            List<FloorballPlayerDto> players = await _api.GetPlayersAsync();
            player = players.FirstOrDefault(p => p.PersonId == person.Id);
        }
        if (player == null)
        {
            _log.LogError("CreateUnknownPlayer", new { oldTeam.Id, oldTeam.Name }, "Player creation failed.");
            return null;
        }

        bool added = await _api.AddPlayerToTeamAsync(newTeamId, player.Id, PositionForward, null);
        if (!added)
        {
            _log.LogError("CreateUnknownPlayer", new { oldTeam.Id, oldTeam.Name }, "Adding player to team roster failed.");
            return null;
        }

        _idMap.UnknownPlayers[oldTeam.Id] = player.Id;
        _idMap.Save();
        return player.Id;
    }

    // ── Season ───────────────────────────────────────────────

    public async Task<FloorballSeasonDto?> ImportSeasonAsync(ProjectImport pi, DivisionDto division)
    {
        OldProject project = pi.Project;
        TeamCategory teamCategory = TeamCategoryResolver.InferFromName(project.Name);

        List<FloorballSeasonDto> existing = await _api.GetSeasonsAsync();

        if (_idMap.Seasons.TryGetValue(project.Id, out Guid mappedSeasonId))
        {
            FloorballSeasonDto? mapped = existing.FirstOrDefault(s => s.Id == mappedSeasonId);
            if (mapped != null)
            {
                mapped = await EnsureSeasonCategoryAsync(mapped, teamCategory);
                Console.WriteLine(
                    $"  Season already imported: '{mapped.Name}' ({mapped.Id}) [{mapped.TeamCategory}]");
                // Team membership is idempotent on the API side, so always (re-)ensure it;
                // an earlier run may have failed to add some teams.
                await EnsureTeamsInSeasonAsync(pi, mapped, division);
                return mapped;
            }
        }

        string seasonName = project.Name;
        FloorballSeasonDto? byName = existing.FirstOrDefault(s =>
            string.Equals(s.Name, seasonName, StringComparison.OrdinalIgnoreCase));
        if (byName != null && _idMap.Seasons.Values.Contains(byName.Id))
        {
            // Name collision with another old project; disambiguate.
            seasonName = $"{project.Name} [JL{project.Id}]";
            byName = existing.FirstOrDefault(s =>
                string.Equals(s.Name, seasonName, StringComparison.OrdinalIgnoreCase));
        }

        FloorballSeasonDto? season = byName;
        if (season == null)
        {
            List<DateTime> matchDates = pi.Matches
                .Where(m => m.Match.MatchDate.HasValue)
                .Select(m => m.Match.MatchDate!.Value)
                .ToList();

            DateTime start = matchDates.Count > 0 ? matchDates.Min().AddMonths(-1)
                : project.StartDate ?? new DateTime(2000, 1, 1);
            DateTime end = matchDates.Count > 0 ? matchDates.Max().AddMonths(1) : start.AddYears(1);
            if (end <= start) end = start.AddMonths(6);

            season = await _api.CreateSeasonAsync(
                seasonName, division.Id, start, end,
                project.NumberOfPeriods, project.PeriodDurationMinutes,
                teamCategory);
            if (season == null)
            {
                _log.LogError("CreateSeason", new { project.Id, seasonName, teamCategory }, "API returned null.");
                return null;
            }
            Console.WriteLine($"  Created season '{seasonName}' ({season.Id}) [{teamCategory}]");
        }
        else
        {
            season = await EnsureSeasonCategoryAsync(season, teamCategory);
            Console.WriteLine($"  Using existing season '{seasonName}' ({season.Id}) [{season.TeamCategory}]");
        }

        _idMap.Seasons[project.Id] = season.Id;
        _idMap.Save();

        await EnsureTeamsInSeasonAsync(pi, season, division);

        return season;
    }

    /// <summary>
    /// Re-runs can fix seasons that were imported before TeamCategory existed (default Adult).
    /// </summary>
    private async Task<FloorballSeasonDto> EnsureSeasonCategoryAsync(
        FloorballSeasonDto season,
        TeamCategory expected)
    {
        if (season.TeamCategory == expected)
            return season;

        FloorballSeasonDto? updated = await _api.UpdateSeasonAsync(season, expected);
        if (updated == null)
        {
            _log.LogError(
                "UpdateSeasonCategory",
                new { season.Id, season.Name, From = season.TeamCategory, To = expected },
                "API returned null; keeping previous category.");
            return season;
        }

        Console.WriteLine($"  Updated season category '{season.Name}': {season.TeamCategory} → {expected}");
        return updated;
    }

    private async Task EnsureTeamsInSeasonAsync(ProjectImport pi, FloorballSeasonDto season, DivisionDto division)
    {
        int teamsAdded = 0;
        HashSet<Guid> handled = [];
        foreach (ProjectTeamImport pti in pi.Teams.Values)
        {
            if (!_idMap.Teams.TryGetValue(pti.Team.Id, out Guid teamId))
                continue;
            if (!handled.Add(teamId))
                continue; // two old project teams can map to the same new team
            bool ok = await _api.AddTeamToSeasonAsync(season.Id, teamId);
            await _api.AddTeamToSeasonDivisionAsync(season.Id, division.Id, teamId);
            if (ok) teamsAdded++;
        }
        Console.WriteLine($"  Teams in season: {teamsAdded}/{pi.Teams.Count}");
    }
}

using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Teams.DTOs;
using Domain.Enums.Common;
using Domain.Enums.Football;
using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

/// <summary>
/// Imports clubs, persons, football players, teams and rosters through the API,
/// deduplicating by name and recording old-id to new-Guid mappings in the id map.
/// Hobby football defaults: 5v5, two halves, officials not required to start.
/// </summary>
public class FootballEntityImporter
{
    public const int HobbyPlayersOnField = 5;
    public const int HobbyNumberOfHalves = 2;
    public const int HobbyHalfDurationMinutes = 25;

    private readonly FootballApiClient _api;
    private readonly IdMapStore _idMap;
    private readonly ImportLogger _log;

    public FootballEntityImporter(FootballApiClient api, IdMapStore idMap, ImportLogger log)
    {
        _api = api;
        _idMap = idMap;
        _log = log;
    }

    public async Task<DivisionDto> GetOrCreateImportDivisionAsync()
    {
        Console.WriteLine("--- Division ---");
        const string divisionName = "MAHL Jalkapallo";

        List<DivisionDto> divisions = await _api.GetDivisionsAsync();
        DivisionDto? division = divisions.FirstOrDefault(d =>
            string.Equals(d.Name, divisionName, StringComparison.OrdinalIgnoreCase));
        if (division != null)
        {
            Console.WriteLine($"  Using existing division '{division.Name}' ({division.Id})");
            return division;
        }

        division = await _api.CreateDivisionAsync(
            divisionName, "JoomLeague-tuonnin jalkapallosarjat", 1, "Football");
        if (division == null)
            throw new InvalidOperationException($"Failed to create division '{divisionName}'.");
        Console.WriteLine($"  Created division '{division.Name}' ({division.Id})");
        return division;
    }

    public async Task ImportClubsAsync(FloorballImportSet set, JoomleagueDatabase db)
    {
        Console.WriteLine("--- Clubs ---");
        List<ClubDto> existing = await _api.GetClubsAsync();
        int created = 0, reused = 0;

        foreach (OldTeam team in set.UniqueTeams.Values)
        {
            OldClub? oldClub = team.ClubId.HasValue ? db.Clubs.GetValueOrDefault(team.ClubId.Value) : null;
            int oldClubKey = oldClub?.Id ?? -team.Id;
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

    public async Task ImportPersonsAndPlayersAsync(FloorballImportSet set)
    {
        Console.WriteLine("--- Persons & Football Players ---");

        if (set.UniquePersons.Keys.All(_idMap.HasPerson))
        {
            Console.WriteLine($"  Persons: 0 created, {set.UniquePersons.Count} already mapped.");
            return;
        }

        List<FootballPlayerDto> existingPlayers = await _api.GetPlayersAsync();
        Dictionary<Guid, FootballPlayerDto> playerByPersonId = [];
        foreach (FootballPlayerDto p in existingPlayers)
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

            FootballPlayerDto? player = playerByPersonId.GetValueOrDefault(person.Id);
            if (player == null)
            {
                player = await _api.CreatePlayerAsync(person.Id);
                if (player == null)
                {
                    _log.LogError("CreateFootballPlayer", new { oldPerson.Id, oldPerson.FullName, NewPersonId = person.Id }, "API returned null.");
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

    public async Task ImportTeamsAsync(FloorballImportSet set, JoomleagueDatabase db, DivisionDto division)
    {
        Console.WriteLine("--- Teams & Rosters ---");
        int mappedTeams = set.UniqueTeams.Keys.Count(_idMap.HasTeam);
        if (mappedTeams == set.UniqueTeams.Count)
        {
            Console.WriteLine($"  Teams: 0 created, {mappedTeams} already mapped (roster check skipped).");
            return;
        }

        List<Application.Features.Football.Teams.DTOs.FootballTeamSummaryDto> existing = await _api.GetTeamsAsync();
        int created = 0, reused = 0;

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
                    union[re.Person.Id] = re;

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

            Application.Features.Football.Teams.DTOs.FootballTeamSummaryDto? team = null;
            TeamCategory teamCategory = categoryByTeam.GetValueOrDefault(oldTeam.Id, TeamCategory.Adult);

            if (_idMap.HasTeam(oldTeam.Id))
            {
                reused++;
                continue;
            }

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
                        _log.LogError("CreateFootballTeam", new { oldTeam.Id, oldTeam.Name }, "No club mapping found.");
                        continue;
                    }

                    FootballTeamDto? createdTeam = await _api.CreateTeamAsync(
                        oldTeam.Name, MakeShortName(oldTeam), clubId, division.Id, teamCategory);
                    if (createdTeam == null)
                    {
                        _log.LogError("CreateFootballTeam", new { oldTeam.Id, oldTeam.Name }, "API returned null.");
                        continue;
                    }
                    team = new Application.Features.Football.Teams.DTOs.FootballTeamSummaryDto(
                        createdTeam.Id,
                        createdTeam.Name,
                        createdTeam.DivisionId,
                        createdTeam.Club,
                        createdTeam.HomeArena,
                        createdTeam.PrimaryJerseyColor,
                        createdTeam.SecondaryJerseyColor,
                        createdTeam.LogoUrl,
                        createdTeam.HasActiveMembers,
                        createdTeam.TeamCategory);
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

            int added = 0, rosterTotal = unionRoster.Count;
            foreach (RosterEntry re in unionRoster.Values)
            {
                if (!_idMap.Persons.TryGetValue(re.Person.Id, out IdMapStore.PersonMapping? mapping))
                    continue;

                int position = (int)re.FootballPosition;
                if (position == (int)FootballPosition.None)
                    position = (int)FootballPosition.Forward;
                int? jersey = re.TeamPlayer.JerseyNumber is > 0 and < 100 ? re.TeamPlayer.JerseyNumber : null;
                bool ok = await _api.AddPlayerToTeamAsync(team.Id, mapping.PlayerId, position, jersey);
                if (ok) added++;
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

    public async Task<Guid> GetOrCreateImportRefereeAsync()
    {
        Console.WriteLine("--- Import Referee ---");
        List<FootballRefereeDto> existing = await _api.GetRefereesAsync();
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

        FootballRefereeDto? referee = await _api.CreateRefereeAsync(refPerson.Id);
        if (referee == null)
            throw new InvalidOperationException("Failed to create football referee.");

        Console.WriteLine($"  Created import referee: {referee.Id}");
        return referee.Id;
    }

    /// <summary>
    /// Gets (creating lazily) a per-team "Tuntematon" player used to attribute goals whose
    /// scorer is not present in the old data, so that final scores stay correct.
    /// </summary>
    public async Task<Guid?> GetOrCreateUnknownPlayerAsync(OldTeam oldTeam, Guid newTeamId)
    {
        List<Guid> players = await EnsureUnknownPlayersAsync(oldTeam, newTeamId, 1);
        return players.Count > 0 ? players[0] : null;
    }

    /// <summary>
    /// Ensures at least <paramref name="count"/> unique unknown players exist on the team roster.
    /// Slot 1 is the shared unknown scorer; further slots are extra lineup pads.
    /// </summary>
    public async Task<List<Guid>> EnsureUnknownPlayersAsync(OldTeam oldTeam, Guid newTeamId, int count)
    {
        List<Guid> result = [];
        if (count <= 0)
            return result;

        Guid? first = await CreateUnknownPlayerInternalAsync(oldTeam, newTeamId, $"({oldTeam.Name})", cachePrimary: true);
        if (first == null)
            return result;
        result.Add(first.Value);

        if (!_idMap.ExtraUnknownPlayers.TryGetValue(oldTeam.Id, out List<Guid>? extras))
        {
            extras = [];
            _idMap.ExtraUnknownPlayers[oldTeam.Id] = extras;
        }

        int nextSlot = 2;
        while (result.Count < count)
        {
            int extraIndex = nextSlot - 2;
            if (extraIndex < extras.Count)
            {
                result.Add(extras[extraIndex]);
                nextSlot++;
                continue;
            }

            Guid? extra = await CreateUnknownPlayerInternalAsync(
                oldTeam, newTeamId, $"({oldTeam.Name} {nextSlot})", cachePrimary: false);
            if (extra == null)
                break;
            extras.Add(extra.Value);
            result.Add(extra.Value);
            nextSlot++;
        }

        _idMap.Save();
        return result;
    }

    private async Task<Guid?> CreateUnknownPlayerInternalAsync(
        OldTeam oldTeam,
        Guid newTeamId,
        string lastName,
        bool cachePrimary)
    {
        if (cachePrimary && _idMap.UnknownPlayers.TryGetValue(oldTeam.Id, out Guid cached))
            return cached;

        const string firstName = "Tuntematon";

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
            _log.LogError("CreateUnknownFootballPlayer", new { oldTeam.Id, oldTeam.Name, lastName }, "Person creation failed.");
            return null;
        }

        FootballPlayerDto? player = await _api.CreatePlayerAsync(person.Id);
        if (player == null)
        {
            List<FootballPlayerDto> players = await _api.GetPlayersAsync();
            player = players.FirstOrDefault(p => p.PersonId == person.Id);
        }
        if (player == null)
        {
            _log.LogError("CreateUnknownFootballPlayer", new { oldTeam.Id, oldTeam.Name, lastName }, "Player creation failed.");
            return null;
        }

        bool added = await _api.AddPlayerToTeamAsync(
            newTeamId, player.Id, (int)FootballPosition.Forward, null);
        if (!added)
        {
            _log.LogError("CreateUnknownFootballPlayer", new { oldTeam.Id, oldTeam.Name, lastName }, "Adding player to team roster failed.");
            return null;
        }

        if (cachePrimary)
            _idMap.UnknownPlayers[oldTeam.Id] = player.Id;

        return player.Id;
    }

    public async Task<FootballSeasonDto?> ImportSeasonAsync(ProjectImport pi, DivisionDto division)
    {
        OldProject project = pi.Project;
        TeamCategory teamCategory = TeamCategoryResolver.InferFromName(project.Name);
        int halves = HobbyNumberOfHalves;
        int halfMinutes = HobbyHalfDurationMinutes;
        if (project.PeriodDurationMinutes is >= 15 and <= 30)
            halfMinutes = project.PeriodDurationMinutes;

        List<FootballSeasonDto> existing = await _api.GetSeasonsAsync();

        if (_idMap.Seasons.TryGetValue(project.Id, out Guid mappedSeasonId))
        {
            FootballSeasonDto? mapped = existing.FirstOrDefault(s => s.Id == mappedSeasonId);
            if (mapped != null)
            {
                mapped = await EnsureSeasonCategoryAsync(mapped, teamCategory);
                Console.WriteLine(
                    $"  Season already imported: '{mapped.Name}' ({mapped.Id}) [{mapped.TeamCategory}]");
                await _api.EnsureSeasonContentBlocksAsync("api/FootballSeason", mapped.Id, project);
                await EnsureTeamsInSeasonAsync(pi, mapped, division);
                return mapped;
            }
        }

        string seasonName = project.Name;
        FootballSeasonDto? byName = existing.FirstOrDefault(s =>
            string.Equals(s.Name, seasonName, StringComparison.OrdinalIgnoreCase));
        if (byName != null && _idMap.Seasons.Values.Contains(byName.Id))
        {
            seasonName = $"{project.Name} [JL{project.Id}]";
            byName = existing.FirstOrDefault(s =>
                string.Equals(s.Name, seasonName, StringComparison.OrdinalIgnoreCase));
        }

        FootballSeasonDto? season = byName;
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
                halves, halfMinutes, HobbyPlayersOnField,
                teamCategory);
            if (season == null)
            {
                _log.LogError("CreateFootballSeason", new { project.Id, seasonName, teamCategory }, "API returned null.");
                return null;
            }
            Console.WriteLine($"  Created season '{seasonName}' ({season.Id}) [{teamCategory}] {halves}×{halfMinutes} min, {HobbyPlayersOnField}v{HobbyPlayersOnField}");
        }
        else
        {
            season = await EnsureSeasonCategoryAsync(season, teamCategory);
            Console.WriteLine($"  Using existing season '{seasonName}' ({season.Id}) [{season.TeamCategory}]");
        }

        _idMap.Seasons[project.Id] = season.Id;
        _idMap.Save();

        await _api.EnsureSeasonContentBlocksAsync("api/FootballSeason", season.Id, project);
        await EnsureTeamsInSeasonAsync(pi, season, division);

        return season;
    }

    private async Task<FootballSeasonDto> EnsureSeasonCategoryAsync(
        FootballSeasonDto season,
        TeamCategory expected)
    {
        if (season.TeamCategory == expected)
            return season;

        FootballSeasonDto? updated = await _api.UpdateSeasonAsync(season, expected);
        if (updated == null)
        {
            _log.LogError(
                "UpdateFootballSeasonCategory",
                new { season.Id, season.Name, From = season.TeamCategory, To = expected },
                "API returned null; keeping previous category.");
            return season;
        }

        Console.WriteLine($"  Updated season category '{season.Name}': {season.TeamCategory} → {expected}");
        return updated;
    }

    private async Task EnsureTeamsInSeasonAsync(ProjectImport pi, FootballSeasonDto season, DivisionDto division)
    {
        int teamsAdded = 0;
        HashSet<Guid> handled = [];
        foreach (ProjectTeamImport pti in pi.Teams.Values)
        {
            if (!_idMap.Teams.TryGetValue(pti.Team.Id, out Guid teamId))
                continue;
            if (!handled.Add(teamId))
                continue;
            bool ok = await _api.AddTeamToSeasonAsync(season.Id, teamId);
            await _api.AddTeamToSeasonDivisionAsync(season.Id, division.Id, teamId);
            if (ok) teamsAdded++;
        }
        Console.WriteLine($"  Teams in season: {teamsAdded}/{pi.Teams.Count}");
    }
}

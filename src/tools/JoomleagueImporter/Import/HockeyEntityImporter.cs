using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Officials.DTOs;
using Application.Features.Hockey.Players.DTOs;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Teams;
using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

/// <summary>
/// Imports clubs, persons, hockey players, teams and rosters through the API,
/// then creates hockey seasons with hobby-friendly roster rules.
/// </summary>
public class HockeyEntityImporter
{
    public const int HobbyMinDressedPlayers = 6;

    private readonly HockeyApiClient _api;
    private readonly IdMapStore _idMap;
    private readonly ImportLogger _log;

    public HockeyEntityImporter(HockeyApiClient api, IdMapStore idMap, ImportLogger log)
    {
        _api = api;
        _idMap = idMap;
        _log = log;
    }

    public async Task<DivisionDto> GetOrCreateImportDivisionAsync()
    {
        Console.WriteLine("--- Division ---");
        const string divisionName = "MAHL Jääkiekko";

        List<DivisionDto> divisions = await _api.GetDivisionsAsync();
        DivisionDto? division = divisions.FirstOrDefault(d =>
            string.Equals(d.Name, divisionName, StringComparison.OrdinalIgnoreCase));
        if (division != null)
        {
            Console.WriteLine($"  Using existing division '{division.Name}' ({division.Id})");
            return division;
        }

        division = await _api.CreateDivisionAsync(
            divisionName, "JoomLeague-tuonnin jääkiekkosarjat", 1, "Icehockey");
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
        Console.WriteLine("--- Persons & Hockey Players ---");

        Dictionary<int, HockeyPosition> positionByPerson = [];
        foreach (ProjectImport pi in set.Projects)
        {
            foreach (ProjectTeamImport pti in pi.Teams.Values)
            {
                foreach (RosterEntry re in pti.Roster)
                {
                    if (!positionByPerson.TryGetValue(re.Person.Id, out HockeyPosition existing) ||
                        existing != HockeyPosition.Goalie && re.HockeyPosition == HockeyPosition.Goalie)
                    {
                        positionByPerson[re.Person.Id] = re.HockeyPosition;
                    }
                }
            }
        }

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

            HockeyPosition position = positionByPerson.GetValueOrDefault(oldPerson.Id, HockeyPosition.Center);
            HockeyCatches? catches = position == HockeyPosition.Goalie ? HockeyCatches.Unknown : null;
            HockeyPlayerDto? player = await _api.CreatePlayerAsync(person.Id, position, catches);
            if (player == null)
            {
                _log.LogError("CreateHockeyPlayer", new { oldPerson.Id, oldPerson.FullName, NewPersonId = person.Id }, "API returned null.");
                failed++;
                continue;
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
        List<HockeyTeamDto> existing = await _api.GetTeamsAsync();
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

            HockeyTeamDto? team = null;
            TeamCategory teamCategory = categoryByTeam.GetValueOrDefault(oldTeam.Id, TeamCategory.Adult);

            if (_idMap.Teams.TryGetValue(oldTeam.Id, out Guid mappedId))
                team = existing.FirstOrDefault(t => t.Id == mappedId);

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
                        _log.LogError("CreateHockeyTeam", new { oldTeam.Id, oldTeam.Name }, "No club mapping found.");
                        continue;
                    }

                    team = await _api.CreateTeamAsync(
                        oldTeam.Name, MakeShortName(oldTeam), clubId, division.Id, teamCategory);
                    if (team == null)
                    {
                        _log.LogError("CreateHockeyTeam", new { oldTeam.Id, oldTeam.Name }, "API returned null.");
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

            team = await _api.GetTeamByIdAsync(team.Id) ?? team;
            HashSet<int> usedJerseys = UsedJerseys(team);

            int added = 0, rosterTotal = unionRoster.Count;
            foreach (RosterEntry re in unionRoster.Values)
            {
                if (!_idMap.Persons.TryGetValue(re.Person.Id, out IdMapStore.PersonMapping? mapping))
                    continue;

                int? preferred = re.TeamPlayer.JerseyNumber is > 0 and < 100 ? re.TeamPlayer.JerseyNumber : null;
                int jersey = NextJersey(usedJerseys, preferred);
                bool ok = await _api.AddPlayerToTeamAsync(team.Id, mapping.PlayerId, re.HockeyPosition, jersey);
                if (!ok)
                {
                    usedJerseys.Remove(jersey);
                    jersey = NextJersey(usedJerseys, null);
                    ok = await _api.AddPlayerToTeamAsync(team.Id, mapping.PlayerId, re.HockeyPosition, jersey);
                }
                if (ok) added++;
                else usedJerseys.Remove(jersey);
            }

            int filled = await EnsureJerseyNumbersAsync(team.Id);
            Console.WriteLine($"  {oldTeam.Name}: roster {added}/{rosterTotal}" +
                              (filled > 0 ? $", filled {filled} jersey numbers" : ""));
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

    private static HashSet<int> UsedJerseys(HockeyTeamDto? team) =>
        team?.Roster
            .Where(r => r.JerseyNumber is > 0 and < 100)
            .Select(r => r.JerseyNumber!.Value)
            .ToHashSet()
        ?? [];

    private static int NextJersey(HashSet<int> used, int? preferred)
    {
        if (preferred is > 0 and < 100 && used.Add(preferred.Value))
            return preferred.Value;

        for (int number = 1; number <= 99; number++)
        {
            if (used.Add(number))
                return number;
        }

        return 99;
    }

    private async Task<int> EnsureJerseyNumbersAsync(Guid teamId)
    {
        HockeyTeamDto? team = await _api.GetTeamByIdAsync(teamId);
        if (team == null)
            return 0;

        HashSet<int> used = UsedJerseys(team);
        int filled = 0;
        foreach (HockeyTeamPlayerDto row in team.Roster)
        {
            if (row.JerseyNumber is > 0)
                continue;

            int jersey = NextJersey(used, null);
            HockeyPosition position = Enum.TryParse(row.Position, true, out HockeyPosition parsedPosition)
                ? parsedPosition
                : HockeyPosition.Center;
            HockeyRosterStatus status = Enum.TryParse(row.RosterStatus, true, out HockeyRosterStatus parsedStatus)
                ? parsedStatus
                : HockeyRosterStatus.Active;
            HockeyCaptainRole captain = Enum.TryParse(row.CaptainRole, true, out HockeyCaptainRole parsedCaptain)
                ? parsedCaptain
                : HockeyCaptainRole.None;

            if (await _api.UpdateTeamPlayerAsync(teamId, row.PlayerId, position, jersey, status, captain))
                filled++;
            else
                used.Remove(jersey);
        }

        return filled;
    }

    public async Task<Guid> GetOrCreateImportOfficialAsync()
    {
        Console.WriteLine("--- Import Official ---");
        List<HockeyOfficialDto> existing = await _api.GetOfficialsAsync();
        if (existing.Count > 0)
        {
            Console.WriteLine($"  Using existing official: {existing[0].Id}");
            return existing[0].Id;
        }

        List<PersonDto> found = await _api.SearchPersonsAsync("Import Referee");
        PersonDto? refPerson = found.FirstOrDefault(p =>
            string.Equals(p.FirstName, "Import", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(p.LastName, "Referee", StringComparison.OrdinalIgnoreCase));
        refPerson ??= await _api.CreatePersonAsync("Import", "Referee");
        if (refPerson == null)
            throw new InvalidOperationException("Failed to create official person.");

        HockeyOfficialDto? official = await _api.CreateOfficialAsync(refPerson.Id);
        if (official == null)
            throw new InvalidOperationException("Failed to create hockey official.");

        Console.WriteLine($"  Created import official: {official.Id}");
        return official.Id;
    }

    public async Task<Guid?> GetOrCreateUnknownPlayerAsync(OldTeam oldTeam, Guid newTeamId)
    {
        List<Guid> players = await EnsureUnknownPlayersAsync(oldTeam, newTeamId, 1, HockeyPosition.Center);
        return players.Count > 0 ? players[0] : null;
    }

    public async Task<Guid?> GetOrCreateUnknownGoalieAsync(OldTeam oldTeam, Guid newTeamId)
    {
        List<Guid> players = await EnsureUnknownPlayersAsync(oldTeam, newTeamId, 1, HockeyPosition.Goalie);
        return players.Count > 0 ? players[0] : null;
    }

    /// <summary>
    /// Ensures at least <paramref name="count"/> unique unknown players exist on the team roster.
    /// </summary>
    public async Task<List<Guid>> EnsureUnknownPlayersAsync(
        OldTeam oldTeam,
        Guid newTeamId,
        int count,
        HockeyPosition position = HockeyPosition.Center)
    {
        List<Guid> result = [];
        if (count <= 0)
            return result;

        string lastName = position == HockeyPosition.Goalie
            ? $"MV ({oldTeam.Name})"
            : $"({oldTeam.Name})";
        Guid? first = await CreateUnknownPlayerInternalAsync(
            oldTeam, newTeamId, lastName, position, cachePrimary: true);
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
                oldTeam, newTeamId, $"({oldTeam.Name} {nextSlot})", position, cachePrimary: false);
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
        HockeyPosition position,
        bool cachePrimary)
    {
        if (cachePrimary && position != HockeyPosition.Goalie &&
            _idMap.UnknownPlayers.TryGetValue(oldTeam.Id, out Guid cached))
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
            _log.LogError("CreateUnknownHockeyPlayer", new { oldTeam.Id, oldTeam.Name, lastName }, "Person creation failed.");
            return null;
        }

        HockeyCatches? catches = position == HockeyPosition.Goalie ? HockeyCatches.Unknown : null;
        HockeyPlayerDto? player = await _api.CreatePlayerAsync(person.Id, position, catches);
        if (player == null)
        {
            _log.LogError("CreateUnknownHockeyPlayer", new { oldTeam.Id, oldTeam.Name, lastName }, "Player creation failed.");
            return null;
        }

        HockeyTeamDto? team = await _api.GetTeamByIdAsync(newTeamId);
        int jersey = NextJersey(UsedJerseys(team), null);
        bool added = await _api.AddPlayerToTeamAsync(newTeamId, player.Id, position, jersey);
        if (!added)
        {
            _log.LogError("CreateUnknownHockeyPlayer", new { oldTeam.Id, oldTeam.Name, lastName }, "Adding player to team roster failed.");
            return null;
        }

        await EnsureJerseyNumbersAsync(newTeamId);

        if (cachePrimary && position != HockeyPosition.Goalie)
            _idMap.UnknownPlayers[oldTeam.Id] = player.Id;

        return player.Id;
    }

    public async Task<HockeySeasonDto?> ImportSeasonAsync(ProjectImport pi, DivisionDto division)
    {
        OldProject project = pi.Project;
        int periodCount = Math.Clamp(project.NumberOfPeriods, 1, 5);
        int periodMinutes = Math.Clamp(project.PeriodDurationMinutes, 1, 60);

        List<HockeySeasonDto> existing = await _api.GetSeasonsAsync();

        if (_idMap.Seasons.TryGetValue(project.Id, out Guid mappedSeasonId))
        {
            HockeySeasonDto? mapped = existing.FirstOrDefault(s => s.Id == mappedSeasonId)
                ?? await _api.GetSeasonByIdAsync(mappedSeasonId);
            if (mapped != null)
            {
                Console.WriteLine($"  Season already imported: '{mapped.Name}' ({mapped.Id})");
                await EnsureSeasonReadyAsync(mapped, pi, division, periodCount, periodMinutes);
                return await _api.GetSeasonByIdAsync(mapped.Id) ?? mapped;
            }
        }

        string seasonName = project.Name;
        HockeySeasonDto? byName = existing.FirstOrDefault(s =>
            string.Equals(s.Name, seasonName, StringComparison.OrdinalIgnoreCase));
        if (byName != null && _idMap.Seasons.Values.Contains(byName.Id))
        {
            seasonName = $"{project.Name} [JL{project.Id}]";
            byName = existing.FirstOrDefault(s =>
                string.Equals(s.Name, seasonName, StringComparison.OrdinalIgnoreCase));
        }

        HockeySeasonDto? season = byName;
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

            season = await _api.CreateSeasonAsync(seasonName, start, end);
            if (season == null)
            {
                _log.LogError("CreateHockeySeason", new { project.Id, seasonName }, "API returned null.");
                return null;
            }
            Console.WriteLine($"  Created season '{seasonName}' ({season.Id}) {periodCount}×{periodMinutes} min");
        }
        else
        {
            Console.WriteLine($"  Using existing season '{seasonName}' ({season.Id})");
        }

        _idMap.Seasons[project.Id] = season.Id;
        _idMap.Save();

        await EnsureSeasonReadyAsync(season, pi, division, periodCount, periodMinutes);
        return await _api.GetSeasonByIdAsync(season.Id) ?? season;
    }

    private async Task EnsureSeasonReadyAsync(
        HockeySeasonDto season,
        ProjectImport pi,
        DivisionDto division,
        int periodCount,
        int periodMinutes)
    {
        await _api.ApplyHobbyRulesAsync(season.Id, periodCount, periodMinutes, HobbyMinDressedPlayers);
        await _api.AddDivisionToSeasonAsync(season.Id, division.Id, division.Name);

        HockeySeasonDto? refreshed = await _api.GetSeasonByIdAsync(season.Id) ?? season;
        Guid? competitionDivisionId = refreshed.Divisions
            .FirstOrDefault(d => d.DivisionId == division.Id)?.Id;

        int teamsAdded = 0;
        HashSet<Guid> handled = [];
        foreach (ProjectTeamImport pti in pi.Teams.Values)
        {
            if (!_idMap.Teams.TryGetValue(pti.Team.Id, out Guid teamId))
                continue;
            if (!handled.Add(teamId))
                continue;

            HockeyCompetitionTeamDto? competitionTeam = refreshed.Teams.FirstOrDefault(t => t.TeamId == teamId);
            if (competitionTeam == null)
            {
                competitionTeam = await _api.AddTeamToSeasonAsync(season.Id, teamId);
                if (competitionTeam != null)
                    teamsAdded++;
            }
            else
            {
                teamsAdded++;
            }

            if (competitionTeam != null && competitionDivisionId.HasValue)
                await _api.AddTeamToSeasonDivisionAsync(season.Id, competitionDivisionId.Value, competitionTeam.Id);
        }

        await _api.PublishSeasonAsync(season.Id);
        await _api.OpenSeasonRegistrationAsync(season.Id);
        await _api.ActivateSeasonAsync(season.Id);
        Console.WriteLine($"  Teams in season: {teamsAdded}/{pi.Teams.Count}");
    }
}

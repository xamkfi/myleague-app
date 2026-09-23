using System.Collections.Concurrent;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
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

    private const string SportLabel = "Jääkiekko";
    private const string SportType = "Icehockey";

    private readonly HockeyApiClient _api;
    private readonly IdMapStore _idMap;
    private readonly ImportLogger _log;
    private readonly ImportDivisionCatalog _divisions;

    public HockeyEntityImporter(HockeyApiClient api, IdMapStore idMap, ImportLogger log)
    {
        _api = api;
        _idMap = idMap;
        _log = log;
        _divisions = new ImportDivisionCatalog(api);
    }

    public Task ImportClubsAsync(FloorballImportSet set, JoomleagueDatabase db) =>
        ClubEntityImport.ImportAsync(_api, _idMap, _log, set, db);

    public async Task ImportPersonsAndPlayersAsync(FloorballImportSet set)
    {
        Console.WriteLine("--- Persons & Hockey Players ---");

        if (set.UniquePersons.Keys.All(_idMap.HasPerson))
        {
            Console.WriteLine($"  Persons: 0 created, {set.UniquePersons.Count} already mapped.");
            return;
        }

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

        List<OldPerson> pending = set.UniquePersons.Values.Where(p => !_idMap.HasPerson(p.Id)).ToList();
        int created = 0, reused = 0, failed = 0;
        int done = set.UniquePersons.Count - pending.Count;
        int total = set.UniquePersons.Count;

        Console.WriteLine($"  Importing {pending.Count} persons (concurrency {MatchImportParallel.PersonDegree})...");
        await MatchImportParallel.ForEachPersonAsync(pending, async oldPerson =>
        {
            (PersonDto? person, bool wasCreated) = await _api.FindOrCreatePersonAsync(
                oldPerson.FirstName, oldPerson.LastName, oldPerson.Birthday);
            if (person == null)
            {
                _log.LogError("CreatePerson", new { oldPerson.Id, oldPerson.FullName }, "API returned null.");
                Interlocked.Increment(ref failed);
                return;
            }

            if (wasCreated) Interlocked.Increment(ref created);
            else Interlocked.Increment(ref reused);

            HockeyPosition position = positionByPerson.GetValueOrDefault(oldPerson.Id, HockeyPosition.Center);
            HockeyCatches? catches = position == HockeyPosition.Goalie ? HockeyCatches.Unknown : null;
            HockeyPlayerDto? player = await _api.CreatePlayerAsync(person.Id, position, catches);
            if (player == null)
            {
                _log.LogError("CreateHockeyPlayer", new { oldPerson.Id, oldPerson.FullName, NewPersonId = person.Id }, "API returned null.");
                Interlocked.Increment(ref failed);
                return;
            }

            _idMap.MapPerson(oldPerson.Id, new IdMapStore.PersonMapping { PersonId = person.Id, PlayerId = player.Id });

            int n = Interlocked.Increment(ref done);
            if (n % 100 == 0)
            {
                _idMap.Save(force: true);
                Console.WriteLine($"  ... {n}/{total} persons processed");
            }
        });

        _idMap.Save(force: true);
        Console.WriteLine($"  Persons: {created} created, {reused} already existed, {failed} failed (total {total}).");
    }

    public async Task ImportTeamsAsync(FloorballImportSet set, JoomleagueDatabase db)
    {
        Console.WriteLine("--- Teams & Rosters ---");
        (_, Dictionary<int, TeamCategory> categories) = TeamRosterUnion.Build(set);

        Dictionary<Guid, HockeyTeamDto> byId = [];
        ConcurrentDictionary<string, HockeyTeamDto> byName = new(StringComparer.OrdinalIgnoreCase);
        foreach (HockeyTeamDto existing in await _api.GetTeamsAsync())
        {
            byId[existing.Id] = existing;
            byName.TryAdd(existing.Name, existing);
        }

        int created = 0, reused = 0, updated = 0;
        List<OldTeam> teams = set.UniqueTeams.Values.ToList();
        Console.WriteLine($"  Importing {teams.Count} teams (concurrency {MatchImportParallel.TeamDegree})...");
        await MatchImportParallel.ForEachTeamAsync(teams, async oldTeam =>
        {
            TeamCategory teamCategory = categories.TryGetValue(oldTeam.Id, out TeamCategory fromProjects)
                ? fromProjects
                : TeamCategoryResolver.InferFromName(oldTeam.Name);
            ImportSeriesProfile home = ImportSeriesResolver.HomeForTeam(set, oldTeam.Id, SportLabel);
            DivisionDto division = await _divisions.GetOrCreateAsync(home, SportType);

            HockeyTeamDto? team = null;
            if (_idMap.Teams.TryGetValue(oldTeam.Id, out Guid mappedId))
                byId.TryGetValue(mappedId, out team);
            if (team == null)
                byName.TryGetValue(oldTeam.Name, out team);

            if (team == null)
            {
                int clubKey = oldTeam.ClubId.HasValue && db.Clubs.ContainsKey(oldTeam.ClubId.Value)
                    ? oldTeam.ClubId.Value
                    : -oldTeam.Id;
                if (!_idMap.TryGetClub(clubKey, out Guid clubId))
                {
                    _log.LogError("CreateHockeyTeam", new { oldTeam.Id, oldTeam.Name }, "No club mapping found.");
                    return;
                }

                team = await _api.CreateTeamAsync(
                    oldTeam.Name, MakeShortName(oldTeam), clubId, division.Id, teamCategory);
                if (team == null)
                {
                    _log.LogError("CreateHockeyTeam", new { oldTeam.Id, oldTeam.Name }, "API returned null.");
                    return;
                }
                byId[team.Id] = team;
                byName.TryAdd(team.Name, team);
                Interlocked.Increment(ref created);
            }
            else
            {
                Interlocked.Increment(ref reused);
                if (await _api.UpdateTeamPlacementAsync(team, division.Id, teamCategory))
                    Interlocked.Increment(ref updated);
            }

            _idMap.MapTeam(oldTeam.Id, team.Id);
        });

        _idMap.Save(force: true);
        Console.WriteLine($"  Teams: {created} created, {reused} already existed, {updated} placement updated.");
    }

    public Task ApplyActiveMembershipsAsync(FloorballImportSet set)
    {
        _ = set;
        Console.WriteLine("--- Active club memberships ---");
        Console.WriteLine("  Skipped: competition-scoped rosters keep each season independent.");
        return Task.CompletedTask;
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

    public async Task<Guid?> GetOrCreateUnknownPlayerAsync(
        OldTeam oldTeam,
        Guid newTeamId,
        Guid? competitionId = null)
    {
        List<Guid> players = await EnsureUnknownPlayersAsync(
            oldTeam, newTeamId, 1, HockeyPosition.Center, competitionId);
        return players.Count > 0 ? players[0] : null;
    }

    public async Task<Guid?> GetOrCreateUnknownGoalieAsync(
        OldTeam oldTeam,
        Guid newTeamId,
        Guid? competitionId = null)
    {
        List<Guid> players = await EnsureUnknownPlayersAsync(
            oldTeam, newTeamId, 1, HockeyPosition.Goalie, competitionId);
        return players.Count > 0 ? players[0] : null;
    }

    /// <summary>
    /// Ensures at least <paramref name="count"/> unique unknown players exist on the team roster.
    /// </summary>
    public async Task<List<Guid>> EnsureUnknownPlayersAsync(
        OldTeam oldTeam,
        Guid newTeamId,
        int count,
        HockeyPosition position = HockeyPosition.Center,
        Guid? competitionId = null)
    {
        List<Guid> result = [];
        if (count <= 0)
            return result;

        string lastName = position == HockeyPosition.Goalie
            ? $"MV ({oldTeam.Name})"
            : $"({oldTeam.Name})";
        Guid? first = await CreateUnknownPlayerInternalAsync(
            oldTeam, newTeamId, lastName, position, cachePrimary: true, competitionId);
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
                oldTeam, newTeamId, $"({oldTeam.Name} {nextSlot})", position, cachePrimary: false, competitionId);
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
        bool cachePrimary,
        Guid? competitionId = null)
    {
        if (cachePrimary && position != HockeyPosition.Goalie &&
            _idMap.UnknownPlayers.TryGetValue(oldTeam.Id, out Guid cached))
        {
            await _api.AddPlayerToTeamAsync(newTeamId, cached, position, jerseyNumber: null, competitionId);
            return cached;
        }

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

        HockeyTeamDto? team = await _api.GetTeamByIdAsync(newTeamId, competitionId);
        int jersey = NextJersey(UsedJerseys(team), null);
        bool added = await _api.AddPlayerToTeamAsync(newTeamId, player.Id, position, jersey, competitionId);
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

    public async Task<HockeySeasonDto?> ImportSeasonAsync(ProjectImport pi)
    {
        OldProject project = pi.Project;
        ImportSeriesProfile profile = ImportSeriesResolver.Resolve(project.Name, SportLabel);
        DivisionDto division = await _divisions.GetOrCreateAsync(profile, SportType);
        TeamCategory teamCategory = profile.Category;
        int periodCount = Math.Clamp(project.NumberOfPeriods, 1, 5);
        int periodMinutes = Math.Clamp(project.PeriodDurationMinutes, 1, 60);

        List<HockeySeasonDto> existing = await _api.GetSeasonsAsync();

        if (_idMap.TryGetSeason(project.Id, out Guid mappedSeasonId))
        {
            HockeySeasonDto? mapped = existing.FirstOrDefault(s => s.Id == mappedSeasonId)
                ?? await _api.GetSeasonByIdAsync(mappedSeasonId);
            if (mapped != null)
            {
                mapped = await EnsureSeasonCategoryAsync(mapped, teamCategory);
                Console.WriteLine($"  Season already imported: '{mapped.Name}' ({mapped.Id}) [{mapped.TeamCategory}]");
                await EnsureSeasonReadyAsync(mapped, pi, division, periodCount, periodMinutes);
                return await _api.GetSeasonByIdAsync(mapped.Id) ?? mapped;
            }
        }

        string seasonName = project.Name;
        HockeySeasonDto? byName = existing.FirstOrDefault(s =>
            string.Equals(s.Name, seasonName, StringComparison.OrdinalIgnoreCase));
        if (byName != null && _idMap.HasMappedSeasonId(byName.Id))
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

            season = await _api.CreateSeasonAsync(seasonName, start, end, teamCategory);
            if (season == null)
            {
                _log.LogError("CreateHockeySeason", new { project.Id, seasonName, teamCategory }, "API returned null.");
                return null;
            }
            Console.WriteLine($"  Created season '{seasonName}' ({season.Id}) [{teamCategory}] {periodCount}×{periodMinutes} min");
        }
        else
        {
            season = await EnsureSeasonCategoryAsync(season, teamCategory);
            Console.WriteLine($"  Using existing season '{seasonName}' ({season.Id}) [{season.TeamCategory}]");
        }

        _idMap.MapSeason(project.Id, season.Id);

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
        await _api.EnsureSeasonContentBlocksAsync("api/HockeySeason", season.Id, pi.Project);
        await _api.ApplyHobbyRulesAsync(season.Id, periodCount, periodMinutes, HobbyMinDressedPlayers);
        bool divisionPresent = season.Divisions.Any(item => item.DivisionId == division.Id);
        if (!divisionPresent)
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

            await ImportProjectRosterAsync(pti, teamId, season.Id);
        }

        if (season.EndDate.Date >= DateTime.UtcNow.Date)
        {
            await _api.PublishSeasonAsync(season.Id);
            await _api.OpenSeasonRegistrationAsync(season.Id);
            await _api.ActivateSeasonAsync(season.Id);
        }
        Console.WriteLine($"  Teams in season: {teamsAdded}/{pi.Teams.Count}");
    }

    private async Task<HockeySeasonDto> EnsureSeasonCategoryAsync(HockeySeasonDto season, TeamCategory expected)
    {
        if (string.Equals(season.TeamCategory, expected.ToString(), StringComparison.OrdinalIgnoreCase))
            return season;

        HockeySeasonDto? updated = await _api.UpdateSeasonAsync(season, expected);
        if (updated == null)
        {
            _log.LogError(
                "UpdateHockeySeasonCategory",
                new { season.Id, season.Name, From = season.TeamCategory, To = expected },
                "API returned null; keeping previous category.");
            return season;
        }

        Console.WriteLine($"  Updated season category '{season.Name}': {season.TeamCategory} → {expected}");
        return updated;
    }

    private async Task ImportProjectRosterAsync(ProjectTeamImport pti, Guid teamId, Guid competitionId)
    {
        HockeyTeamDto? team = await _api.GetTeamByIdAsync(teamId, competitionId);
        HashSet<int> usedJerseys = UsedJerseys(team);

        int added = 0;
        foreach (RosterEntry re in pti.Roster)
        {
            if (!_idMap.TryGetPerson(re.Person.Id, out IdMapStore.PersonMapping? mapping) || mapping == null)
                continue;

            int? preferred = re.TeamPlayer.JerseyNumber is > 0 and < 100 ? re.TeamPlayer.JerseyNumber : null;
            int jersey = NextJersey(usedJerseys, preferred);
            bool ok = await _api.AddPlayerToTeamAsync(teamId, mapping.PlayerId, re.HockeyPosition, jersey, competitionId);
            if (!ok)
            {
                usedJerseys.Remove(jersey);
                jersey = NextJersey(usedJerseys, null);
                ok = await _api.AddPlayerToTeamAsync(teamId, mapping.PlayerId, re.HockeyPosition, jersey, competitionId);
            }

            if (ok)
                added++;
            else
                usedJerseys.Remove(jersey);
        }

        Console.WriteLine($"    Roster {pti.Team.Name}: {added}/{pti.Roster.Count}");
    }
}

using System.Collections.Concurrent;
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
    private readonly SemaphoreSlim _unknownPlayerLock = new(1, 1);

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

    public Task ImportClubsAsync(FloorballImportSet set, JoomleagueDatabase db) =>
        ClubEntityImport.ImportAsync(_api, _idMap, _log, set, db);

    // ── Persons & players ────────────────────────────────────

    public async Task ImportPersonsAndPlayersAsync(FloorballImportSet set)
    {
        Console.WriteLine("--- Persons & Floorball Players ---");

        if (set.UniquePersons.Keys.All(_idMap.HasPerson))
        {
            Console.WriteLine($"  Persons: 0 created, {set.UniquePersons.Count} already mapped.");
            return;
        }

        List<FloorballPlayerDto> existingPlayers = await _api.GetPlayersAsync();
        ConcurrentDictionary<Guid, FloorballPlayerDto> playerByPersonId = new();
        foreach (FloorballPlayerDto p in existingPlayers)
            playerByPersonId[p.PersonId] = p;

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

            if (!playerByPersonId.TryGetValue(person.Id, out FloorballPlayerDto? player))
            {
                player = await _api.CreatePlayerAsync(person.Id);
                if (player == null)
                {
                    _log.LogError("CreatePlayer", new { oldPerson.Id, oldPerson.FullName, NewPersonId = person.Id }, "API returned null.");
                    Interlocked.Increment(ref failed);
                    return;
                }
                playerByPersonId[person.Id] = player;
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

    // ── Teams & rosters ──────────────────────────────────────

    public async Task ImportTeamsAsync(FloorballImportSet set, JoomleagueDatabase db, DivisionDto division)
    {
        Console.WriteLine("--- Teams & Rosters ---");
        int mappedTeams = set.UniqueTeams.Keys.Count(_idMap.HasTeam);
        if (mappedTeams == set.UniqueTeams.Count)
        {
            Console.WriteLine($"  Teams: 0 created, {mappedTeams} already mapped (roster check skipped).");
            return;
        }

        ConcurrentDictionary<string, FloorballTeamDto> byName = new(StringComparer.OrdinalIgnoreCase);
        foreach (FloorballTeamDto t in await _api.GetTeamsAsync())
            byName.TryAdd(t.Name, t);

        (Dictionary<int, Dictionary<int, RosterEntry>> rosterByTeam, Dictionary<int, TeamCategory> categoryByTeam) =
            TeamRosterUnion.Build(set);

        List<OldTeam> pending = [];
        foreach (OldTeam oldTeam in set.UniqueTeams.Values)
        {
            if (!rosterByTeam.TryGetValue(oldTeam.Id, out Dictionary<int, RosterEntry>? unionRoster) ||
                unionRoster.Count == 0)
            {
                Console.WriteLine($"  SKIP {oldTeam.Name}: 0 roster players");
                continue;
            }

            if (_idMap.HasTeam(oldTeam.Id))
                continue;
            pending.Add(oldTeam);
        }

        int created = 0, reused = 0;
        Console.WriteLine($"  Importing {pending.Count} teams (concurrency {MatchImportParallel.TeamDegree})...");
        await MatchImportParallel.ForEachTeamAsync(pending, async oldTeam =>
        {
            Dictionary<int, RosterEntry> unionRoster = rosterByTeam[oldTeam.Id];
            TeamCategory teamCategory = categoryByTeam.GetValueOrDefault(oldTeam.Id, TeamCategory.Adult);

            if (!byName.TryGetValue(oldTeam.Name, out FloorballTeamDto? team))
            {
                int clubKey = oldTeam.ClubId.HasValue && db.Clubs.ContainsKey(oldTeam.ClubId.Value)
                    ? oldTeam.ClubId.Value
                    : -oldTeam.Id;
                if (!_idMap.TryGetClub(clubKey, out Guid clubId))
                {
                    _log.LogError("CreateTeam", new { oldTeam.Id, oldTeam.Name }, "No club mapping found.");
                    return;
                }

                team = await _api.CreateTeamAsync(
                    oldTeam.Name, MakeShortName(oldTeam), clubId, division.Id, teamCategory);
                if (team == null)
                {
                    _log.LogError("CreateTeam", new { oldTeam.Id, oldTeam.Name }, "API returned null.");
                    return;
                }
                byName.TryAdd(team.Name, team);
                Interlocked.Increment(ref created);
            }
            else
            {
                Interlocked.Increment(ref reused);
            }

            _idMap.MapTeam(oldTeam.Id, team.Id);

            int added = 0;
            foreach (RosterEntry re in unionRoster.Values)
            {
                if (!_idMap.TryGetPerson(re.Person.Id, out IdMapStore.PersonMapping? mapping) || mapping == null)
                    continue;

                int position = re.IsGoalkeeper ? PositionGoalkeeper : PositionForward;
                int? jersey = re.TeamPlayer.JerseyNumber is > 0 and < 100 ? re.TeamPlayer.JerseyNumber : null;
                if (await _api.AddPlayerToTeamAsync(team.Id, mapping.PlayerId, position, jersey))
                    added++;
            }

            Console.WriteLine($"  {oldTeam.Name}: roster {added}/{unionRoster.Count}");
        });

        _idMap.Save(force: true);
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
        await _unknownPlayerLock.WaitAsync();
        try
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
        finally
        {
            _unknownPlayerLock.Release();
        }
    }

    // ── Season ───────────────────────────────────────────────

    public async Task<FloorballSeasonDto?> ImportSeasonAsync(ProjectImport pi, DivisionDto division)
    {
        OldProject project = pi.Project;
        TeamCategory teamCategory = TeamCategoryResolver.InferFromName(project.Name);

        List<FloorballSeasonDto> existing = await _api.GetSeasonsAsync();

        if (_idMap.TryGetSeason(project.Id, out Guid mappedSeasonId))
        {
            FloorballSeasonDto? mapped = existing.FirstOrDefault(s => s.Id == mappedSeasonId);
            if (mapped != null)
            {
                mapped = await EnsureSeasonCategoryAsync(mapped, teamCategory);
                Console.WriteLine(
                    $"  Season already imported: '{mapped.Name}' ({mapped.Id}) [{mapped.TeamCategory}]");
                await _api.EnsureSeasonContentBlocksAsync("api/floorballseason", mapped.Id, project);
                // Team membership is idempotent on the API side, so always (re-)ensure it;
                // an earlier run may have failed to add some teams.
                await EnsureTeamsInSeasonAsync(pi, mapped, division);
                return mapped;
            }
        }

        string seasonName = project.Name;
        FloorballSeasonDto? byName = existing.FirstOrDefault(s =>
            string.Equals(s.Name, seasonName, StringComparison.OrdinalIgnoreCase));
        if (byName != null && _idMap.HasMappedSeasonId(byName.Id))
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

        _idMap.MapSeason(project.Id, season.Id);

        await _api.EnsureSeasonContentBlocksAsync("api/floorballseason", season.Id, project);
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

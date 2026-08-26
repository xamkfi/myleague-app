using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Teams;
using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

/// <summary>
/// Imports hockey matches: confirm roster, start, periods, goals/assists/penalties, finish, stats.
/// </summary>
public class HockeyMatchImporter
{
    private const int MaxDressedPlayers = 20;

    private readonly HockeyApiClient _api;
    private readonly IdMapStore _idMap;
    private readonly ImportLogger _log;
    private readonly HockeyEntityImporter _entities;
    private readonly JoomleagueDatabase _db;
    private readonly bool _fillUnknownGoals;
    private readonly HashSet<int> _repairMatchIds;
    private readonly bool _repairAll;

    public int Succeeded { get; private set; }
    public int ScheduledOnly { get; private set; }
    public int Skipped { get; private set; }
    public int Failed { get; private set; }
    public int Repaired { get; private set; }

    public HockeyMatchImporter(
        HockeyApiClient api,
        IdMapStore idMap,
        ImportLogger log,
        HockeyEntityImporter entities,
        JoomleagueDatabase db,
        bool fillUnknownGoals,
        HashSet<int>? repairMatchIds = null,
        bool repairAll = false)
    {
        _api = api;
        _idMap = idMap;
        _log = log;
        _entities = entities;
        _db = db;
        _fillUnknownGoals = fillUnknownGoals;
        _repairMatchIds = repairMatchIds ?? [];
        _repairAll = repairAll;
    }

    private class SideInfo
    {
        public required OldTeam OldTeam { get; init; }
        public required Guid TeamId { get; init; }
        public Guid? GoaliePlayerId { get; set; }
        public List<Guid> RosterPlayerIds { get; } = [];
    }

    private class GoalRec
    {
        public int ProjectTeamId { get; init; }
        public Guid? ScorerPlayerId { get; set; }
        public int TimeSeconds { get; init; }
        public Guid? AssisterPlayerId { get; set; }
        public Guid? SecondaryAssisterPlayerId { get; set; }
        public HockeyGoalStrength Strength { get; init; } = HockeyGoalStrength.EvenStrength;
    }

    private class PenaltyRec
    {
        public int ProjectTeamId { get; init; }
        public Guid? PlayerId { get; init; }
        public int Minutes { get; init; }
        public int TimeSeconds { get; init; }
        public HockeyPenaltySeverity Severity { get; init; }
    }

    public async Task ImportProjectMatchesAsync(ProjectImport pi, HockeySeasonDto season, Guid officialId)
    {
        OldProject project = pi.Project;
        int periodSeconds = project.PeriodDurationMinutes * 60;
        int regularPeriods = project.NumberOfPeriods;
        Guid? competitionDivisionId = season.Divisions.FirstOrDefault()?.Id;

        Dictionary<int, SideInfo> sides = [];
        Dictionary<int, Guid> playerByTeamPlayerId = [];

        foreach (ProjectTeamImport pti in pi.Teams.Values)
        {
            if (!_idMap.Teams.TryGetValue(pti.Team.Id, out Guid teamId))
                continue;

            SideInfo side = new() { OldTeam = pti.Team, TeamId = teamId };

            RosterEntry? goalie = pti.Roster.FirstOrDefault(r => r.IsGoalkeeper && _idMap.Persons.ContainsKey(r.Person.Id))
                               ?? pti.Roster.FirstOrDefault(r => _idMap.Persons.ContainsKey(r.Person.Id));
            if (goalie != null)
                side.GoaliePlayerId = _idMap.Persons[goalie.Person.Id].PlayerId;

            foreach (RosterEntry re in pti.Roster)
            {
                if (!_idMap.Persons.TryGetValue(re.Person.Id, out IdMapStore.PersonMapping? mapping))
                    continue;
                playerByTeamPlayerId[re.TeamPlayer.Id] = mapping.PlayerId;
                if (!side.RosterPlayerIds.Contains(mapping.PlayerId))
                    side.RosterPlayerIds.Add(mapping.PlayerId);
            }

            sides[pti.ProjectTeam.Id] = side;
        }

        int index = 0;
        foreach (MatchImport mi in pi.Matches)
        {
            index++;
            OldMatch match = mi.Match;
            string prefix = $"  [{index}/{pi.Matches.Count}] JL#{match.Id}";

            bool alreadyProcessed = _idMap.ProcessedMatches.TryGetValue(match.Id, out Guid existingMatchId);
            bool repairRequested = alreadyProcessed && (_repairAll || _repairMatchIds.Contains(match.Id));

            if (alreadyProcessed && !repairRequested)
            {
                Skipped++;
                continue;
            }

            if (match.Cancelled)
            {
                Skipped++;
                _log.LogInfo($"Match JL#{match.Id} skipped (cancelled in old system).");
                continue;
            }

            SideInfo? home = sides.GetValueOrDefault(match.ProjectTeam1Id);
            SideInfo? away = sides.GetValueOrDefault(match.ProjectTeam2Id);
            if (home == null || away == null || home.TeamId == away.TeamId)
            {
                Skipped++;
                _log.LogError("MatchTeams", new { match.Id, match.ProjectTeam1Id, match.ProjectTeam2Id },
                    "Home or away projectteam could not be resolved to an imported team.");
                continue;
            }

            try
            {
                if (repairRequested)
                {
                    bool ok = await RepairMatchAsync(
                        mi, existingMatchId, home, away, playerByTeamPlayerId, periodSeconds, regularPeriods, prefix);
                    if (ok) Repaired++;
                    else Failed++;
                }
                else
                {
                    bool ok = await ImportSingleMatchAsync(
                        mi, season, competitionDivisionId, officialId, home, away,
                        playerByTeamPlayerId, periodSeconds, regularPeriods, prefix);
                    if (!ok) Failed++;
                }
            }
            catch (Exception ex)
            {
                Failed++;
                Console.WriteLine($"{prefix} ERROR: {ex.Message}");
                _log.LogError("ImportHockeyMatch", new { match.Id }, ex.ToString());
            }
        }
    }

    private async Task<bool> ImportSingleMatchAsync(
        MatchImport mi,
        HockeySeasonDto season,
        Guid? competitionDivisionId,
        Guid officialId,
        SideInfo home,
        SideInfo away,
        Dictionary<int, Guid> playerByTeamPlayerId,
        int periodSeconds,
        int regularPeriods,
        string prefix)
    {
        OldMatch match = mi.Match;
        DateTime scheduled = match.MatchDate ?? new DateTime(2000, 1, 1, 18, 0, 0);
        string? venue = match.PlaygroundId.HasValue ? _db.Playgrounds.GetValueOrDefault(match.PlaygroundId.Value) : null;

        HockeyMatchDto? created = await _api.CreateMatchAsync(season.Id, competitionDivisionId, scheduled, venue);
        if (created == null)
        {
            _log.LogError("CreateHockeyMatch", new { match.Id, Home = home.OldTeam.Name, Away = away.OldTeam.Name, scheduled }, "API returned null.");
            return false;
        }

        created = await _api.SetMatchTeamsAsync(created.Id, home.TeamId, away.TeamId) ?? created;

        if (!match.HasResult)
        {
            _idMap.ProcessedMatches[match.Id] = created.Id;
            _idMap.Save();
            ScheduledOnly++;
            Console.WriteLine($"{prefix} {home.OldTeam.Name} - {away.OldTeam.Name}: scheduled only");
            return true;
        }

        (List<GoalRec> goals, List<PenaltyRec> penalties, int ignoredEvents) =
            await BuildEventsAsync(mi, home, away, playerByTeamPlayerId, periodSeconds, regularPeriods);

        HockeyMatchDto? dressed = await ConfirmSidesAsync(created, home, away, goals, penalties);
        if (dressed == null)
        {
            _idMap.ProcessedMatches[match.Id] = created.Id;
            _idMap.Save();
            ScheduledOnly++;
            _log.LogWarning("NoRoster",
                $"Match JL#{match.Id} left as Scheduled: could not confirm hockey rosters.");
            Console.WriteLine($"{prefix} {home.OldTeam.Name} - {away.OldTeam.Name}: scheduled only (roster)");
            return true;
        }

        await _api.AddMatchOfficialAsync(dressed.Id, officialId);

        if (!await _api.StartMatchAsync(dressed.Id, scheduled))
        {
            _idMap.ProcessedMatches[match.Id] = dressed.Id;
            _idMap.Save();
            _log.LogError("StartHockeyMatch", new { match.Id, NewMatchId = dressed.Id }, "Could not start match; left as Scheduled.");
            return false;
        }

        dressed = await _api.GetMatchByIdAsync(dressed.Id) ?? dressed;

        (int goalsRecorded, int penaltiesRecorded) = await RecordEventsAsync(
            dressed, match, home, away, goals, penalties, periodSeconds, regularPeriods);

        HockeyMatchResultType resultType = ResolveResultType(match);
        DateTime endTime = scheduled.AddMinutes(Math.Max(1, regularPeriods * (periodSeconds / 60)));
        bool finished = await _api.FinishMatchAsync(dressed.Id, endTime, resultType);
        if (!finished)
            _log.LogError("FinishHockeyMatch", new { match.Id, NewMatchId = dressed.Id }, "API call failed.");

        await _api.RecalculateMatchAsync(dressed.Id);

        _idMap.ProcessedMatches[match.Id] = dressed.Id;
        _idMap.Save();
        Succeeded++;

        string eventNote = ignoredEvents > 0 ? $", {ignoredEvents} events ignored" : "";
        Console.WriteLine(
            $"{prefix} {home.OldTeam.Name} - {away.OldTeam.Name} " +
            $"{match.Team1Result}-{match.Team2Result}: {goalsRecorded} goals, {penaltiesRecorded} penalties{eventNote}");
        return true;
    }

    private async Task<bool> RepairMatchAsync(
        MatchImport mi,
        Guid newMatchId,
        SideInfo home,
        SideInfo away,
        Dictionary<int, Guid> playerByTeamPlayerId,
        int periodSeconds,
        int regularPeriods,
        string prefix)
    {
        OldMatch match = mi.Match;
        HockeyMatchDto? dto = await _api.GetMatchByIdAsync(newMatchId);
        if (dto == null)
        {
            _log.LogError("RepairHockeyMatch", new { match.Id, newMatchId }, "Match not found in new system.");
            return false;
        }

        if (!match.HasResult)
        {
            Console.WriteLine($"{prefix} {home.OldTeam.Name} - {away.OldTeam.Name}: repair skipped (no result, stays scheduled)");
            return true;
        }

        if (string.Equals(dto.Status, nameof(HockeyMatchStatus.Finished), StringComparison.OrdinalIgnoreCase))
        {
            if (!await _api.SetMatchStatusAsync(newMatchId, HockeyMatchStatus.InProgress))
            {
                _log.LogError("RepairHockeyMatch", new { match.Id, newMatchId }, "Could not reopen finished match.");
                return false;
            }
            dto = await _api.GetMatchByIdAsync(newMatchId) ?? dto;
        }
        else if (string.Equals(dto.Status, nameof(HockeyMatchStatus.Scheduled), StringComparison.OrdinalIgnoreCase))
        {
            (List<GoalRec> pendingGoals, List<PenaltyRec> pendingPenalties, _) =
                await BuildEventsAsync(mi, home, away, playerByTeamPlayerId, periodSeconds, regularPeriods);
            dto = await ConfirmSidesAsync(dto, home, away, pendingGoals, pendingPenalties);
            if (dto == null)
            {
                _log.LogError("RepairHockeyMatch", new { match.Id, newMatchId }, "Could not confirm rosters.");
                return false;
            }
            if (!await _api.StartMatchAsync(newMatchId, match.MatchDate))
            {
                _log.LogError("RepairHockeyMatch", new { match.Id, newMatchId }, "Start failed during repair.");
                return false;
            }
            dto = await _api.GetMatchByIdAsync(newMatchId) ?? dto;
        }

        foreach (HockeyMatchEventDto ev in dto.Events)
        {
            if (string.Equals(ev.EventType, "Goal", StringComparison.OrdinalIgnoreCase))
                await _api.DeleteGoalEventAsync(newMatchId, ev.Id);
            else if (string.Equals(ev.EventType, "Penalty", StringComparison.OrdinalIgnoreCase))
                await _api.DeletePenaltyEventAsync(newMatchId, ev.Id);
        }

        (List<GoalRec> goals, List<PenaltyRec> penalties, _) =
            await BuildEventsAsync(mi, home, away, playerByTeamPlayerId, periodSeconds, regularPeriods);

        dto = await _api.GetMatchByIdAsync(newMatchId) ?? dto;
        (int goalsRecorded, int penaltiesRecorded) = await RecordEventsAsync(
            dto, match, home, away, goals, penalties, periodSeconds, regularPeriods);

        HockeyMatchResultType resultType = ResolveResultType(match);
        bool finished = await _api.FinishMatchAsync(newMatchId, DateTime.UtcNow, resultType);
        if (!finished)
            _log.LogError("RepairHockeyMatch", new { match.Id, newMatchId }, "Finish failed after repair.");
        await _api.RecalculateMatchAsync(newMatchId);

        Console.WriteLine(
            $"{prefix} {home.OldTeam.Name} - {away.OldTeam.Name} " +
            $"{match.Team1Result}-{match.Team2Result}: REPAIRED, {goalsRecorded} goals, {penaltiesRecorded} penalties");
        return true;
    }

    private async Task<HockeyMatchDto?> ConfirmSidesAsync(
        HockeyMatchDto match,
        SideInfo home,
        SideInfo away,
        List<GoalRec> goals,
        List<PenaltyRec> penalties)
    {
        HashSet<Guid> homePlayers = CollectNeededFromSide(home, goals, penalties);
        HashSet<Guid> awayPlayers = CollectNeededFromSide(away, goals, penalties);

        HockeyMatchDto? current = match;
        current = await ConfirmOneSideAsync(current, home, homePlayers);
        if (current == null)
            return null;
        current = await ConfirmOneSideAsync(current, away, awayPlayers);
        return current;
    }

    private static HashSet<Guid> CollectNeededFromSide(SideInfo side, List<GoalRec> goals, List<PenaltyRec> penalties)
    {
        HashSet<Guid> needed = [];
        foreach (GoalRec goal in goals)
        {
            if (goal.ScorerPlayerId is Guid scorer)
                needed.Add(scorer);
            if (goal.AssisterPlayerId is Guid assist)
                needed.Add(assist);
            if (goal.SecondaryAssisterPlayerId is Guid second)
                needed.Add(second);
        }
        foreach (PenaltyRec pen in penalties)
        {
            if (pen.PlayerId is Guid player)
                needed.Add(player);
        }

        HashSet<Guid> onSide = side.RosterPlayerIds.ToHashSet();
        if (side.GoaliePlayerId.HasValue)
            onSide.Add(side.GoaliePlayerId.Value);
        needed.IntersectWith(onSide);
        return needed;
    }

    private async Task<HockeyMatchDto?> ConfirmOneSideAsync(HockeyMatchDto match, SideInfo side, HashSet<Guid> neededPlayerIds)
    {
        HockeyMatchTeamDto? matchTeam = match.MatchTeams.FirstOrDefault(t => t.TeamId == side.TeamId);
        if (matchTeam == null)
            return match;

        HockeyTeamDto? team = await _api.GetTeamByIdAsync(side.TeamId);
        if (team == null)
        {
            _log.LogError("ConfirmHockeyRoster", new { side.TeamId }, "Team not found.");
            return null;
        }

        List<Guid> dressedTeamPlayerIds = [];
        HashSet<Guid> usedPlayerIds = [];
        const int maxDressedGoalies = 2;
        int dressedGoalies = 0;

        bool IsGoalieRow(HockeyTeamPlayerDto? row) =>
            string.Equals(row?.Position, nameof(HockeyPosition.Goalie), StringComparison.OrdinalIgnoreCase);

        void TryAdd(Guid playerId, bool allowGoalie)
        {
            if (!usedPlayerIds.Add(playerId))
                return;
            HockeyTeamPlayerDto? rosterRow = team.Roster.FirstOrDefault(r => r.PlayerId == playerId);
            if (rosterRow == null || dressedTeamPlayerIds.Count >= MaxDressedPlayers)
                return;
            if (IsGoalieRow(rosterRow))
            {
                if (!allowGoalie || dressedGoalies >= maxDressedGoalies)
                    return;
                dressedGoalies++;
            }
            dressedTeamPlayerIds.Add(rosterRow.Id);
        }

        if (side.GoaliePlayerId.HasValue)
            TryAdd(side.GoaliePlayerId.Value, allowGoalie: true);
        foreach (HockeyTeamPlayerDto goalieRow in team.Roster.Where(IsGoalieRow))
            TryAdd(goalieRow.PlayerId, allowGoalie: true);
        foreach (Guid playerId in neededPlayerIds)
            TryAdd(playerId, allowGoalie: false);
        foreach (Guid playerId in side.RosterPlayerIds)
            TryAdd(playerId, allowGoalie: false);

        if (dressedGoalies == 0)
        {
            Guid? goaliePlayerId = await _entities.GetOrCreateUnknownGoalieAsync(side.OldTeam, side.TeamId);
            if (goaliePlayerId.HasValue)
            {
                team = await _api.GetTeamByIdAsync(side.TeamId) ?? team;
                if (dressedTeamPlayerIds.Count >= MaxDressedPlayers && dressedTeamPlayerIds.Count > 0)
                    dressedTeamPlayerIds.RemoveAt(dressedTeamPlayerIds.Count - 1);
                usedPlayerIds.Remove(goaliePlayerId.Value);
                TryAdd(goaliePlayerId.Value, allowGoalie: true);
            }
        }

        int missing = HockeyEntityImporter.HobbyMinDressedPlayers - dressedTeamPlayerIds.Count;
        if (missing > 0)
        {
            List<Guid> pads = await _entities.EnsureUnknownPlayersAsync(side.OldTeam, side.TeamId, missing);
            team = await _api.GetTeamByIdAsync(side.TeamId) ?? team;
            foreach (Guid padId in pads)
                TryAdd(padId, allowGoalie: false);
        }

        if (dressedTeamPlayerIds.Count < HockeyEntityImporter.HobbyMinDressedPlayers)
        {
            _log.LogError("ConfirmHockeyRoster", new { side.TeamId, dressedTeamPlayerIds.Count },
                "Could not dress the minimum hockey roster.");
            return null;
        }

        return await _api.ConfirmMatchRosterAsync(match.Id, matchTeam.Id, dressedTeamPlayerIds);
    }

    private async Task<(List<GoalRec> Goals, List<PenaltyRec> Penalties, int IgnoredEvents)> BuildEventsAsync(
        MatchImport mi,
        SideInfo home,
        SideInfo away,
        Dictionary<int, Guid> playerByTeamPlayerId,
        int periodSeconds,
        int regularPeriods)
    {
        OldMatch match = mi.Match;
        List<GoalRec> goals = [];
        List<(int PtId, Guid? PlayerId, int TimeSeconds)> assists = [];
        List<PenaltyRec> penalties = [];
        int ignoredEvents = 0;

        foreach (OldMatchEvent ev in mi.Events)
        {
            if (ev.ProjectTeamId != match.ProjectTeam1Id && ev.ProjectTeamId != match.ProjectTeam2Id)
            {
                if (JoomleagueDatabase.GoalEventTypes.Contains(ev.EventTypeId) ||
                    JoomleagueDatabase.AssistEventTypes.Contains(ev.EventTypeId) ||
                    ev.EventTypeId == JoomleagueDatabase.EventPenalty)
                {
                    ignoredEvents++;
                }
                continue;
            }

            int timeSeconds = ImportTimeParser.ParseEventTime(ev.EventTime) ?? 0;
            Guid? playerId = playerByTeamPlayerId.TryGetValue(ev.TeamPlayerId, out Guid pid) ? pid : null;

            if (JoomleagueDatabase.GoalEventTypes.Contains(ev.EventTypeId))
            {
                HockeyGoalStrength strength = ev.EventTypeId == JoomleagueDatabase.EventPowerPlayGoal
                    ? HockeyGoalStrength.PowerPlayOneMan
                    : ev.EventTypeId == JoomleagueDatabase.EventShortHandedGoal
                        ? HockeyGoalStrength.ShortHandedOneMan
                        : HockeyGoalStrength.EvenStrength;
                for (int i = 0; i < ev.Count; i++)
                {
                    goals.Add(new GoalRec
                    {
                        ProjectTeamId = ev.ProjectTeamId,
                        ScorerPlayerId = playerId,
                        TimeSeconds = timeSeconds,
                        Strength = strength,
                    });
                }
            }
            else if (JoomleagueDatabase.AssistEventTypes.Contains(ev.EventTypeId))
            {
                assists.Add((ev.ProjectTeamId, playerId, timeSeconds));
            }
            else if (ev.EventTypeId == JoomleagueDatabase.EventPenalty)
            {
                int minutes = Math.Clamp(ev.Count, 2, 20);
                penalties.Add(new PenaltyRec
                {
                    ProjectTeamId = ev.ProjectTeamId,
                    PlayerId = playerId,
                    Minutes = minutes,
                    TimeSeconds = timeSeconds,
                    Severity = MapPenaltySeverity(minutes),
                });
            }
        }

        foreach ((int ptId, Guid? assisterId, int timeSeconds) in assists)
        {
            if (assisterId == null) continue;
            GoalRec? target = goals.FirstOrDefault(g =>
                g.ProjectTeamId == ptId && g.TimeSeconds == timeSeconds &&
                (g.AssisterPlayerId == null || g.SecondaryAssisterPlayerId == null) &&
                g.ScorerPlayerId != assisterId);
            target ??= goals.FirstOrDefault(g =>
                g.ProjectTeamId == ptId && Math.Abs(g.TimeSeconds - timeSeconds) <= 5 &&
                (g.AssisterPlayerId == null || g.SecondaryAssisterPlayerId == null) &&
                g.ScorerPlayerId != assisterId);
            if (target == null) continue;
            if (target.AssisterPlayerId == null) target.AssisterPlayerId = assisterId;
            else target.SecondaryAssisterPlayerId = assisterId;
        }

        int endOfMatch = periodSeconds * regularPeriods;
        int homeEventGoals = goals.Count(g => g.ProjectTeamId == match.ProjectTeam1Id);
        int awayEventGoals = goals.Count(g => g.ProjectTeamId == match.ProjectTeam2Id);
        int missingHome = (match.Team1Result ?? 0) - homeEventGoals;
        int missingAway = (match.Team2Result ?? 0) - awayEventGoals;

        if (homeEventGoals > (match.Team1Result ?? 0) || awayEventGoals > (match.Team2Result ?? 0))
        {
            _log.LogWarning("ScoreMismatch",
                $"Match JL#{match.Id} has more goal events ({homeEventGoals}-{awayEventGoals}) than the recorded result " +
                $"({match.Team1Result}-{match.Team2Result}). Importing events as-is.");
        }

        if (_fillUnknownGoals)
        {
            missingHome = Math.Max(0, missingHome);
            missingAway = Math.Max(0, missingAway);
            if (missingHome > 0)
            {
                Guid? unknown = await ResolveUnknownScorerAsync(home);
                if (unknown != null)
                    for (int i = 0; i < missingHome; i++)
                        goals.Add(new GoalRec { ProjectTeamId = match.ProjectTeam1Id, ScorerPlayerId = unknown, TimeSeconds = Math.Max(0, endOfMatch - 1) });
            }
            if (missingAway > 0)
            {
                Guid? unknown = await ResolveUnknownScorerAsync(away);
                if (unknown != null)
                    for (int i = 0; i < missingAway; i++)
                        goals.Add(new GoalRec { ProjectTeamId = match.ProjectTeam2Id, ScorerPlayerId = unknown, TimeSeconds = Math.Max(0, endOfMatch - 1) });
            }
        }

        foreach (GoalRec goal in goals.Where(g => g.ScorerPlayerId == null).ToList())
        {
            SideInfo side = goal.ProjectTeamId == match.ProjectTeam1Id ? home : away;
            Guid? unknown = await ResolveUnknownScorerAsync(side);
            if (unknown != null)
            {
                goal.ScorerPlayerId = unknown;
            }
            else
            {
                goals.Remove(goal);
                _log.LogWarning("GoalDropped", $"Match JL#{match.Id}: goal at {goal.TimeSeconds}s dropped (no scorer, no unknown player).");
            }
        }

        return (goals, penalties, ignoredEvents);
    }

    private async Task<(int GoalsRecorded, int PenaltiesRecorded)> RecordEventsAsync(
        HockeyMatchDto matchDto,
        OldMatch match,
        SideInfo home,
        SideInfo away,
        List<GoalRec> goals,
        List<PenaltyRec> penalties,
        int periodSeconds,
        int regularPeriods)
    {
        int goalsRecorded = 0, penaltiesRecorded = 0;
        HockeyMatchTeamDto? homeMatchTeam = matchDto.MatchTeams.FirstOrDefault(t => t.TeamId == home.TeamId);
        HockeyMatchTeamDto? awayMatchTeam = matchDto.MatchTeams.FirstOrDefault(t => t.TeamId == away.TeamId);
        if (homeMatchTeam == null || awayMatchTeam == null)
            return (0, 0);

        HockeyTeamDto? homeTeam = await _api.GetTeamByIdAsync(home.TeamId);
        HockeyTeamDto? awayTeam = await _api.GetTeamByIdAsync(away.TeamId);
        Dictionary<Guid, Guid> homeActive = MapActivePlayersByPlayerId(homeMatchTeam, homeTeam);
        Dictionary<Guid, Guid> awayActive = MapActivePlayersByPlayerId(awayMatchTeam, awayTeam);
        Guid? homeGoalieActive = homeMatchTeam.ActivePlayers.FirstOrDefault(p => p.IsGoalie)?.Id;
        Guid? awayGoalieActive = awayMatchTeam.ActivePlayers.FirstOrDefault(p => p.IsGoalie)?.Id;

        for (int period = 1; period <= regularPeriods; period++)
        {
            int periodStart = (period - 1) * periodSeconds;
            int periodEnd = period * periodSeconds;
            await _api.RecordPeriodAsync(matchDto.Id, period, periodStart, HockeyPeriodAction.PeriodStarted);

            int currentPeriod = period;
            List<GoalRec> periodGoals = goals
                .Where(g => PeriodOf(g.TimeSeconds, periodSeconds, regularPeriods) == currentPeriod)
                .OrderBy(g => g.TimeSeconds)
                .ToList();
            List<PenaltyRec> periodPenalties = penalties
                .Where(p => PeriodOf(p.TimeSeconds, periodSeconds, regularPeriods) == currentPeriod)
                .OrderBy(p => p.TimeSeconds)
                .ToList();

            foreach (GoalRec goal in periodGoals)
            {
                bool isHome = goal.ProjectTeamId == match.ProjectTeam1Id;
                HockeyMatchTeamDto scoringTeam = isHome ? homeMatchTeam : awayMatchTeam;
                Dictionary<Guid, Guid> actives = isHome ? homeActive : awayActive;
                if (goal.ScorerPlayerId == null || !actives.TryGetValue(goal.ScorerPlayerId.Value, out Guid scorerActive))
                {
                    _log.LogWarning("GoalSkipped", $"Match JL#{match.Id}: goal at {goal.TimeSeconds}s skipped (scorer not dressed).");
                    continue;
                }

                Guid? assistActive = goal.AssisterPlayerId is Guid assist && actives.TryGetValue(assist, out Guid assistId)
                    ? assistId : null;
                Guid? secondActive = goal.SecondaryAssisterPlayerId is Guid second && actives.TryGetValue(second, out Guid secondId)
                    ? secondId : null;
                Guid? goalieActive = isHome ? awayGoalieActive : homeGoalieActive;

                bool ok = await _api.RecordGoalAsync(
                    matchDto.Id, scoringTeam.Id, scorerActive, assistActive, secondActive, goalieActive,
                    period, goal.TimeSeconds, goal.Strength);
                if (ok) goalsRecorded++;
                else _log.LogError("RecordHockeyGoal", new { match.Id, NewMatchId = matchDto.Id, period, goal.TimeSeconds }, "API call failed.");
            }

            foreach (PenaltyRec pen in periodPenalties)
            {
                bool isHome = pen.ProjectTeamId == match.ProjectTeam1Id;
                HockeyMatchTeamDto penaltyTeam = isHome ? homeMatchTeam : awayMatchTeam;
                Dictionary<Guid, Guid> actives = isHome ? homeActive : awayActive;
                Guid? penalized = pen.PlayerId is Guid player && actives.TryGetValue(player, out Guid active)
                    ? active : null;

                bool ok = await _api.RecordPenaltyAsync(
                    matchDto.Id, penaltyTeam.Id, penalized, period, pen.TimeSeconds, pen.Severity, pen.Minutes);
                if (ok) penaltiesRecorded++;
                else _log.LogError("RecordHockeyPenalty", new { match.Id, NewMatchId = matchDto.Id, period, pen.TimeSeconds }, "API call failed.");
            }

            await _api.RecordPeriodAsync(matchDto.Id, period, periodEnd, HockeyPeriodAction.PeriodEnded);
        }

        return (goalsRecorded, penaltiesRecorded);
    }

    private static Dictionary<Guid, Guid> MapActivePlayersByPlayerId(
        HockeyMatchTeamDto matchTeam,
        HockeyTeamDto? team)
    {
        Dictionary<Guid, Guid> byPlayerId = [];
        if (team == null)
            return byPlayerId;

        Dictionary<Guid, Guid> playerByTeamPlayer = team.Roster
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => g.First().PlayerId);

        foreach (HockeyMatchActivePlayerDto active in matchTeam.ActivePlayers)
        {
            if (playerByTeamPlayer.TryGetValue(active.TeamPlayerId, out Guid playerId))
                byPlayerId[playerId] = active.Id;
        }

        return byPlayerId;
    }

    private async Task<Guid?> ResolveUnknownScorerAsync(SideInfo side)
    {
        if (!_fillUnknownGoals)
            return null;
        Guid? playerId = await _entities.GetOrCreateUnknownPlayerAsync(side.OldTeam, side.TeamId);
        if (playerId.HasValue && !side.RosterPlayerIds.Contains(playerId.Value))
            side.RosterPlayerIds.Add(playerId.Value);
        return playerId;
    }

    private static HockeyPenaltySeverity MapPenaltySeverity(int minutes) => minutes switch
    {
        4 => HockeyPenaltySeverity.DoubleMinor,
        5 => HockeyPenaltySeverity.Major,
        10 => HockeyPenaltySeverity.Misconduct,
        _ => HockeyPenaltySeverity.Minor,
    };

    private static HockeyMatchResultType ResolveResultType(OldMatch match)
    {
        int home = match.Team1Result ?? 0;
        int away = match.Team2Result ?? 0;
        if (home > away) return HockeyMatchResultType.HomeWin;
        if (away > home) return HockeyMatchResultType.AwayWin;
        return HockeyMatchResultType.Draw;
    }

    private static int PeriodOf(int timeSeconds, int periodSeconds, int regularPeriods)
    {
        if (periodSeconds <= 0) return 1;
        int period = timeSeconds / periodSeconds + 1;
        return Math.Clamp(period, 1, regularPeriods);
    }
}

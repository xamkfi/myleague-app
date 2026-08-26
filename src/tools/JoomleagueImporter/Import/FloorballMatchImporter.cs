using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Seasons.DTOs;
using Domain.Enums.Floorball;
using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

/// <summary>
/// Imports the matches of one old project into the corresponding new season, replaying the
/// full lifecycle (create, goalies, start, periods, goals/assists/penalties, complete).
/// </summary>
public class FloorballMatchImporter
{
    private readonly FloorballApiClient _api;
    private readonly IdMapStore _idMap;
    private readonly ImportLogger _log;
    private readonly FloorballEntityImporter _entities;
    private readonly JoomleagueDatabase _db;
    private readonly bool _fillUnknownGoals;
    private readonly HashSet<int> _repairMatchIds;
    private readonly bool _repairAll;

    public int Succeeded { get; private set; }
    public int ScheduledOnly { get; private set; }
    public int Skipped { get; private set; }
    public int Failed { get; private set; }
    public int Repaired { get; private set; }

    public FloorballMatchImporter(
        FloorballApiClient api,
        IdMapStore idMap,
        ImportLogger log,
        FloorballEntityImporter entities,
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
    }

    private class GoalRec
    {
        public int ProjectTeamId { get; init; }
        public Guid? ScorerPlayerId { get; set; }
        public int TimeSeconds { get; init; }
        public Guid? AssisterPlayerId { get; set; }
        public Guid? SecondaryAssisterPlayerId { get; set; }
    }

    private class PenaltyRec
    {
        public int ProjectTeamId { get; init; }
        public Guid? PlayerId { get; init; }
        public int Minutes { get; init; }
        public int TimeSeconds { get; init; }
    }

    public async Task ImportProjectMatchesAsync(ProjectImport pi, FloorballSeasonDto season, Guid refereeId)
    {
        OldProject project = pi.Project;
        int periodSeconds = project.PeriodDurationMinutes * 60;
        int regularPeriods = project.NumberOfPeriods;

        // projectteam id -> side info (new team + default goalie)
        Dictionary<int, SideInfo> sides = [];
        // old team_player id -> new player guid
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
                if (_idMap.Persons.TryGetValue(re.Person.Id, out IdMapStore.PersonMapping? mapping))
                    playerByTeamPlayerId[re.TeamPlayer.Id] = mapping.PlayerId;
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
                    bool ok = await RepairMatchAsync(mi, existingMatchId, home, away, playerByTeamPlayerId, periodSeconds, regularPeriods, prefix);
                    if (ok) Repaired++;
                    else Failed++;
                }
                else
                {
                    bool ok = await ImportSingleMatchAsync(
                        mi, season, refereeId, home, away, playerByTeamPlayerId, periodSeconds, regularPeriods, prefix);
                    if (!ok) Failed++;
                }
            }
            catch (Exception ex)
            {
                Failed++;
                Console.WriteLine($"{prefix} ERROR: {ex.Message}");
                _log.LogError("ImportMatch", new { match.Id }, ex.ToString());
            }
        }
    }

    private async Task<bool> ImportSingleMatchAsync(
        MatchImport mi,
        FloorballSeasonDto season,
        Guid refereeId,
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

        FloorballMatchDto? created = await _api.CreateMatchAsync(
            season.Id, home.TeamId, away.TeamId, refereeId, scheduled, venue);
        if (created == null)
        {
            _log.LogError("CreateMatch", new { match.Id, Home = home.OldTeam.Name, Away = away.OldTeam.Name, scheduled }, "API returned null.");
            return false;
        }

        if (!match.HasResult)
        {
            // Future / unplayed match: leave as Scheduled.
            _idMap.ProcessedMatches[match.Id] = created.Id;
            _idMap.Save();
            ScheduledOnly++;
            Console.WriteLine($"{prefix} {home.OldTeam.Name} - {away.OldTeam.Name}: scheduled only");
            return true;
        }

        (List<GoalRec> goals, List<PenaltyRec> penalties, int ignoredEvents) =
            await BuildEventsAsync(mi, home, away, playerByTeamPlayerId, periodSeconds, regularPeriods);

        // ── Lifecycle ────────────────────────────────────────
        Guid homeGoalie = home.GoaliePlayerId ?? await ResolveUnknownScorerAsync(home) ?? Guid.Empty;
        Guid awayGoalie = away.GoaliePlayerId ?? await ResolveUnknownScorerAsync(away) ?? Guid.Empty;
        if (homeGoalie == Guid.Empty || awayGoalie == Guid.Empty)
        {
            _idMap.ProcessedMatches[match.Id] = created.Id;
            _idMap.Save();
            ScheduledOnly++;
            _log.LogWarning("NoGoalie",
                $"Match JL#{match.Id} left as Scheduled: no roster players found for goalie assignment.");
            Console.WriteLine($"{prefix} {home.OldTeam.Name} - {away.OldTeam.Name}: scheduled only (no goalies)");
            return true;
        }

        await _api.SetGoalieAsync(created.Id, home.TeamId, homeGoalie);
        await _api.SetGoalieAsync(created.Id, away.TeamId, awayGoalie);

        bool started = await _api.StartMatchAsync(created.Id);
        if (!started)
        {
            _idMap.ProcessedMatches[match.Id] = created.Id;
            _idMap.Save();
            _log.LogError("StartMatch", new { match.Id, NewMatchId = created.Id }, "Could not start match; left as Scheduled.");
            return false;
        }

        (int goalsRecorded, int penaltiesRecorded) = await RecordEventsAsync(
            created.Id, match, home, away, goals, penalties, periodSeconds, regularPeriods);

        bool completed = await _api.CompleteMatchAsync(created.Id);
        if (!completed)
            _log.LogError("CompleteMatch", new { match.Id, NewMatchId = created.Id }, "API call failed.");

        _idMap.ProcessedMatches[match.Id] = created.Id;
        _idMap.Save();
        Succeeded++;

        string eventNote = ignoredEvents > 0 ? $", {ignoredEvents} events ignored" : "";
        Console.WriteLine(
            $"{prefix} {home.OldTeam.Name} - {away.OldTeam.Name} " +
            $"{match.Team1Result}-{match.Team2Result}: {goalsRecorded} goals, {penaltiesRecorded} penalties{eventNote}");
        return true;
    }

    /// <summary>
    /// Re-imports the events of an already imported match: reopens it, deletes existing goal
    /// and penalty events and replays them from the old data (with current player mappings).
    /// </summary>
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

        FloorballMatchDto? dto = await _api.GetMatchByIdAsync(newMatchId);
        if (dto == null)
        {
            _log.LogError("RepairMatch", new { match.Id, newMatchId }, "Match not found in new system.");
            return false;
        }

        if (!match.HasResult)
        {
            // Old match has no result; the new match should just stay Scheduled.
            Console.WriteLine($"{prefix} {home.OldTeam.Name} - {away.OldTeam.Name}: repair skipped (no result, stays scheduled)");
            return true;
        }

        if (dto.Status == FloorballMatchStatus.Completed)
        {
            if (!await _api.ReopenMatchAsync(newMatchId))
            {
                _log.LogError("RepairMatch", new { match.Id, newMatchId }, "Reopen failed.");
                return false;
            }
        }
        else if (dto.Status == FloorballMatchStatus.Scheduled)
        {
            // The match was created but never started (e.g. an earlier run failed at goalie
            // assignment); bring it to InProgress so events can be recorded.
            Guid homeGoalie = home.GoaliePlayerId ?? await ResolveUnknownScorerAsync(home) ?? Guid.Empty;
            Guid awayGoalie = away.GoaliePlayerId ?? await ResolveUnknownScorerAsync(away) ?? Guid.Empty;
            if (homeGoalie == Guid.Empty || awayGoalie == Guid.Empty)
            {
                _log.LogError("RepairMatch", new { match.Id, newMatchId }, "No goalies available; cannot start match.");
                return false;
            }
            await _api.SetGoalieAsync(newMatchId, home.TeamId, homeGoalie);
            await _api.SetGoalieAsync(newMatchId, away.TeamId, awayGoalie);
            if (!await _api.StartMatchAsync(newMatchId))
            {
                _log.LogError("RepairMatch", new { match.Id, newMatchId }, "StartMatch failed during repair.");
                return false;
            }
        }

        foreach (FloorballGoalEventDto goalEvent in dto.GoalEvents)
            await _api.DeleteGoalEventAsync(newMatchId, goalEvent.Id);
        foreach (FloorballPenaltyEventDto penaltyEvent in dto.PenaltyEvents)
            await _api.DeletePenaltyEventAsync(newMatchId, penaltyEvent.Id);

        (List<GoalRec> goals, List<PenaltyRec> penalties, _) =
            await BuildEventsAsync(mi, home, away, playerByTeamPlayerId, periodSeconds, regularPeriods);

        (int goalsRecorded, int penaltiesRecorded) = await RecordEventsAsync(
            newMatchId, match, home, away, goals, penalties, periodSeconds, regularPeriods);

        bool completed = await _api.CompleteMatchAsync(newMatchId);
        if (!completed)
            _log.LogError("RepairMatch", new { match.Id, newMatchId }, "Complete failed after repair.");

        Console.WriteLine(
            $"{prefix} {home.OldTeam.Name} - {away.OldTeam.Name} " +
            $"{match.Team1Result}-{match.Team2Result}: REPAIRED, {goalsRecorded} goals, {penaltiesRecorded} penalties");
        return true;
    }

    // ── Event building ────────────────────────────────────────

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
                for (int i = 0; i < ev.Count; i++)
                {
                    goals.Add(new GoalRec
                    {
                        ProjectTeamId = ev.ProjectTeamId,
                        ScorerPlayerId = playerId,
                        TimeSeconds = timeSeconds,
                    });
                }
            }
            else if (JoomleagueDatabase.AssistEventTypes.Contains(ev.EventTypeId))
            {
                assists.Add((ev.ProjectTeamId, playerId, timeSeconds));
            }
            else if (ev.EventTypeId == JoomleagueDatabase.EventPenalty)
            {
                penalties.Add(new PenaltyRec
                {
                    ProjectTeamId = ev.ProjectTeamId,
                    PlayerId = playerId,
                    Minutes = Math.Clamp(ev.Count, 2, 20),
                    TimeSeconds = timeSeconds,
                });
            }
        }

        // Pair assists to goals of the same team at the same clock time.
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

        // Score reconciliation: fill in unattributed goals so the final score matches the old result.
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

        // Goals whose scorer is still unresolved get attributed to the team's unknown player too;
        // otherwise the score would come out wrong since scores derive from goal events.
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

    // ── Event recording ───────────────────────────────────────

    private async Task<(int GoalsRecorded, int PenaltiesRecorded)> RecordEventsAsync(
        Guid newMatchId,
        OldMatch match,
        SideInfo home,
        SideInfo away,
        List<GoalRec> goals,
        List<PenaltyRec> penalties,
        int periodSeconds,
        int regularPeriods)
    {
        int goalsRecorded = 0, penaltiesRecorded = 0;

        for (int period = 1; period <= regularPeriods; period++)
        {
            await _api.StartPeriodAsync(newMatchId, period);

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
                Guid teamId = goal.ProjectTeamId == match.ProjectTeam1Id ? home.TeamId : away.TeamId;
                bool ok = await _api.RecordGoalAsync(
                    newMatchId, teamId, goal.ScorerPlayerId!.Value,
                    goal.AssisterPlayerId, goal.SecondaryAssisterPlayerId,
                    period, goal.TimeSeconds);
                if (ok) goalsRecorded++;
                else _log.LogError("RecordGoal", new { match.Id, NewMatchId = newMatchId, teamId, goal.ScorerPlayerId, period, goal.TimeSeconds }, "API call failed.");
            }

            foreach (PenaltyRec pen in periodPenalties)
            {
                if (pen.PlayerId == null)
                {
                    _log.LogWarning("PenaltySkipped", $"Match JL#{match.Id}: penalty at {pen.TimeSeconds}s skipped (player not mapped).");
                    continue;
                }
                Guid teamId = pen.ProjectTeamId == match.ProjectTeam1Id ? home.TeamId : away.TeamId;
                bool ok = await _api.RecordPenaltyAsync(
                    newMatchId, teamId, pen.PlayerId.Value, pen.Minutes, period, pen.TimeSeconds);
                if (ok) penaltiesRecorded++;
                else _log.LogError("RecordPenalty", new { match.Id, NewMatchId = newMatchId, teamId, pen.PlayerId, period, pen.TimeSeconds }, "API call failed.");
            }

            await _api.EndPeriodAsync(newMatchId, period);
        }

        return (goalsRecorded, penaltiesRecorded);
    }

    private async Task<Guid?> ResolveUnknownScorerAsync(SideInfo side)
    {
        if (!_fillUnknownGoals)
            return null;
        return await _entities.GetOrCreateUnknownPlayerAsync(side.OldTeam, side.TeamId);
    }

    private static int PeriodOf(int timeSeconds, int periodSeconds, int regularPeriods)
    {
        if (periodSeconds <= 0) return 1;
        int period = timeSeconds / periodSeconds + 1;
        return Math.Clamp(period, 1, regularPeriods);
    }

}

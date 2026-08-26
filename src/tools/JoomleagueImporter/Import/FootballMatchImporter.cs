using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Seasons.DTOs;
using Domain.Enums.Football;
using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

/// <summary>
/// Imports the matches of one old football project into the corresponding new season:
/// lineup (hobby 5v5 with one GK), start, halves, goals/assists/cards, complete.
/// </summary>
public class FootballMatchImporter
{
    private readonly FootballApiClient _api;
    private readonly IdMapStore _idMap;
    private readonly ImportLogger _log;
    private readonly FootballEntityImporter _entities;
    private readonly JoomleagueDatabase _db;
    private readonly bool _fillUnknownGoals;
    private readonly HashSet<int> _repairMatchIds;
    private readonly bool _repairAll;

    public int Succeeded { get; private set; }
    public int ScheduledOnly { get; private set; }
    public int Skipped { get; private set; }
    public int Failed { get; private set; }
    public int Repaired { get; private set; }

    public FootballMatchImporter(
        FootballApiClient api,
        IdMapStore idMap,
        ImportLogger log,
        FootballEntityImporter entities,
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

    private class LineupCandidate
    {
        public required Guid PlayerId { get; init; }
        public required FootballPosition Position { get; init; }
    }

    private class SideInfo
    {
        public required OldTeam OldTeam { get; init; }
        public required Guid TeamId { get; init; }
        public List<LineupCandidate> Roster { get; } = [];
    }

    private class GoalRec
    {
        public int ProjectTeamId { get; init; }
        public Guid? ScorerPlayerId { get; set; }
        public int TimeSeconds { get; init; }
        public Guid? AssisterPlayerId { get; set; }
    }

    private class CardRec
    {
        public int ProjectTeamId { get; init; }
        public Guid? PlayerId { get; init; }
        public FootballCardType CardType { get; init; }
        public int TimeSeconds { get; init; }
    }

    public async Task ImportProjectMatchesAsync(ProjectImport pi, FootballSeasonDto season, Guid refereeId)
    {
        int periodSeconds = Math.Max(1, season.MatchRules.HalfDurationMinutes) * 60;
        int regularPeriods = Math.Max(1, season.MatchRules.NumberOfHalves);
        int playersOnField = Math.Max(1, season.MatchRules.PlayersOnField);

        Dictionary<int, SideInfo> sides = [];
        Dictionary<int, Guid> playerByTeamPlayerId = [];

        foreach (ProjectTeamImport pti in pi.Teams.Values)
        {
            if (!_idMap.Teams.TryGetValue(pti.Team.Id, out Guid teamId))
                continue;

            SideInfo side = new() { OldTeam = pti.Team, TeamId = teamId };

            foreach (RosterEntry re in pti.Roster)
            {
                if (!_idMap.Persons.TryGetValue(re.Person.Id, out IdMapStore.PersonMapping? mapping))
                    continue;
                playerByTeamPlayerId[re.TeamPlayer.Id] = mapping.PlayerId;
                if (side.Roster.All(c => c.PlayerId != mapping.PlayerId))
                {
                    side.Roster.Add(new LineupCandidate
                    {
                        PlayerId = mapping.PlayerId,
                        Position = re.FootballPosition == FootballPosition.None
                            ? FootballPosition.Forward
                            : re.FootballPosition,
                    });
                }
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
                        mi, existingMatchId, home, away, playerByTeamPlayerId,
                        periodSeconds, regularPeriods, playersOnField, prefix);
                    if (ok) Repaired++;
                    else Failed++;
                }
                else
                {
                    bool ok = await ImportSingleMatchAsync(
                        mi, season, refereeId, home, away, playerByTeamPlayerId,
                        periodSeconds, regularPeriods, playersOnField, prefix);
                    if (!ok) Failed++;
                }
            }
            catch (Exception ex)
            {
                Failed++;
                Console.WriteLine($"{prefix} ERROR: {ex.Message}");
                _log.LogError("ImportFootballMatch", new { match.Id }, ex.ToString());
            }
        }
    }

    private async Task<bool> ImportSingleMatchAsync(
        MatchImport mi,
        FootballSeasonDto season,
        Guid refereeId,
        SideInfo home,
        SideInfo away,
        Dictionary<int, Guid> playerByTeamPlayerId,
        int periodSeconds,
        int regularPeriods,
        int playersOnField,
        string prefix)
    {
        OldMatch match = mi.Match;
        DateTime scheduled = match.MatchDate ?? new DateTime(2000, 1, 1, 18, 0, 0);
        string? venue = match.PlaygroundId.HasValue ? _db.Playgrounds.GetValueOrDefault(match.PlaygroundId.Value) : null;

        FootballMatchDto? created = await _api.CreateMatchAsync(
            season.Id, home.TeamId, away.TeamId, refereeId, scheduled, venue);
        if (created == null)
        {
            _log.LogError("CreateFootballMatch", new { match.Id, Home = home.OldTeam.Name, Away = away.OldTeam.Name, scheduled }, "API returned null.");
            return false;
        }

        if (!match.HasResult)
        {
            _idMap.ProcessedMatches[match.Id] = created.Id;
            _idMap.Save();
            ScheduledOnly++;
            Console.WriteLine($"{prefix} {home.OldTeam.Name} - {away.OldTeam.Name}: scheduled only");
            return true;
        }

        (List<GoalRec> goals, List<CardRec> cards, int ignoredEvents) =
            await BuildEventsAsync(mi, home, away, playerByTeamPlayerId, periodSeconds, regularPeriods);

        List<(Guid PlayerId, FootballPosition Position, bool IsOnField)>? homeLineup =
            await BuildLineupAsync(home, playersOnField);
        List<(Guid PlayerId, FootballPosition Position, bool IsOnField)>? awayLineup =
            await BuildLineupAsync(away, playersOnField);
        if (homeLineup == null || awayLineup == null)
        {
            _idMap.ProcessedMatches[match.Id] = created.Id;
            _idMap.Save();
            ScheduledOnly++;
            _log.LogWarning("NoLineup",
                $"Match JL#{match.Id} left as Scheduled: could not fill a {playersOnField}-player lineup.");
            Console.WriteLine($"{prefix} {home.OldTeam.Name} - {away.OldTeam.Name}: scheduled only (lineup too small)");
            return true;
        }

        await _api.SetLineupAsync(created.Id, home.TeamId, homeLineup);
        await _api.SetLineupAsync(created.Id, away.TeamId, awayLineup);

        bool started = await _api.StartMatchAsync(created.Id);
        if (!started)
        {
            _idMap.ProcessedMatches[match.Id] = created.Id;
            _idMap.Save();
            _log.LogError("StartFootballMatch", new { match.Id, NewMatchId = created.Id }, "Could not start match; left as Scheduled.");
            return false;
        }

        (int goalsRecorded, int cardsRecorded) = await RecordEventsAsync(
            created.Id, match, home, away, goals, cards, periodSeconds, regularPeriods);

        bool completed = await _api.CompleteMatchAsync(created.Id);
        if (!completed)
            _log.LogError("CompleteFootballMatch", new { match.Id, NewMatchId = created.Id }, "API call failed.");

        _idMap.ProcessedMatches[match.Id] = created.Id;
        _idMap.Save();
        Succeeded++;

        string eventNote = ignoredEvents > 0 ? $", {ignoredEvents} events ignored" : "";
        Console.WriteLine(
            $"{prefix} {home.OldTeam.Name} - {away.OldTeam.Name} " +
            $"{match.Team1Result}-{match.Team2Result}: {goalsRecorded} goals, {cardsRecorded} cards{eventNote}");
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
        int playersOnField,
        string prefix)
    {
        OldMatch match = mi.Match;

        FootballMatchDto? dto = await _api.GetMatchByIdAsync(newMatchId);
        if (dto == null)
        {
            _log.LogError("RepairFootballMatch", new { match.Id, newMatchId }, "Match not found in new system.");
            return false;
        }

        if (!match.HasResult)
        {
            Console.WriteLine($"{prefix} {home.OldTeam.Name} - {away.OldTeam.Name}: repair skipped (no result, stays scheduled)");
            return true;
        }

        if (dto.Status == FootballMatchStatus.Completed)
        {
            if (!await _api.ReopenMatchAsync(newMatchId))
            {
                _log.LogError("RepairFootballMatch", new { match.Id, newMatchId }, "Reopen failed.");
                return false;
            }
        }
        else if (dto.Status == FootballMatchStatus.Scheduled)
        {
            List<(Guid PlayerId, FootballPosition Position, bool IsOnField)>? homeLineup =
                await BuildLineupAsync(home, playersOnField);
            List<(Guid PlayerId, FootballPosition Position, bool IsOnField)>? awayLineup =
                await BuildLineupAsync(away, playersOnField);
            if (homeLineup == null || awayLineup == null)
            {
                _log.LogError("RepairFootballMatch", new { match.Id, newMatchId }, "Could not build lineup; cannot start match.");
                return false;
            }
            await _api.SetLineupAsync(newMatchId, home.TeamId, homeLineup);
            await _api.SetLineupAsync(newMatchId, away.TeamId, awayLineup);
            if (!await _api.StartMatchAsync(newMatchId))
            {
                _log.LogError("RepairFootballMatch", new { match.Id, newMatchId }, "StartMatch failed during repair.");
                return false;
            }
        }

        foreach (FootballGoalEventDto goalEvent in dto.GoalEvents)
            await _api.DeleteGoalEventAsync(newMatchId, goalEvent.Id);
        foreach (FootballCardEventDto cardEvent in dto.CardEvents)
            await _api.DeleteCardEventAsync(newMatchId, cardEvent.Id);

        (List<GoalRec> goals, List<CardRec> cards, _) =
            await BuildEventsAsync(mi, home, away, playerByTeamPlayerId, periodSeconds, regularPeriods);

        (int goalsRecorded, int cardsRecorded) = await RecordEventsAsync(
            newMatchId, match, home, away, goals, cards, periodSeconds, regularPeriods);

        bool completed = await _api.CompleteMatchAsync(newMatchId);
        if (!completed)
            _log.LogError("RepairFootballMatch", new { match.Id, newMatchId }, "Complete failed after repair.");

        Console.WriteLine(
            $"{prefix} {home.OldTeam.Name} - {away.OldTeam.Name} " +
            $"{match.Team1Result}-{match.Team2Result}: REPAIRED, {goalsRecorded} goals, {cardsRecorded} cards");
        return true;
    }

    private async Task<List<(Guid PlayerId, FootballPosition Position, bool IsOnField)>?> BuildLineupAsync(
        SideInfo side,
        int playersOnField)
    {
        if (side.Roster.Count < playersOnField)
        {
            for (int unknownCount = 1; unknownCount <= playersOnField && side.Roster.Count < playersOnField; unknownCount++)
            {
                List<Guid> pads = await _entities.EnsureUnknownPlayersAsync(side.OldTeam, side.TeamId, unknownCount);
                foreach (Guid padId in pads)
                {
                    if (side.Roster.Any(c => c.PlayerId == padId))
                        continue;
                    side.Roster.Add(new LineupCandidate { PlayerId = padId, Position = FootballPosition.Forward });
                }
            }
        }

        if (side.Roster.Count < playersOnField)
            return null;

        List<(Guid PlayerId, FootballPosition Position, bool IsOnField)> lineup = [];
        HashSet<Guid> used = [];

        LineupCandidate? gk = side.Roster.FirstOrDefault(c => c.Position == FootballPosition.Goalkeeper)
                              ?? side.Roster.FirstOrDefault();
        if (gk == null)
            return null;

        lineup.Add((gk.PlayerId, FootballPosition.Goalkeeper, true));
        used.Add(gk.PlayerId);

        foreach (LineupCandidate candidate in side.Roster)
        {
            if (lineup.Count(p => p.IsOnField) >= playersOnField)
                break;
            if (!used.Add(candidate.PlayerId))
                continue;
            FootballPosition pos = candidate.Position == FootballPosition.Goalkeeper
                ? FootballPosition.Forward
                : candidate.Position;
            lineup.Add((candidate.PlayerId, pos, true));
        }

        if (lineup.Count(p => p.IsOnField) != playersOnField)
            return null;

        foreach (LineupCandidate candidate in side.Roster)
        {
            if (used.Contains(candidate.PlayerId))
                continue;
            lineup.Add((candidate.PlayerId, candidate.Position, false));
            used.Add(candidate.PlayerId);
        }

        return lineup;
    }

    private async Task<(List<GoalRec> Goals, List<CardRec> Cards, int IgnoredEvents)> BuildEventsAsync(
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
        List<CardRec> cards = [];
        int ignoredEvents = 0;

        foreach (OldMatchEvent ev in mi.Events)
        {
            bool isGoal = _db.FootballGoalEventTypeIds.Contains(ev.EventTypeId);
            bool isAssist = _db.FootballAssistEventTypeIds.Contains(ev.EventTypeId);
            bool isYellow = _db.FootballYellowCardEventTypeIds.Contains(ev.EventTypeId);
            bool isRed = _db.FootballRedCardEventTypeIds.Contains(ev.EventTypeId);

            if (ev.ProjectTeamId != match.ProjectTeam1Id && ev.ProjectTeamId != match.ProjectTeam2Id)
            {
                if (isGoal || isAssist || isYellow || isRed)
                    ignoredEvents++;
                continue;
            }

            int timeSeconds = ImportTimeParser.ParseEventTime(ev.EventTime) ?? 0;
            Guid? playerId = playerByTeamPlayerId.TryGetValue(ev.TeamPlayerId, out Guid pid) ? pid : null;

            if (isGoal)
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
            else if (isAssist)
            {
                assists.Add((ev.ProjectTeamId, playerId, timeSeconds));
            }
            else if (isYellow)
            {
                cards.Add(new CardRec
                {
                    ProjectTeamId = ev.ProjectTeamId,
                    PlayerId = playerId,
                    CardType = FootballCardType.Yellow,
                    TimeSeconds = timeSeconds,
                });
            }
            else if (isRed)
            {
                cards.Add(new CardRec
                {
                    ProjectTeamId = ev.ProjectTeamId,
                    PlayerId = playerId,
                    CardType = FootballCardType.DirectRed,
                    TimeSeconds = timeSeconds,
                });
            }
        }

        foreach ((int ptId, Guid? assisterId, int timeSeconds) in assists)
        {
            if (assisterId == null) continue;
            GoalRec? target = goals.FirstOrDefault(g =>
                g.ProjectTeamId == ptId && g.TimeSeconds == timeSeconds &&
                g.AssisterPlayerId == null &&
                g.ScorerPlayerId != assisterId);
            target ??= goals.FirstOrDefault(g =>
                g.ProjectTeamId == ptId && Math.Abs(g.TimeSeconds - timeSeconds) <= 5 &&
                g.AssisterPlayerId == null &&
                g.ScorerPlayerId != assisterId);
            if (target == null) continue;
            target.AssisterPlayerId = assisterId;
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

        return (goals, cards, ignoredEvents);
    }

    private async Task<(int GoalsRecorded, int CardsRecorded)> RecordEventsAsync(
        Guid newMatchId,
        OldMatch match,
        SideInfo home,
        SideInfo away,
        List<GoalRec> goals,
        List<CardRec> cards,
        int periodSeconds,
        int regularPeriods)
    {
        int goalsRecorded = 0, cardsRecorded = 0;

        for (int period = 1; period <= regularPeriods; period++)
        {
            await _api.StartPeriodAsync(newMatchId, period);

            int currentPeriod = period;
            List<GoalRec> periodGoals = goals
                .Where(g => PeriodOf(g.TimeSeconds, periodSeconds, regularPeriods) == currentPeriod)
                .OrderBy(g => g.TimeSeconds)
                .ToList();
            List<CardRec> periodCards = cards
                .Where(c => PeriodOf(c.TimeSeconds, periodSeconds, regularPeriods) == currentPeriod)
                .OrderBy(c => c.TimeSeconds)
                .ToList();

            foreach (GoalRec goal in periodGoals)
            {
                Guid teamId = goal.ProjectTeamId == match.ProjectTeam1Id ? home.TeamId : away.TeamId;
                bool ok = await _api.RecordGoalAsync(
                    newMatchId, teamId, goal.ScorerPlayerId!.Value,
                    goal.AssisterPlayerId,
                    period, goal.TimeSeconds);
                if (ok) goalsRecorded++;
                else _log.LogError("RecordFootballGoal", new { match.Id, NewMatchId = newMatchId, teamId, goal.ScorerPlayerId, period, goal.TimeSeconds }, "API call failed.");
            }

            foreach (CardRec card in periodCards)
            {
                if (card.PlayerId == null)
                {
                    _log.LogWarning("CardSkipped", $"Match JL#{match.Id}: {card.CardType} at {card.TimeSeconds}s skipped (player not mapped).");
                    continue;
                }
                Guid teamId = card.ProjectTeamId == match.ProjectTeam1Id ? home.TeamId : away.TeamId;
                bool ok = await _api.RecordCardAsync(
                    newMatchId, teamId, card.PlayerId.Value, card.CardType, period, card.TimeSeconds);
                if (ok) cardsRecorded++;
                else _log.LogError("RecordFootballCard", new { match.Id, NewMatchId = newMatchId, teamId, card.PlayerId, period, card.TimeSeconds }, "API call failed.");
            }

            await _api.EndPeriodAsync(newMatchId, period);
        }

        return (goalsRecorded, cardsRecorded);
    }

    private async Task<Guid?> ResolveUnknownScorerAsync(SideInfo side)
    {
        if (!_fillUnknownGoals)
            return null;
        Guid? unknown = await _entities.GetOrCreateUnknownPlayerAsync(side.OldTeam, side.TeamId);
        if (unknown != null && side.Roster.All(c => c.PlayerId != unknown.Value))
            side.Roster.Add(new LineupCandidate { PlayerId = unknown.Value, Position = FootballPosition.Forward });
        return unknown;
    }

    private static int PeriodOf(int timeSeconds, int periodSeconds, int regularPeriods)
    {
        if (periodSeconds <= 0) return 1;
        int period = timeSeconds / periodSeconds + 1;
        return Math.Clamp(period, 1, regularPeriods);
    }
}

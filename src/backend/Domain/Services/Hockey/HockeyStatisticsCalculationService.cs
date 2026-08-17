using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Entities.Hockey.Statistics;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Statistics;
using Domain.Enums.Hockey.Teams;
using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Services.Hockey;

/// <summary>
/// In-memory statistics calculation from match events into match and competition stats entities.
/// Does not persist; Application/Infrastructure owns storage.
/// </summary>
public class HockeyStatisticsCalculationService
{
    public HockeyMatchTeamStatistics BuildMatchTeamStatistics(HockeyMatch match, HockeyMatchTeam matchTeam)
    {
        ArgumentNullException.ThrowIfNull(match);
        ArgumentNullException.ThrowIfNull(matchTeam);
        if (matchTeam.MatchId != match.Id)
            throw new InvalidOperationException("Match team does not belong to the match.");

        Guid matchTeamId = matchTeam.Id;
        IEnumerable<HockeyMatchEvent> events = match.Events;

        List<HockeyGoal> goalsFor = events.OfType<HockeyGoal>()
            .Where(g => g.ScoringMatchTeamId == matchTeamId)
            .ToList();
        List<HockeyGoal> goalsAgainst = events.OfType<HockeyGoal>()
            .Where(g => g.ScoringMatchTeamId != matchTeamId)
            .ToList();

        List<HockeyShot> shots = events.OfType<HockeyShot>()
            .Where(s => s.ShootingMatchTeamId == matchTeamId)
            .ToList();
        List<HockeyShot> shotsAgainst = events.OfType<HockeyShot>()
            .Where(s => s.ShootingMatchTeamId != matchTeamId)
            .ToList();

        int shotsOnGoal = shots.Count(s => s.CountsAsShotOnGoal);
        int missed = shots.Count(s => s.ShotResult == HockeyShotResult.Missed);
        int blocked = shots.Count(s => s.ShotResult == HockeyShotResult.Blocked);
        int saves = shotsAgainst.Count(s => s.ShotResult == HockeyShotResult.Saved);
        int shotsAgainstOnGoal = shotsAgainst.Count(s =>
            s.ShotResult is HockeyShotResult.Saved or HockeyShotResult.Goal);

        List<HockeyFaceoff> faceoffsWon = events.OfType<HockeyFaceoff>()
            .Where(f => f.WinningMatchTeamId == matchTeamId)
            .ToList();
        int faceoffAttempts = events.OfType<HockeyFaceoff>()
            .Count(f => f.WinningMatchTeamId == matchTeamId || f.LosingMatchTeamId == matchTeamId);

        List<HockeyPenalty> penalties = events.OfType<HockeyPenalty>()
            .Where(p => p.PenaltyMatchTeamId == matchTeamId)
            .ToList();

        int ppGoals = goalsFor.Count(IsPowerPlayGoal);
        int ppOpportunities = events.OfType<HockeyPenalty>().Count(p => p.PenaltyMatchTeamId != matchTeamId);
        int pkOpportunities = penalties.Count;
        int pkSuccesses = Math.Max(0, pkOpportunities - goalsAgainst.Count(IsPowerPlayGoal));

        HockeyMatchTeamStatistics stats = new(match.Id, matchTeamId, matchTeam.TeamId);
        stats.UpdateScoring(goalsFor.Count, goalsAgainst.Count);
        stats.UpdateShooting(shotsOnGoal, shots.Count, missed, blocked);
        stats.UpdateGoaltending(saves, shotsAgainstOnGoal);
        stats.UpdateFaceoffs(faceoffsWon.Count, faceoffAttempts);
        stats.UpdateSpecialTeams(ppOpportunities, ppGoals, pkOpportunities, pkSuccesses);
        stats.UpdateDisciplineAndMisc(
            penalties.Count,
            penalties.Sum(p => p.PenaltyMinutes),
            hits: 0,
            blockedShots: blocked,
            takeaways: 0,
            giveaways: 0);
        return stats;
    }

    public IReadOnlyList<HockeyMatchPlayerStatistics> BuildMatchPlayerStatistics(
        HockeyMatch match,
        HockeyMatchTeam matchTeam)
    {
        ArgumentNullException.ThrowIfNull(match);
        ArgumentNullException.ThrowIfNull(matchTeam);
        if (matchTeam.MatchId != match.Id)
            throw new InvalidOperationException("Match team does not belong to the match.");

        HockeyMatchPlayerSelection selection = matchTeam.PlayerSelection
            ?? throw new InvalidOperationException("Match team has no player selection.");

        Dictionary<Guid, int> shifts = CountShifts(matchTeam);
        List<HockeyMatchPlayerStatistics> results = new();

        foreach (HockeyMatchActivePlayer active in selection.ActivePlayers.Where(p => p.IsActive))
        {
            if (active.IsGoalie || active.Position == HockeyPosition.Goalie)
                continue;

            Guid playerId = ResolvePlayerId(active);
            Guid activeId = active.Id;

            int goals = match.Events.OfType<HockeyGoal>()
                .Count(g => g.ScorerActivePlayerId == activeId);
            int assists = match.Events.OfType<HockeyGoal>()
                .Count(g =>
                    g.PrimaryAssistActivePlayerId == activeId
                    || g.SecondaryAssistActivePlayerId == activeId);
            int pim = match.Events.OfType<HockeyPenalty>()
                .Where(p => p.PenalizedActivePlayerId == activeId)
                .Sum(p => p.PenaltyMinutes);
            int sog = match.Events.OfType<HockeyShot>()
                .Count(s => s.ShooterActivePlayerId == activeId && s.CountsAsShotOnGoal);
            int attempts = match.Events.OfType<HockeyShot>()
                .Count(s => s.ShooterActivePlayerId == activeId);
            int foWins = match.Events.OfType<HockeyFaceoff>()
                .Count(f => f.WinningActivePlayerId == activeId);
            int foAttempts = match.Events.OfType<HockeyFaceoff>()
                .Count(f => f.WinningActivePlayerId == activeId || f.LosingActivePlayerId == activeId);

            HockeyMatchPlayerStatistics stats = new(
                match.Id,
                matchTeam.Id,
                activeId,
                active.TeamPlayerId,
                playerId,
                matchTeam.TeamId);
            stats.UpdateScoring(goals, assists, pim, plusMinusRating: 0);
            stats.UpdateShooting(sog, attempts);
            stats.UpdateFaceoffs(foWins, foAttempts);
            stats.UpdateMisc(
                hits: 0,
                blockedShots: 0,
                takeaways: 0,
                giveaways: 0,
                timeOnIceSeconds: 0,
                shifts: shifts.GetValueOrDefault(activeId));
            results.Add(stats);
        }

        return results;
    }

    public IReadOnlyList<HockeyGoalieMatchStatistics> BuildGoalieMatchStatistics(
        HockeyMatch match,
        HockeyMatchTeam matchTeam)
    {
        ArgumentNullException.ThrowIfNull(match);
        ArgumentNullException.ThrowIfNull(matchTeam);
        if (matchTeam.MatchId != match.Id)
            throw new InvalidOperationException("Match team does not belong to the match.");

        HockeyMatchPlayerSelection selection = matchTeam.PlayerSelection
            ?? throw new InvalidOperationException("Match team has no player selection.");

        List<HockeyMatchActivePlayer> goalies = selection.ActivePlayers
            .Where(p => p.IsActive && (p.IsGoalie || p.IsEmergencyGoalie || p.Position == HockeyPosition.Goalie))
            .ToList();

        List<HockeyGoalieMatchStatistics> results = new();
        foreach (HockeyMatchActivePlayer goalie in goalies)
        {
            Guid goalieId = goalie.Id;
            List<HockeyShot> against = match.Events.OfType<HockeyShot>()
                .Where(s => s.GoalieActivePlayerId == goalieId)
                .ToList();
            int saves = against.Count(s => s.ShotResult == HockeyShotResult.Saved);
            int goalsAgainst = against.Count(s => s.ShotResult == HockeyShotResult.Goal)
                + match.Events.OfType<HockeyGoal>().Count(g => g.GoalieActivePlayerId == goalieId);
            int shotsAgainst = saves + against.Count(s => s.ShotResult == HockeyShotResult.Goal);
            if (shotsAgainst < saves + goalsAgainst)
                shotsAgainst = saves + goalsAgainst;

            bool wasStarter = goalie.IsStartingPlayer
                || matchTeam.ActiveGoalieMatchPlayerId == goalieId;

            HockeyGoalieDecision decision = ResolveGoalieDecision(match, matchTeam, goalieId, wasStarter);
            HockeyGoalieMatchStatistics stats = new(
                match.Id,
                matchTeam.Id,
                goalieId,
                goalie.TeamPlayerId,
                ResolvePlayerId(goalie),
                matchTeam.TeamId,
                wasStarter,
                decision);

            int minutes = EstimateGoalieMinutes(match, goalieId);
            int shutouts = goalsAgainst == 0 && shotsAgainst > 0 ? 1 : 0;
            stats.UpdateGoaltending(saves, shotsAgainst, goalsAgainst, minutes, shutouts);

            foreach (IGrouping<int, HockeyShot> periodGroup in against.GroupBy(s => s.PeriodNumber))
            {
                HockeyGoaliePeriodStatistics period = stats.AddPeriodStatistics(
                    periodGroup.Key,
                    ResolvePeriodType(periodGroup.Key, match));
                int periodSaves = periodGroup.Count(s => s.ShotResult == HockeyShotResult.Saved);
                int periodGa = periodGroup.Count(s => s.ShotResult == HockeyShotResult.Goal);
                period.Update(
                    timeOnIceSeconds: 0,
                    shotsAgainst: periodSaves + periodGa,
                    saves: periodSaves,
                    goalsAgainst: periodGa);
            }

            results.Add(stats);
        }

        return results;
    }

    public HockeyPlayerCompetitionStatistics AggregatePlayerCompetitionStatistics(
        Guid playerId,
        Guid teamId,
        Guid teamPlayerId,
        Guid competitionId,
        HockeyStatisticsScope scope,
        IEnumerable<HockeyMatchPlayerStatistics> matchStatistics,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null)
    {
        ArgumentNullException.ThrowIfNull(matchStatistics);
        List<HockeyMatchPlayerStatistics> rows = matchStatistics
            .Where(s => s.PlayerId == playerId && s.TeamId == teamId)
            .ToList();

        HockeyPlayerCompetitionStatistics aggregate = new(
            playerId,
            teamId,
            teamPlayerId,
            competitionId,
            scope,
            competitionDivisionId,
            tournamentGroupId,
            playoffSeriesId);

        aggregate.UpdateTotals(
            gamesPlayed: rows.Sum(r => r.GamesPlayed),
            goals: rows.Sum(r => r.Goals),
            assists: rows.Sum(r => r.Assists),
            penaltyMinutes: rows.Sum(r => r.PenaltyMinutes),
            plusMinusRating: rows.Sum(r => r.PlusMinusRating),
            shotsOnGoal: rows.Sum(r => r.ShotsOnGoal),
            shotAttempts: rows.Sum(r => r.ShotAttempts),
            faceoffWins: rows.Sum(r => r.FaceoffWins),
            faceoffAttempts: rows.Sum(r => r.FaceoffAttempts),
            hits: rows.Sum(r => r.Hits),
            blockedShots: rows.Sum(r => r.BlockedShots),
            takeaways: rows.Sum(r => r.Takeaways),
            giveaways: rows.Sum(r => r.Giveaways),
            timeOnIceSeconds: rows.Sum(r => r.TimeOnIceSeconds),
            shifts: rows.Sum(r => r.Shifts));
        return aggregate;
    }

    public HockeyGoalieCompetitionStatistics AggregateGoalieCompetitionStatistics(
        Guid playerId,
        Guid teamId,
        Guid teamPlayerId,
        Guid competitionId,
        HockeyStatisticsScope scope,
        IEnumerable<HockeyGoalieMatchStatistics> matchStatistics,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null)
    {
        ArgumentNullException.ThrowIfNull(matchStatistics);
        List<HockeyGoalieMatchStatistics> rows = matchStatistics
            .Where(s => s.PlayerId == playerId && s.TeamId == teamId)
            .ToList();

        HockeyGoalieCompetitionStatistics aggregate = new(
            playerId,
            teamId,
            teamPlayerId,
            competitionId,
            scope,
            competitionDivisionId,
            tournamentGroupId,
            playoffSeriesId);

        aggregate.UpdateTotals(
            gamesPlayed: rows.Sum(r => r.GamesPlayed),
            gamesStarted: rows.Sum(r => r.GamesStarted),
            wins: rows.Sum(r => r.Wins),
            losses: rows.Sum(r => r.Losses),
            overtimeLosses: rows.Sum(r => r.OvertimeLosses),
            shootoutLosses: rows.Sum(r => r.ShootoutLosses),
            noDecisions: rows.Sum(r => r.NoDecisions),
            saves: rows.Sum(r => r.Saves),
            shotsAgainst: rows.Sum(r => r.ShotsAgainst),
            goalsAgainst: rows.Sum(r => r.GoalsAgainst),
            shutouts: rows.Sum(r => r.Shutouts),
            minutesPlayed: rows.Sum(r => r.MinutesPlayed));
        return aggregate;
    }

    public HockeyTeamCompetitionStatistics AggregateTeamCompetitionStatistics(
        Guid teamId,
        Guid competitionId,
        HockeyStatisticsScope scope,
        IEnumerable<HockeyMatch> matches,
        IEnumerable<HockeyMatchTeamStatistics> matchTeamStatistics,
        HockeyStandingRules standingRules,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null)
    {
        ArgumentNullException.ThrowIfNull(matches);
        ArgumentNullException.ThrowIfNull(matchTeamStatistics);
        ArgumentNullException.ThrowIfNull(standingRules);

        List<HockeyMatch> teamMatches = matches
            .Where(m => m.MatchTeams.Any(t => t.TeamId == teamId))
            .ToList();

        int regulationWins = 0, overtimeWins = 0, shootoutWins = 0;
        int regulationLosses = 0, overtimeLosses = 0, shootoutLosses = 0, ties = 0;
        int homeWins = 0, homeLosses = 0, awayWins = 0, awayLosses = 0;

        foreach (HockeyMatch match in teamMatches)
        {
            HockeyMatchTeam? side = match.MatchTeams.FirstOrDefault(t => t.TeamId == teamId);
            if (side is null || match.ResultType is null)
                continue;

            ApplyMatchResult(
                match.ResultType.Value,
                side.TeamSlot,
                ref regulationWins,
                ref overtimeWins,
                ref shootoutWins,
                ref regulationLosses,
                ref overtimeLosses,
                ref shootoutLosses,
                ref ties,
                ref homeWins,
                ref homeLosses,
                ref awayWins,
                ref awayLosses);
        }

        HashSet<Guid> matchTeamIds = teamMatches
            .SelectMany(m => m.MatchTeams)
            .Where(t => t.TeamId == teamId)
            .Select(t => t.Id)
            .ToHashSet();

        List<HockeyMatchTeamStatistics> rows = matchTeamStatistics
            .Where(s => s.TeamId == teamId && matchTeamIds.Contains(s.MatchTeamId))
            .ToList();

        HockeyTeamCompetitionStatistics aggregate = new(
            teamId,
            competitionId,
            scope,
            competitionDivisionId,
            tournamentGroupId,
            playoffSeriesId);

        aggregate.UpdateRecord(
            gamesPlayed: teamMatches.Count(m => m.ResultType is not null),
            regulationWins,
            overtimeWins,
            shootoutWins,
            regulationLosses,
            overtimeLosses,
            shootoutLosses,
            ties,
            homeWins,
            homeLosses,
            awayWins,
            awayLosses);

        aggregate.UpdateScoringAndSpecialTeams(
            goalsFor: rows.Sum(r => r.GoalsFor),
            goalsAgainst: rows.Sum(r => r.GoalsAgainst),
            shotsFor: rows.Sum(r => r.ShotsOnGoal),
            shotsAgainst: rows.Sum(r => r.ShotsAgainst),
            powerPlayGoals: rows.Sum(r => r.PowerPlayGoals),
            powerPlayOpportunities: rows.Sum(r => r.PowerPlayOpportunities),
            penaltyKillOpportunities: rows.Sum(r => r.PenaltyKillOpportunities),
            penaltyKillSuccesses: rows.Sum(r => r.PenaltyKillSuccesses),
            penaltyMinutes: rows.Sum(r => r.PenaltyMinutes),
            faceoffWins: rows.Sum(r => r.FaceoffWins),
            faceoffAttempts: rows.Sum(r => r.FaceoffAttempts));

        aggregate.RecalculateStandingsMetrics(standingRules);
        return aggregate;
    }

    private static bool IsPowerPlayGoal(HockeyGoal goal) =>
        goal.GoalStrength is HockeyGoalStrength.PowerPlayOneMan or HockeyGoalStrength.PowerPlayTwoMan;

    private static Guid ResolvePlayerId(HockeyMatchActivePlayer active)
    {
        if (active.TeamPlayer is not null)
            return active.TeamPlayer.PlayerId;
        throw new InvalidOperationException(
            $"Active player {active.Id} requires TeamPlayer navigation to resolve PlayerId for statistics.");
    }

    private static Dictionary<Guid, int> CountShifts(HockeyMatchTeam matchTeam)
    {
        Dictionary<Guid, int> shifts = new();
        if (matchTeam.OnIceState is null)
            return shifts;

        foreach (HockeyOnIceChange change in matchTeam.OnIceState.ChangeLog)
        {
            if (change.IncomingActivePlayerId is Guid incoming)
                shifts[incoming] = shifts.GetValueOrDefault(incoming) + 1;
        }

        return shifts;
    }

    private static HockeyGoalieDecision ResolveGoalieDecision(
        HockeyMatch match,
        HockeyMatchTeam matchTeam,
        Guid goalieActivePlayerId,
        bool wasStarter)
    {
        if (match.ResultType is null || !wasStarter)
            return HockeyGoalieDecision.NoDecision;

        bool isHome = matchTeam.TeamSlot == HockeyTeamSlot.Home;
        return match.ResultType.Value switch
        {
            HockeyMatchResultType.HomeWin => isHome ? HockeyGoalieDecision.Win : HockeyGoalieDecision.Loss,
            HockeyMatchResultType.AwayWin => isHome ? HockeyGoalieDecision.Loss : HockeyGoalieDecision.Win,
            HockeyMatchResultType.OvertimeHomeWin => isHome ? HockeyGoalieDecision.Win : HockeyGoalieDecision.OvertimeLoss,
            HockeyMatchResultType.OvertimeAwayWin => isHome ? HockeyGoalieDecision.OvertimeLoss : HockeyGoalieDecision.Win,
            HockeyMatchResultType.ShootoutHomeWin => isHome ? HockeyGoalieDecision.Win : HockeyGoalieDecision.ShootoutLoss,
            HockeyMatchResultType.ShootoutAwayWin => isHome ? HockeyGoalieDecision.ShootoutLoss : HockeyGoalieDecision.Win,
            HockeyMatchResultType.Draw => HockeyGoalieDecision.Tie,
            HockeyMatchResultType.ForfeitHomeWin => isHome ? HockeyGoalieDecision.Win : HockeyGoalieDecision.Loss,
            HockeyMatchResultType.ForfeitAwayWin => isHome ? HockeyGoalieDecision.Loss : HockeyGoalieDecision.Win,
            _ => HockeyGoalieDecision.NoDecision
        };
    }

    private static int EstimateGoalieMinutes(HockeyMatch match, Guid goalieActivePlayerId)
    {
        // Lightweight estimate: full regulation minutes when goalie faced any shot or started.
        bool appeared = match.Events.OfType<HockeyShot>().Any(s => s.GoalieActivePlayerId == goalieActivePlayerId)
            || match.Events.OfType<HockeyGoal>().Any(g => g.GoalieActivePlayerId == goalieActivePlayerId)
            || match.Events.OfType<HockeyGoalieChange>().Any(c =>
                c.IncomingGoalieActivePlayerId == goalieActivePlayerId
                || c.OutgoingGoalieActivePlayerId == goalieActivePlayerId);

        if (!appeared)
            return 0;

        int periodMinutes = match.MatchRules.RegularPeriodLengthMinutes;
        int periods = match.MatchRules.RegularPeriodCount;
        int minutes = periodMinutes * periods;
        if (match.WentToOvertime)
            minutes += match.MatchRules.OvertimeLengthMinutes;
        return minutes;
    }

    private static HockeyPeriodType ResolvePeriodType(int periodNumber, HockeyMatch match)
    {
        if (match.WentToShootout && periodNumber > match.MatchRules.RegularPeriodCount + 1)
            return HockeyPeriodType.Shootout;
        if (periodNumber > match.MatchRules.RegularPeriodCount)
            return HockeyPeriodType.Overtime;
        return HockeyPeriodType.RegularPeriod;
    }

    private static void ApplyMatchResult(
        HockeyMatchResultType result,
        HockeyTeamSlot slot,
        ref int regulationWins,
        ref int overtimeWins,
        ref int shootoutWins,
        ref int regulationLosses,
        ref int overtimeLosses,
        ref int shootoutLosses,
        ref int ties,
        ref int homeWins,
        ref int homeLosses,
        ref int awayWins,
        ref int awayLosses)
    {
        bool isHome = slot == HockeyTeamSlot.Home;
        switch (result)
        {
            case HockeyMatchResultType.HomeWin:
                if (isHome) { regulationWins++; homeWins++; } else { regulationLosses++; awayLosses++; }
                break;
            case HockeyMatchResultType.AwayWin:
                if (isHome) { regulationLosses++; homeLosses++; } else { regulationWins++; awayWins++; }
                break;
            case HockeyMatchResultType.OvertimeHomeWin:
                if (isHome) { overtimeWins++; homeWins++; } else { overtimeLosses++; awayLosses++; }
                break;
            case HockeyMatchResultType.OvertimeAwayWin:
                if (isHome) { overtimeLosses++; homeLosses++; } else { overtimeWins++; awayWins++; }
                break;
            case HockeyMatchResultType.ShootoutHomeWin:
                if (isHome) { shootoutWins++; homeWins++; } else { shootoutLosses++; awayLosses++; }
                break;
            case HockeyMatchResultType.ShootoutAwayWin:
                if (isHome) { shootoutLosses++; homeLosses++; } else { shootoutWins++; awayWins++; }
                break;
            case HockeyMatchResultType.Draw:
                ties++;
                break;
            case HockeyMatchResultType.ForfeitHomeWin:
                if (isHome) { regulationWins++; homeWins++; } else { regulationLosses++; awayLosses++; }
                break;
            case HockeyMatchResultType.ForfeitAwayWin:
                if (isHome) { regulationLosses++; homeLosses++; } else { regulationWins++; awayWins++; }
                break;
        }
    }
}

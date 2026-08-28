using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Statistics;
using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Entities.Hockey.Statistics;

/// <summary>
/// Aggregated team standings and stats for a competition (season or tournament) at a given scope.
/// Points are computed from <see cref="HockeyStandingRules"/> via <see cref="RecalculateStandingsMetrics"/>.
/// </summary>
public class HockeyTeamCompetitionStatistics : BaseEntity
{
    public Guid TeamId { get; private set; }
    public HockeyTeam? Team { get; private set; }

    public Guid CompetitionId { get; private set; }
    public HockeyCompetition? Competition { get; private set; }

    public HockeyStatisticsScope Scope { get; private set; }

    public Guid? CompetitionDivisionId { get; private set; }
    public HockeyCompetitionDivision? CompetitionDivision { get; private set; }

    public Guid? TournamentGroupId { get; private set; }
    public HockeyTournamentGroup? TournamentGroup { get; private set; }

    public Guid? PlayoffSeriesId { get; private set; }
    public HockeyPlayoffSeries? PlayoffSeries { get; private set; }

    public int GamesPlayed { get; private set; }
    public int RegulationWins { get; private set; }
    public int OvertimeWins { get; private set; }
    public int ShootoutWins { get; private set; }
    public int RegulationLosses { get; private set; }
    public int OvertimeLosses { get; private set; }
    public int ShootoutLosses { get; private set; }
    public int Ties { get; private set; }
    public int Wins { get; private set; }
    public int Losses { get; private set; }
    public int Points { get; private set; }
    public int GoalsFor { get; private set; }
    public int GoalsAgainst { get; private set; }
    public int GoalDifference { get; private set; }
    public int ShotsFor { get; private set; }
    public int ShotsAgainst { get; private set; }
    public decimal ShotPercentage { get; private set; }
    public int PowerPlayGoals { get; private set; }
    public int PowerPlayOpportunities { get; private set; }
    public decimal PowerPlayPercentage { get; private set; }
    public int PenaltyKillOpportunities { get; private set; }
    public int PenaltyKillSuccesses { get; private set; }
    public decimal PenaltyKillPercentage { get; private set; }
    public int PenaltyMinutes { get; private set; }
    public int FaceoffWins { get; private set; }
    public int FaceoffAttempts { get; private set; }
    public decimal FaceoffPercentage { get; private set; }
    public int HomeWins { get; private set; }
    public int HomeLosses { get; private set; }
    public int AwayWins { get; private set; }
    public int AwayLosses { get; private set; }
    public int StandingRank { get; private set; }

    private HockeyTeamCompetitionStatistics() { }

    public HockeyTeamCompetitionStatistics(
        Guid teamId,
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null)
    {
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));
        if (competitionId == Guid.Empty)
            throw new ArgumentException("Competition id cannot be empty.", nameof(competitionId));

        HockeyStatisticsScopeValidator.Validate(scope, competitionDivisionId, tournamentGroupId, playoffSeriesId);

        TeamId = teamId;
        CompetitionId = competitionId;
        Scope = scope;
        CompetitionDivisionId = competitionDivisionId;
        TournamentGroupId = tournamentGroupId;
        PlayoffSeriesId = playoffSeriesId;
        RecalculatePercentages();
    }

    public void UpdateRecord(
        int gamesPlayed,
        int regulationWins,
        int overtimeWins,
        int shootoutWins,
        int regulationLosses,
        int overtimeLosses,
        int shootoutLosses,
        int ties,
        int homeWins,
        int homeLosses,
        int awayWins,
        int awayLosses)
    {
        HockeyStatisticsMath.EnsureNonNegative(gamesPlayed, nameof(gamesPlayed));
        HockeyStatisticsMath.EnsureNonNegative(regulationWins, nameof(regulationWins));
        HockeyStatisticsMath.EnsureNonNegative(overtimeWins, nameof(overtimeWins));
        HockeyStatisticsMath.EnsureNonNegative(shootoutWins, nameof(shootoutWins));
        HockeyStatisticsMath.EnsureNonNegative(regulationLosses, nameof(regulationLosses));
        HockeyStatisticsMath.EnsureNonNegative(overtimeLosses, nameof(overtimeLosses));
        HockeyStatisticsMath.EnsureNonNegative(shootoutLosses, nameof(shootoutLosses));
        HockeyStatisticsMath.EnsureNonNegative(ties, nameof(ties));
        HockeyStatisticsMath.EnsureNonNegative(homeWins, nameof(homeWins));
        HockeyStatisticsMath.EnsureNonNegative(homeLosses, nameof(homeLosses));
        HockeyStatisticsMath.EnsureNonNegative(awayWins, nameof(awayWins));
        HockeyStatisticsMath.EnsureNonNegative(awayLosses, nameof(awayLosses));

        GamesPlayed = gamesPlayed;
        RegulationWins = regulationWins;
        OvertimeWins = overtimeWins;
        ShootoutWins = shootoutWins;
        RegulationLosses = regulationLosses;
        OvertimeLosses = overtimeLosses;
        ShootoutLosses = shootoutLosses;
        Ties = ties;
        HomeWins = homeWins;
        HomeLosses = homeLosses;
        AwayWins = awayWins;
        AwayLosses = awayLosses;
    }

    public void UpdateScoringAndSpecialTeams(
        int goalsFor,
        int goalsAgainst,
        int shotsFor,
        int shotsAgainst,
        int powerPlayGoals,
        int powerPlayOpportunities,
        int penaltyKillOpportunities,
        int penaltyKillSuccesses,
        int penaltyMinutes,
        int faceoffWins,
        int faceoffAttempts)
    {
        HockeyStatisticsMath.EnsureNonNegative(goalsFor, nameof(goalsFor));
        HockeyStatisticsMath.EnsureNonNegative(goalsAgainst, nameof(goalsAgainst));
        HockeyStatisticsMath.EnsureNonNegative(shotsFor, nameof(shotsFor));
        HockeyStatisticsMath.EnsureNonNegative(shotsAgainst, nameof(shotsAgainst));
        HockeyStatisticsMath.EnsureNonNegative(powerPlayGoals, nameof(powerPlayGoals));
        HockeyStatisticsMath.EnsureNonNegative(powerPlayOpportunities, nameof(powerPlayOpportunities));
        HockeyStatisticsMath.EnsureNonNegative(penaltyKillOpportunities, nameof(penaltyKillOpportunities));
        HockeyStatisticsMath.EnsureNonNegative(penaltyKillSuccesses, nameof(penaltyKillSuccesses));
        HockeyStatisticsMath.EnsureNonNegative(penaltyMinutes, nameof(penaltyMinutes));
        HockeyStatisticsMath.EnsureNonNegative(faceoffWins, nameof(faceoffWins));
        HockeyStatisticsMath.EnsureNonNegative(faceoffAttempts, nameof(faceoffAttempts));

        GoalsFor = goalsFor;
        GoalsAgainst = goalsAgainst;
        ShotsFor = shotsFor;
        ShotsAgainst = shotsAgainst;
        PowerPlayGoals = powerPlayGoals;
        PowerPlayOpportunities = powerPlayOpportunities;
        PenaltyKillOpportunities = penaltyKillOpportunities;
        PenaltyKillSuccesses = penaltyKillSuccesses;
        PenaltyMinutes = penaltyMinutes;
        FaceoffWins = faceoffWins;
        FaceoffAttempts = faceoffAttempts;
        RecalculatePercentages();
        GoalDifference = GoalsFor - GoalsAgainst;
    }

    public void SetStandingRank(int standingRank)
    {
        HockeyStatisticsMath.EnsureNonNegative(standingRank, nameof(standingRank));
        StandingRank = standingRank;
    }

    /// <summary>
    /// Recomputes Wins, Losses, GoalDifference and Points from standing rules.
    /// </summary>
    public void RecalculateStandingsMetrics(HockeyStandingRules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        Wins = RegulationWins + OvertimeWins + ShootoutWins;
        Losses = RegulationLosses + OvertimeLosses + ShootoutLosses;
        GoalDifference = GoalsFor - GoalsAgainst;
        Points =
            RegulationWins * rules.RegulationWinPoints
            + OvertimeWins * rules.OvertimeWinPoints
            + ShootoutWins * rules.ShootoutWinPoints
            + OvertimeLosses * rules.OvertimeLossPoints
            + ShootoutLosses * rules.ShootoutLossPoints
            + Ties * rules.TiePoints;
    }

    private void RecalculatePercentages()
    {
        ShotPercentage = HockeyStatisticsMath.Percentage(GoalsFor, ShotsFor);
        PowerPlayPercentage = HockeyStatisticsMath.Percentage(PowerPlayGoals, PowerPlayOpportunities);
        PenaltyKillPercentage = HockeyStatisticsMath.Percentage(PenaltyKillSuccesses, PenaltyKillOpportunities);
        FaceoffPercentage = HockeyStatisticsMath.Percentage(FaceoffWins, FaceoffAttempts);
    }
}

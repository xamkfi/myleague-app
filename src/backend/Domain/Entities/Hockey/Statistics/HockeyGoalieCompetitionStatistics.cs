using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Statistics;

namespace Domain.Entities.Hockey.Statistics;

/// <summary>
/// Aggregated goalie statistics for a competition (season or tournament) at a given scope.
/// </summary>
public class HockeyGoalieCompetitionStatistics : BaseEntity
{
    public Guid PlayerId { get; private set; }
    public HockeyPlayer? Player { get; private set; }

    public Guid TeamId { get; private set; }
    public HockeyTeam? Team { get; private set; }

    public Guid TeamPlayerId { get; private set; }
    public HockeyTeamPlayer? TeamPlayer { get; private set; }

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
    public int GamesStarted { get; private set; }
    public int Wins { get; private set; }
    public int Losses { get; private set; }
    public int OvertimeLosses { get; private set; }
    public int ShootoutLosses { get; private set; }
    public int NoDecisions { get; private set; }
    public int Saves { get; private set; }
    public int ShotsAgainst { get; private set; }
    public decimal SavePercentage { get; private set; }
    public int GoalsAgainst { get; private set; }
    public decimal GoalsAgainstAverage { get; private set; }
    public int Shutouts { get; private set; }
    public int MinutesPlayed { get; private set; }

    private HockeyGoalieCompetitionStatistics() { }

    public HockeyGoalieCompetitionStatistics(
        Guid playerId,
        Guid teamId,
        Guid teamPlayerId,
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null)
    {
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player id cannot be empty.", nameof(playerId));
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));
        if (teamPlayerId == Guid.Empty)
            throw new ArgumentException("Team player id cannot be empty.", nameof(teamPlayerId));
        if (competitionId == Guid.Empty)
            throw new ArgumentException("Competition id cannot be empty.", nameof(competitionId));

        HockeyStatisticsScopeValidator.Validate(scope, competitionDivisionId, tournamentGroupId, playoffSeriesId);

        PlayerId = playerId;
        TeamId = teamId;
        TeamPlayerId = teamPlayerId;
        CompetitionId = competitionId;
        Scope = scope;
        CompetitionDivisionId = competitionDivisionId;
        TournamentGroupId = tournamentGroupId;
        PlayoffSeriesId = playoffSeriesId;
        RecalculateDerived();
    }

    public void UpdateTotals(
        int gamesPlayed,
        int gamesStarted,
        int wins,
        int losses,
        int overtimeLosses,
        int shootoutLosses,
        int noDecisions,
        int saves,
        int shotsAgainst,
        int goalsAgainst,
        int shutouts,
        int minutesPlayed)
    {
        HockeyStatisticsMath.EnsureNonNegative(gamesPlayed, nameof(gamesPlayed));
        HockeyStatisticsMath.EnsureNonNegative(gamesStarted, nameof(gamesStarted));
        HockeyStatisticsMath.EnsureNonNegative(wins, nameof(wins));
        HockeyStatisticsMath.EnsureNonNegative(losses, nameof(losses));
        HockeyStatisticsMath.EnsureNonNegative(overtimeLosses, nameof(overtimeLosses));
        HockeyStatisticsMath.EnsureNonNegative(shootoutLosses, nameof(shootoutLosses));
        HockeyStatisticsMath.EnsureNonNegative(noDecisions, nameof(noDecisions));
        HockeyStatisticsMath.EnsureNonNegative(saves, nameof(saves));
        HockeyStatisticsMath.EnsureNonNegative(shotsAgainst, nameof(shotsAgainst));
        HockeyStatisticsMath.EnsureNonNegative(goalsAgainst, nameof(goalsAgainst));
        HockeyStatisticsMath.EnsureNonNegative(shutouts, nameof(shutouts));
        HockeyStatisticsMath.EnsureNonNegative(minutesPlayed, nameof(minutesPlayed));

        GamesPlayed = gamesPlayed;
        GamesStarted = gamesStarted;
        Wins = wins;
        Losses = losses;
        OvertimeLosses = overtimeLosses;
        ShootoutLosses = shootoutLosses;
        NoDecisions = noDecisions;
        Saves = saves;
        ShotsAgainst = shotsAgainst;
        GoalsAgainst = goalsAgainst;
        Shutouts = shutouts;
        MinutesPlayed = minutesPlayed;
        RecalculateDerived();
    }

    private void RecalculateDerived()
    {
        SavePercentage = HockeyStatisticsMath.Percentage(Saves, ShotsAgainst);
        GoalsAgainstAverage = HockeyStatisticsMath.GoalsAgainstAverage(GoalsAgainst, MinutesPlayed);
    }
}

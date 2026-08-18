using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Matches;

namespace Domain.Entities.Hockey.Statistics;

/// <summary>
/// Goalie stats for a single period within a match.
/// </summary>
public class HockeyGoaliePeriodStatistics : BaseEntity
{
    public Guid GoalieMatchStatisticsId { get; private set; }
    public HockeyGoalieMatchStatistics GoalieMatchStatistics { get; private set; } = null!;

    public Guid MatchId { get; private set; }
    public HockeyMatch Match { get; private set; } = null!;

    public Guid MatchTeamId { get; private set; }
    public HockeyMatchTeam MatchTeam { get; private set; } = null!;

    public Guid MatchActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer MatchActivePlayer { get; private set; } = null!;

    public Guid TeamPlayerId { get; private set; }
    public HockeyTeamPlayer? TeamPlayer { get; private set; }

    public Guid PlayerId { get; private set; }
    public HockeyPlayer? Player { get; private set; }

    public Guid TeamId { get; private set; }
    public HockeyTeam? Team { get; private set; }

    public int PeriodNumber { get; private set; }
    public HockeyPeriodType PeriodType { get; private set; }
    public int TimeOnIceSeconds { get; private set; }
    public int ShotsAgainst { get; private set; }
    public int Saves { get; private set; }
    public int GoalsAgainst { get; private set; }
    public decimal SavePercentage { get; private set; }

    private HockeyGoaliePeriodStatistics() { }

    internal HockeyGoaliePeriodStatistics(
        Guid goalieMatchStatisticsId,
        Guid matchId,
        Guid matchTeamId,
        Guid matchActivePlayerId,
        Guid teamPlayerId,
        Guid playerId,
        Guid teamId,
        int periodNumber,
        HockeyPeriodType periodType)
    {
        if (goalieMatchStatisticsId == Guid.Empty)
            throw new ArgumentException("Goalie match statistics id cannot be empty.", nameof(goalieMatchStatisticsId));
        if (matchId == Guid.Empty)
            throw new ArgumentException("Match id cannot be empty.", nameof(matchId));
        if (matchTeamId == Guid.Empty)
            throw new ArgumentException("Match team id cannot be empty.", nameof(matchTeamId));
        if (matchActivePlayerId == Guid.Empty)
            throw new ArgumentException("Match active player id cannot be empty.", nameof(matchActivePlayerId));
        if (teamPlayerId == Guid.Empty)
            throw new ArgumentException("Team player id cannot be empty.", nameof(teamPlayerId));
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player id cannot be empty.", nameof(playerId));
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));
        if (periodNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), "Period number must be at least 1.");

        GoalieMatchStatisticsId = goalieMatchStatisticsId;
        MatchId = matchId;
        MatchTeamId = matchTeamId;
        MatchActivePlayerId = matchActivePlayerId;
        TeamPlayerId = teamPlayerId;
        PlayerId = playerId;
        TeamId = teamId;
        PeriodNumber = periodNumber;
        PeriodType = periodType;
        RecalculatePercentages();
    }

    public void Update(int timeOnIceSeconds, int shotsAgainst, int saves, int goalsAgainst)
    {
        HockeyStatisticsMath.EnsureNonNegative(timeOnIceSeconds, nameof(timeOnIceSeconds));
        HockeyStatisticsMath.EnsureNonNegative(shotsAgainst, nameof(shotsAgainst));
        HockeyStatisticsMath.EnsureNonNegative(saves, nameof(saves));
        HockeyStatisticsMath.EnsureNonNegative(goalsAgainst, nameof(goalsAgainst));
        TimeOnIceSeconds = timeOnIceSeconds;
        ShotsAgainst = shotsAgainst;
        Saves = saves;
        GoalsAgainst = goalsAgainst;
        RecalculatePercentages();
    }

    private void RecalculatePercentages() =>
        SavePercentage = HockeyStatisticsMath.Percentage(Saves, ShotsAgainst);
}

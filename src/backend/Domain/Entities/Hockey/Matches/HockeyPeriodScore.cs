using Domain.Enums.Hockey.Matches;

namespace Domain.Entities.Hockey.Matches;

/// <summary>
/// Per-period score and basic counting stats for a hockey match.
/// Home/away sides are referenced via <see cref="HockeyMatchTeam"/> ids, not raw team ids.
/// </summary>
public class HockeyPeriodScore : BaseEntity
{
    /// <summary>Gets the parent match identifier.</summary>
    public Guid MatchId { get; private set; }

    /// <summary>Gets the parent match aggregate.</summary>
    public HockeyMatch Match { get; private set; } = null!;

    /// <summary>Gets the period number (1-based within its period type sequence).</summary>
    public int PeriodNumber { get; private set; }

    /// <summary>Gets whether this is a regular, overtime or shootout period.</summary>
    public HockeyPeriodType PeriodType { get; private set; }

    /// <summary>Gets the home match-team identifier for this period row.</summary>
    public Guid HomeMatchTeamId { get; private set; }

    /// <summary>Gets the home match-team navigation.</summary>
    public HockeyMatchTeam HomeMatchTeam { get; private set; } = null!;

    /// <summary>Gets the away match-team identifier for this period row.</summary>
    public Guid AwayMatchTeamId { get; private set; }

    /// <summary>Gets the away match-team navigation.</summary>
    public HockeyMatchTeam AwayMatchTeam { get; private set; } = null!;

    public int HomeGoals { get; private set; }
    public int AwayGoals { get; private set; }
    public int HomeShots { get; private set; }
    public int AwayShots { get; private set; }
    public int HomeFaceoffWins { get; private set; }
    public int AwayFaceoffWins { get; private set; }
    public bool IsCompleted { get; private set; }

    private HockeyPeriodScore() { }

    internal HockeyPeriodScore(
        Guid matchId,
        int periodNumber,
        HockeyPeriodType periodType,
        Guid homeMatchTeamId,
        Guid awayMatchTeamId)
    {
        if (matchId == Guid.Empty)
            throw new ArgumentException("Match id cannot be empty.", nameof(matchId));
        if (periodNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), "Period number must be at least 1.");
        if (homeMatchTeamId == Guid.Empty)
            throw new ArgumentException("Home match team id cannot be empty.", nameof(homeMatchTeamId));
        if (awayMatchTeamId == Guid.Empty)
            throw new ArgumentException("Away match team id cannot be empty.", nameof(awayMatchTeamId));
        if (homeMatchTeamId == awayMatchTeamId)
            throw new ArgumentException("Home and away match teams must be different.", nameof(awayMatchTeamId));

        MatchId = matchId;
        PeriodNumber = periodNumber;
        PeriodType = periodType;
        HomeMatchTeamId = homeMatchTeamId;
        AwayMatchTeamId = awayMatchTeamId;
    }

    internal void UpdateCounts(
        int homeGoals,
        int awayGoals,
        int homeShots,
        int awayShots,
        int homeFaceoffWins,
        int awayFaceoffWins)
    {
        EnsureNonNegative(homeGoals, nameof(homeGoals));
        EnsureNonNegative(awayGoals, nameof(awayGoals));
        EnsureNonNegative(homeShots, nameof(homeShots));
        EnsureNonNegative(awayShots, nameof(awayShots));
        EnsureNonNegative(homeFaceoffWins, nameof(homeFaceoffWins));
        EnsureNonNegative(awayFaceoffWins, nameof(awayFaceoffWins));

        HomeGoals = homeGoals;
        AwayGoals = awayGoals;
        HomeShots = homeShots;
        AwayShots = awayShots;
        HomeFaceoffWins = homeFaceoffWins;
        AwayFaceoffWins = awayFaceoffWins;
    }

    internal void MarkCompleted() => IsCompleted = true;

    private static void EnsureNonNegative(int value, string paramName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, "Value cannot be negative.");
    }
}

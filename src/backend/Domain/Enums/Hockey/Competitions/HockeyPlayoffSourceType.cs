namespace Domain.Enums.Hockey.Competitions;

/// <summary>
/// Identifies how a team enters a playoff slot.
/// </summary>
public enum HockeyPlayoffSourceType
{
    /// <summary>
    /// GroupWinner
    /// </summary>
    GroupWinner = 0,
    /// <summary>
    /// GroupRunnerUp
    /// </summary>
    GroupRunnerUp = 1,
    /// <summary>
    /// GroupRank
    /// </summary>
    GroupRank = 2,
    /// <summary>
    /// Seed
    /// </summary>
    Seed = 3,
    /// <summary>
    /// WildCard
    /// </summary>
    WildCard = 4,
    /// <summary>
    /// PreviousSeriesWinner
    /// </summary>
    PreviousSeriesWinner = 5,
    /// <summary>
    /// PreviousSeriesLoser
    /// </summary>
    PreviousSeriesLoser = 6,
    /// <summary>
    /// ManualTeam
    /// </summary>
    ManualTeam = 7
}


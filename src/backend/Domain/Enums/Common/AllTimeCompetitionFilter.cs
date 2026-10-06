namespace Domain.Enums.Common;

/// <summary>
/// Which competitions to include when summing all-time player statistics.
/// </summary>
public enum AllTimeCompetitionFilter
{
    /// <summary>
    /// Regular seasons only.
    /// </summary>
    Season = 0,

    /// <summary>
    /// Tournaments only.
    /// </summary>
    Tournament = 1,

    /// <summary>
    /// Seasons and tournaments.
    /// </summary>
    All = 2
}

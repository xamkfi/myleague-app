namespace Domain.Enums.Football;

/// <summary>
/// Lifecycle status of a football match.
/// </summary>
public enum FootballMatchStatus
{
    None = 0,
    Scheduled = 1,
    Postponed = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5
}

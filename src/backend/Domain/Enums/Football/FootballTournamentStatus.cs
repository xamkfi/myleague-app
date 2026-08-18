namespace Domain.Enums.Football;

/// <summary>
/// Lifecycle status of a football tournament.
/// </summary>
public enum FootballTournamentStatus
{
    Draft = 0,
    GroupStage = 1,
    PlayoffStage = 2,
    Completed = 3,
    Cancelled = 4
}

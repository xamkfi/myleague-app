namespace Domain.Enums.Floorball;

/// <summary>
/// Represents the current lifecycle status of a tournament
/// </summary>
public enum FloorballTournamentStatus
{
    Draft = 0,
    GroupStage = 1,
    PlayoffStage = 2,
    Completed = 3,
    Cancelled = 4
}

namespace Domain.Enums.Floorball;

/// <summary>
/// Represents the current lifecycle status of a tournament
/// </summary>
public enum FloorballTournamentStatus
{
    Draft = 0,
    Registration = 1,
    GroupStage = 2,
    PlayoffStage = 3,
    Completed = 4
}

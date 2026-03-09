namespace Domain.Enums.Floorball.Tournament;

/// <summary>
/// Represents the lifecycle status of a floorball tournament
/// </summary>
public enum FloorballTournamentStatus
{
    /// <summary>
    /// Tournament is being set up, not yet visible to public
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Tournament is published and visible, but matches have not started
    /// </summary>
    Active = 1,

    /// <summary>
    /// Tournament matches are currently being played
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// All tournament matches have been completed
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Tournament has been cancelled
    /// </summary>
    Cancelled = 4
}

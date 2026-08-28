namespace Domain.Enums.Hockey.Competitions;

/// <summary>
/// Represents the current stage of a hockey tournament.
/// </summary>
public enum HockeyTournamentStage
{
    /// <summary>
    /// Registration
    /// </summary>
    Registration = 0,
    /// <summary>
    /// GroupStage
    /// </summary>
    GroupStage = 1,
    /// <summary>
    /// Playoffs
    /// </summary>
    Playoffs = 2,
    /// <summary>
    /// Finals
    /// </summary>
    Finals = 3,
    /// <summary>
    /// Completed
    /// </summary>
    Completed = 4
}


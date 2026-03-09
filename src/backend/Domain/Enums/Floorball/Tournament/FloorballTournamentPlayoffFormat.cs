namespace Domain.Enums.Floorball.Tournament;

/// <summary>
/// Defines how the playoff stage is structured after the group stage
/// </summary>
public enum FloorballTournamentPlayoffFormat
{
    /// <summary>
    /// No playoff stage; tournament ends after group stage
    /// </summary>
    None = 0,

    /// <summary>
    /// Single-elimination bracket (quarter-finals, semi-finals, final, third-place match)
    /// </summary>
    SingleElimination = 1,

    /// <summary>
    /// Advancing teams form a new final group and play a round-robin
    /// </summary>
    FinalGroup = 2
}

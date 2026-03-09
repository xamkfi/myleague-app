namespace Domain.Enums.Floorball.Tournament;

/// <summary>
/// Identifies the round within a single-elimination playoff bracket.
/// Used on FloorballMatch when TournamentGroupId is null (bracket match).
/// </summary>
public enum FloorballTournamentRound
{
    /// <summary>
    /// Round of 16 (applicable when 16+ teams advance)
    /// </summary>
    RoundOf16 = 0,

    /// <summary>
    /// Quarter-final round
    /// </summary>
    QuarterFinal = 1,

    /// <summary>
    /// Semi-final round
    /// </summary>
    SemiFinal = 2,

    /// <summary>
    /// Third-place match
    /// </summary>
    ThirdPlace = 3,

    /// <summary>
    /// Final match
    /// </summary>
    Final = 4
}

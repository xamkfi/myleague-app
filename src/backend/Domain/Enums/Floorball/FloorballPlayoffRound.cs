namespace Domain.Enums.Floorball;

/// <summary>
/// Represents a round within a tournament's playoff bracket.
/// The order of the values reflects how rounds are advanced through the bracket
/// (Quarterfinal -> Semifinal -> Final/ThirdPlaceMatch).
/// </summary>
public enum FloorballPlayoffRound
{
    /// <summary>
    /// Sentinel value (no playoff round assigned).
    /// </summary>
    None = 0,

    /// <summary>
    /// Round of 8 -> 4 (quarterfinals).
    /// </summary>
    QuarterFinal = 1,

    /// <summary>
    /// Round of 4 -> 2 (semifinals).
    /// </summary>
    SemiFinal = 2,

    /// <summary>
    /// Optional 3rd place match between the two semifinal losers.
    /// </summary>
    ThirdPlaceMatch = 3,

    /// <summary>
    /// The grand final.
    /// </summary>
    Final = 4
}

/// <summary>
/// Identifies the slot of a follow-up playoff match that the winner of a feeder match advances into.
/// </summary>
public enum FloorballPlayoffSlot
{
    /// <summary>
    /// Winner becomes the home team of the next match.
    /// </summary>
    Home = 0,

    /// <summary>
    /// Winner becomes the away team of the next match.
    /// </summary>
    Away = 1
}

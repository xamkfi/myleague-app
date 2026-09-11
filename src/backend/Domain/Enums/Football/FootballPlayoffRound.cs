namespace Domain.Enums.Football;

/// <summary>
/// A round within a tournament's knockout bracket.
/// </summary>
public enum FootballPlayoffRound
{
    None = 0,
    QuarterFinal = 1,
    SemiFinal = 2,
    ThirdPlaceMatch = 3,
    Final = 4
}

/// <summary>
/// Identifies the slot of a follow-up playoff match that the winner of a feeder match advances into.
/// </summary>
public enum FootballPlayoffSlot
{
    Home = 0,
    Away = 1
}

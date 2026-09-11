namespace Domain.Enums.Hockey.Matches;

/// <summary>
/// Represents the severity classification of a penalty.
/// </summary>
public enum HockeyPenaltySeverity
{
    /// <summary>
    /// Minor
    /// </summary>
    Minor = 0,
    /// <summary>
    /// BenchMinor
    /// </summary>
    BenchMinor = 1,
    /// <summary>
    /// DoubleMinor
    /// </summary>
    DoubleMinor = 2,
    /// <summary>
    /// Major
    /// </summary>
    Major = 3,
    /// <summary>
    /// Misconduct
    /// </summary>
    Misconduct = 4,
    /// <summary>
    /// GameMisconduct
    /// </summary>
    GameMisconduct = 5,
    /// <summary>
    /// MatchPenalty
    /// </summary>
    MatchPenalty = 6,
    /// <summary>
    /// PenaltyShot
    /// </summary>
    PenaltyShot = 7
}


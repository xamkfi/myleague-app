namespace Domain.Enums.Common;

/// <summary>
/// The phase of play that decided a match. Standings points depend on it.
/// </summary>
public enum MatchDecision
{
    /// <summary>Decided in regulation time.</summary>
    Regulation = 0,

    /// <summary>Decided in overtime (extra time in football).</summary>
    Overtime = 1,

    /// <summary>Decided in a shootout (penalty shootout in football).</summary>
    Shootout = 2
}

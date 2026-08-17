namespace Domain.Enums.Football;

/// <summary>
/// Types of events that can occur in a football match.
/// </summary>
public enum FootballEventType
{
    Goal = 0,
    Card = 1,
    Substitution = 2,
    HalfStart = 3,
    HalfEnd = 4,
    MatchStart = 5,
    MatchEnd = 6,
    ExtraTimeStart = 7,
    PenaltyShootoutStart = 8,
    Other = 99
}

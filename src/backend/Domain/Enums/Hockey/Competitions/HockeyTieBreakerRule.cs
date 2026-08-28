namespace Domain.Enums.Hockey.Competitions;

/// <summary>
/// Represents a tie-breaking rule used in standings.
/// </summary>
public enum HockeyTieBreakerRule
{
    /// <summary>
    /// Points
    /// </summary>
    Points = 0,
    /// <summary>
    /// RegulationWins
    /// </summary>
    RegulationWins = 1,
    /// <summary>
    /// Wins
    /// </summary>
    Wins = 2,
    /// <summary>
    /// HeadToHeadPoints
    /// </summary>
    HeadToHeadPoints = 3,
    /// <summary>
    /// GoalDifference
    /// </summary>
    GoalDifference = 4,
    /// <summary>
    /// GoalsFor
    /// </summary>
    GoalsFor = 5,
    /// <summary>
    /// GoalsAgainst
    /// </summary>
    GoalsAgainst = 6,
    /// <summary>
    /// FewestPenaltyMinutes
    /// </summary>
    FewestPenaltyMinutes = 7,
    /// <summary>
    /// ManualDecision
    /// </summary>
    ManualDecision = 8
}


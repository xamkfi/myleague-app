namespace Domain.Enums.Hockey.Matches;

/// <summary>
/// Represents the decision outcome of a review.
/// </summary>
public enum HockeyReviewDecision
{
    /// <summary>
    /// Goal
    /// </summary>
    Goal = 0,
    /// <summary>
    /// NoGoal
    /// </summary>
    NoGoal = 1,
    /// <summary>
    /// Penalty
    /// </summary>
    Penalty = 2,
    /// <summary>
    /// NoPenalty
    /// </summary>
    NoPenalty = 3,
    /// <summary>
    /// Confirmed
    /// </summary>
    Confirmed = 4,
    /// <summary>
    /// Overturned
    /// </summary>
    Overturned = 5,
    /// <summary>
    /// Inconclusive
    /// </summary>
    Inconclusive = 6
}


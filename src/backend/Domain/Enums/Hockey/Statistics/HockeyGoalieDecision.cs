namespace Domain.Enums.Hockey.Statistics;

/// <summary>
/// Represents the goalie decision recorded for a game.
/// </summary>
public enum HockeyGoalieDecision
{
    /// <summary>
    /// Win
    /// </summary>
    Win = 0,
    /// <summary>
    /// Loss
    /// </summary>
    Loss = 1,
    /// <summary>
    /// OvertimeLoss
    /// </summary>
    OvertimeLoss = 2,
    /// <summary>
    /// ShootoutLoss
    /// </summary>
    ShootoutLoss = 3,
    /// <summary>
    /// Tie
    /// </summary>
    Tie = 4,
    /// <summary>
    /// NoDecision
    /// </summary>
    NoDecision = 5
}


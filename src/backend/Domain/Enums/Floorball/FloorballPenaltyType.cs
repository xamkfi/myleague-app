namespace Domain.Enums.Floorball;

/// <summary>
/// Represents the types of penalties in floorball
/// </summary>
public enum FloorballPenaltyType
{
    /// <summary>
    /// No penalty assigned
    /// </summary>
    None = 0,

    /// <summary>
    /// 2-minute minor penalty
    /// </summary>
    Minor = 1,
    
    /// <summary>
    /// 5-minute major penalty
    /// </summary>
    Major = 2,
    
    /// <summary>
    /// 10-minute misconduct penalty
    /// </summary>
    Misconduct = 3,
    
    /// <summary>
    /// Match penalty (player is ejected from the game)
    /// </summary>
    MatchPenalty = 4,
    
    /// <summary>
    /// Technical penalty (e.g., too many players on the field)
    /// </summary>
    Technical = 5,
    
    /// <summary>
    /// Penalty shot awarded
    /// </summary>
    PenaltyShot = 6
} 

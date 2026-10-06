namespace Domain.Enums.Common;

/// <summary>
/// Column used to order an all-time player statistics list.
/// </summary>
public enum AllTimeStatSort
{
    /// <summary>
    /// Games played.
    /// </summary>
    Games = 0,

    /// <summary>
    /// Goals.
    /// </summary>
    Goals = 1,

    /// <summary>
    /// Assists.
    /// </summary>
    Assists = 2,

    /// <summary>
    /// Points (goals + assists).
    /// </summary>
    Points = 3,

    /// <summary>
    /// Penalty minutes. Used by floorball and hockey.
    /// </summary>
    Penalties = 4,

    /// <summary>
    /// Yellow cards. Used by football.
    /// </summary>
    YellowCards = 5,

    /// <summary>
    /// Red cards. Used by football.
    /// </summary>
    RedCards = 6
}

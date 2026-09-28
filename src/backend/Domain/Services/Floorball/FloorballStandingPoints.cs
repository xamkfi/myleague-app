using Domain.Enums.Floorball;

namespace Domain.Services.Floorball;

/// <summary>
/// League points for a floorball match.
/// Regulation win is 3. A shootout win is 2 and a shootout loss is 1.
/// A draw is 1 point for each team.
/// </summary>
public static class FloorballStandingPoints
{
    public const int RegulationWinPoints = 3;
    public const int ShootoutWinPoints = 2;
    public const int ShootoutLossPoints = 1;
    public const int TiePoints = 1;

    public static int For(FloorballGameResult result, bool wentToShootout)
    {
        return result switch
        {
            FloorballGameResult.Win => wentToShootout ? ShootoutWinPoints : RegulationWinPoints,
            FloorballGameResult.Loss => wentToShootout ? ShootoutLossPoints : 0,
            FloorballGameResult.Tie => TiePoints,
            _ => throw new ArgumentException($"Invalid game result: {result}", nameof(result))
        };
    }

    public static int ForScore(int goalsFor, int goalsAgainst, bool wentToShootout)
    {
        FloorballGameResult result = goalsFor > goalsAgainst
            ? FloorballGameResult.Win
            : goalsFor < goalsAgainst
                ? FloorballGameResult.Loss
                : FloorballGameResult.Tie;
        return For(result, wentToShootout);
    }
}

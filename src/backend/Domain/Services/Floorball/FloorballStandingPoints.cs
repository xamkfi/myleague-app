using Domain.Enums.Floorball;
using Domain.ValueObjects.Floorball;

namespace Domain.Services.Floorball;

/// <summary>
/// League points for a floorball match.
/// The parameterless helpers keep the historical tournament formula: regulation win 3,
/// shootout win 2, shootout loss 1, draw 1. A goal in overtime without a shootout stays a regulation win.
/// Season tables pass <see cref="FloorballStandingRules"/> and treat overtime and shootouts the same.
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

    public static int For(
        FloorballStandingRules rules,
        FloorballGameResult result,
        bool wentToOvertime,
        bool wentToShootout)
    {
        ArgumentNullException.ThrowIfNull(rules);
        bool decidedAfterRegulation = wentToOvertime || wentToShootout;
        return result switch
        {
            FloorballGameResult.Win => decidedAfterRegulation ? rules.OvertimeWinPoints : rules.WinPoints,
            FloorballGameResult.Loss => decidedAfterRegulation ? rules.OvertimeLossPoints : 0,
            FloorballGameResult.Tie => rules.DrawPoints,
            _ => throw new ArgumentException($"Invalid game result: {result}", nameof(result))
        };
    }

    public static int ForScore(int goalsFor, int goalsAgainst, bool wentToShootout)
    {
        return For(ResultFor(goalsFor, goalsAgainst), wentToShootout);
    }

    public static int ForScore(
        FloorballStandingRules rules,
        int goalsFor,
        int goalsAgainst,
        bool wentToOvertime,
        bool wentToShootout)
    {
        return For(rules, ResultFor(goalsFor, goalsAgainst), wentToOvertime, wentToShootout);
    }

    private static FloorballGameResult ResultFor(int goalsFor, int goalsAgainst)
    {
        if (goalsFor > goalsAgainst)
            return FloorballGameResult.Win;
        if (goalsFor < goalsAgainst)
            return FloorballGameResult.Loss;
        return FloorballGameResult.Tie;
    }
}

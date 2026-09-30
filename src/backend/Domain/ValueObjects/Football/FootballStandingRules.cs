namespace Domain.ValueObjects.Football;

/// <summary>
/// Point allocation for football standings. Hobby default is 3–1–0.
/// Extra time and penalty shootouts use the overtime values. Defaults match a full win and a zero loss,
/// so existing tables stay unchanged until an admin sets different overtime points.
/// </summary>
public class FootballStandingRules : IEquatable<FootballStandingRules>
{
    public int WinPoints { get; private set; }
    public int DrawPoints { get; private set; }
    public int LossPoints { get; private set; }
    public int OvertimeWinPoints { get; private set; }
    public int OvertimeLossPoints { get; private set; }

    private FootballStandingRules()
    {
        WinPoints = 3;
        DrawPoints = 1;
        LossPoints = 0;
        OvertimeWinPoints = 3;
        OvertimeLossPoints = 0;
    }

    public FootballStandingRules(
        int winPoints,
        int drawPoints,
        int lossPoints,
        int overtimeWinPoints = 3,
        int overtimeLossPoints = 0)
    {
        if (winPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(winPoints), "Win points cannot be negative.");
        if (drawPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(drawPoints), "Draw points cannot be negative.");
        if (lossPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(lossPoints), "Loss points cannot be negative.");
        if (overtimeWinPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(overtimeWinPoints), "Overtime win points cannot be negative.");
        if (overtimeLossPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(overtimeLossPoints), "Overtime loss points cannot be negative.");

        WinPoints = winPoints;
        DrawPoints = drawPoints;
        LossPoints = lossPoints;
        OvertimeWinPoints = overtimeWinPoints;
        OvertimeLossPoints = overtimeLossPoints;
    }

    public static FootballStandingRules Default() => new(3, 1, 0);

    public int PointsFor(Enums.Football.FootballGameResult result, bool decidedAfterRegulation = false) => result switch
    {
        Enums.Football.FootballGameResult.Win => decidedAfterRegulation ? OvertimeWinPoints : WinPoints,
        Enums.Football.FootballGameResult.Draw => DrawPoints,
        Enums.Football.FootballGameResult.Loss => decidedAfterRegulation ? OvertimeLossPoints : LossPoints,
        _ => throw new ArgumentOutOfRangeException(nameof(result))
    };

    public override bool Equals(object? obj) => Equals(obj as FootballStandingRules);

    public bool Equals(FootballStandingRules? other)
    {
        if (other is null)
            return false;
        return WinPoints == other.WinPoints
            && DrawPoints == other.DrawPoints
            && LossPoints == other.LossPoints
            && OvertimeWinPoints == other.OvertimeWinPoints
            && OvertimeLossPoints == other.OvertimeLossPoints;
    }

    public override int GetHashCode() =>
        HashCode.Combine(WinPoints, DrawPoints, LossPoints, OvertimeWinPoints, OvertimeLossPoints);

    public static bool operator ==(FootballStandingRules? left, FootballStandingRules? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(FootballStandingRules? left, FootballStandingRules? right) => !(left == right);
}

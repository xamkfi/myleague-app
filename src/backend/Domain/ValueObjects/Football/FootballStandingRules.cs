namespace Domain.ValueObjects.Football;

/// <summary>
/// Point allocation for football standings. Hobby default is 3–1–0.
/// </summary>
public class FootballStandingRules : IEquatable<FootballStandingRules>
{
    public int WinPoints { get; private set; }
    public int DrawPoints { get; private set; }
    public int LossPoints { get; private set; }

    private FootballStandingRules()
    {
        WinPoints = 3;
        DrawPoints = 1;
        LossPoints = 0;
    }

    public FootballStandingRules(int winPoints, int drawPoints, int lossPoints)
    {
        if (winPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(winPoints), "Win points cannot be negative.");
        if (drawPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(drawPoints), "Draw points cannot be negative.");
        if (lossPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(lossPoints), "Loss points cannot be negative.");

        WinPoints = winPoints;
        DrawPoints = drawPoints;
        LossPoints = lossPoints;
    }

    public static FootballStandingRules Default() => new(3, 1, 0);

    public int PointsFor(Enums.Football.FootballGameResult result) => result switch
    {
        Enums.Football.FootballGameResult.Win => WinPoints,
        Enums.Football.FootballGameResult.Draw => DrawPoints,
        Enums.Football.FootballGameResult.Loss => LossPoints,
        _ => throw new ArgumentOutOfRangeException(nameof(result))
    };

    public override bool Equals(object? obj) => Equals(obj as FootballStandingRules);

    public bool Equals(FootballStandingRules? other)
    {
        if (other is null)
            return false;
        return WinPoints == other.WinPoints
            && DrawPoints == other.DrawPoints
            && LossPoints == other.LossPoints;
    }

    public override int GetHashCode() => HashCode.Combine(WinPoints, DrawPoints, LossPoints);

    public static bool operator ==(FootballStandingRules? left, FootballStandingRules? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(FootballStandingRules? left, FootballStandingRules? right) => !(left == right);
}

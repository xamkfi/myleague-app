namespace Domain.ValueObjects.Floorball;

/// <summary>
/// Point allocation for a floorball season table.
/// A regulation win uses <see cref="WinPoints"/>. A win or loss after overtime or a shootout
/// uses the overtime values. A draw uses <see cref="DrawPoints"/>. A regulation loss is 0.
/// </summary>
public class FloorballStandingRules : IEquatable<FloorballStandingRules>
{
    public int WinPoints { get; private set; }
    public int DrawPoints { get; private set; }
    public int OvertimeWinPoints { get; private set; }
    public int OvertimeLossPoints { get; private set; }

    private FloorballStandingRules()
    {
        WinPoints = 3;
        DrawPoints = 1;
        OvertimeWinPoints = 2;
        OvertimeLossPoints = 1;
    }

    public FloorballStandingRules(int winPoints, int drawPoints, int overtimeWinPoints, int overtimeLossPoints)
    {
        if (winPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(winPoints), "Win points cannot be negative.");
        if (drawPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(drawPoints), "Draw points cannot be negative.");
        if (overtimeWinPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(overtimeWinPoints), "Overtime win points cannot be negative.");
        if (overtimeLossPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(overtimeLossPoints), "Overtime loss points cannot be negative.");

        WinPoints = winPoints;
        DrawPoints = drawPoints;
        OvertimeWinPoints = overtimeWinPoints;
        OvertimeLossPoints = overtimeLossPoints;
    }

    public static FloorballStandingRules Default() => new(3, 1, 2, 1);

    public override bool Equals(object? obj) => Equals(obj as FloorballStandingRules);

    public bool Equals(FloorballStandingRules? other)
    {
        if (other is null)
            return false;
        return WinPoints == other.WinPoints
            && DrawPoints == other.DrawPoints
            && OvertimeWinPoints == other.OvertimeWinPoints
            && OvertimeLossPoints == other.OvertimeLossPoints;
    }

    public override int GetHashCode() => HashCode.Combine(WinPoints, DrawPoints, OvertimeWinPoints, OvertimeLossPoints);

    public static bool operator ==(FloorballStandingRules? left, FloorballStandingRules? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(FloorballStandingRules? left, FloorballStandingRules? right) => !(left == right);
}

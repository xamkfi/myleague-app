using System;

namespace Domain.ValueObjects.Floorball;

/// <summary>
/// Represents the configurable rules for matches within a season.
/// Stored as an owned entity on both FloorballSeason and FloorballMatch.
/// </summary>
public class FloorballMatchRules : IEquatable<FloorballMatchRules>
{
    /// <summary>
    /// Gets the number of regular periods (e.g., 2 or 3).
    /// </summary>
    public int NumberOfPeriods { get; private set; }

    /// <summary>
    /// Gets the duration in minutes for each regular period (e.g., 15 or 20).
    /// </summary>
    public int PeriodDurationMinutes { get; private set; }

    /// <summary>
    /// Gets whether overtime is allowed when the match is tied after regular periods.
    /// </summary>
    public bool AllowOvertime { get; private set; }

    /// <summary>
    /// Gets the duration in minutes for the overtime period (e.g., 5 or 10).
    /// Only relevant when <see cref="AllowOvertime"/> is true.
    /// </summary>
    public int OvertimeDurationMinutes { get; private set; }

    /// <summary>
    /// Gets whether a shootout is allowed after overtime.
    /// Only relevant when <see cref="AllowOvertime"/> is true.
    /// </summary>
    public bool AllowShootout { get; private set; }

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private FloorballMatchRules()
    {
        NumberOfPeriods = 2;
        PeriodDurationMinutes = 15;
        AllowOvertime = true;
        OvertimeDurationMinutes = 5;
        AllowShootout = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FloorballMatchRules"/> class.
    /// </summary>
    /// <param name="numberOfPeriods">Number of regular periods (1-5).</param>
    /// <param name="periodDurationMinutes">Duration in minutes per regular period (1-60).</param>
    /// <param name="allowOvertime">Whether overtime is allowed.</param>
    /// <param name="overtimeDurationMinutes">Duration in minutes for overtime (1-30). Ignored if overtime is not allowed.</param>
    /// <param name="allowShootout">Whether shootout is allowed. Ignored if overtime is not allowed.</param>
    public FloorballMatchRules(
        int numberOfPeriods,
        int periodDurationMinutes,
        bool allowOvertime,
        int overtimeDurationMinutes,
        bool allowShootout)
    {
        if (numberOfPeriods < 1 || numberOfPeriods > 5)
            throw new ArgumentOutOfRangeException(nameof(numberOfPeriods), "Number of periods must be between 1 and 5.");

        if (periodDurationMinutes < 1 || periodDurationMinutes > 60)
            throw new ArgumentOutOfRangeException(nameof(periodDurationMinutes), "Period duration must be between 1 and 60 minutes.");

        if (allowOvertime && (overtimeDurationMinutes < 1 || overtimeDurationMinutes > 30))
            throw new ArgumentOutOfRangeException(nameof(overtimeDurationMinutes), "Overtime duration must be between 1 and 30 minutes.");

        NumberOfPeriods = numberOfPeriods;
        PeriodDurationMinutes = periodDurationMinutes;
        AllowOvertime = allowOvertime;
        OvertimeDurationMinutes = allowOvertime ? overtimeDurationMinutes : 0;
        AllowShootout = allowShootout;
    }

    /// <summary>
    /// Creates a default set of floorball match rules (2 periods, 15 min, overtime + shootout allowed).
    /// </summary>
    public static FloorballMatchRules Default()
    {
        return new FloorballMatchRules(
            numberOfPeriods: 2,
            periodDurationMinutes: 15,
            allowOvertime: true,
            overtimeDurationMinutes: 5,
            allowShootout: true);
    }

    /// <summary>
    /// Gets the period duration in seconds for regular periods.
    /// </summary>
    public int PeriodDurationSeconds => PeriodDurationMinutes * 60;

    /// <summary>
    /// Gets the overtime duration in seconds.
    /// </summary>
    public int OvertimeDurationSeconds => OvertimeDurationMinutes * 60;

    public override bool Equals(object? obj)
    {
        return Equals(obj as FloorballMatchRules);
    }

    public bool Equals(FloorballMatchRules? other)
    {
        if (other is null)
            return false;

        return NumberOfPeriods == other.NumberOfPeriods
            && PeriodDurationMinutes == other.PeriodDurationMinutes
            && AllowOvertime == other.AllowOvertime
            && OvertimeDurationMinutes == other.OvertimeDurationMinutes
            && AllowShootout == other.AllowShootout;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(NumberOfPeriods, PeriodDurationMinutes, AllowOvertime, OvertimeDurationMinutes, AllowShootout);
    }

    public static bool operator ==(FloorballMatchRules? left, FloorballMatchRules? right)
    {
        if (ReferenceEquals(left, null))
            return ReferenceEquals(right, null);

        return left.Equals(right);
    }

    public static bool operator !=(FloorballMatchRules? left, FloorballMatchRules? right) => !(left == right);

    public override string ToString()
    {
        return $"{NumberOfPeriods} periods x {PeriodDurationMinutes}min, OT: {AllowOvertime} ({OvertimeDurationMinutes}min), SO: {AllowShootout}";
    }
}

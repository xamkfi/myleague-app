using Domain.Enums.Hockey.Competitions;

namespace Domain.ValueObjects.Hockey.Rules;

/// <summary>
/// Configurable match rules for a hockey competition.
/// </summary>
public class HockeyMatchRules : IEquatable<HockeyMatchRules>
{
    public int RegularPeriodCount { get; private set; }
    public int RegularPeriodLengthMinutes { get; private set; }
    public int OvertimeLengthMinutes { get; private set; }
    public bool StopClock { get; private set; }
    public bool OvertimeEnabled { get; private set; }
    public bool ShootoutEnabled { get; private set; }
    public bool OffsideEnabled { get; private set; }
    public bool DelayedOffsideEnabled { get; private set; }
    public HockeyIcingRule IcingRule { get; private set; }
    public bool PenaltyShotEnabled { get; private set; }
    public bool GoaliePullAllowed { get; private set; }

    private HockeyMatchRules() { }

    public HockeyMatchRules(
        int regularPeriodCount,
        int regularPeriodLengthMinutes,
        int overtimeLengthMinutes,
        bool stopClock,
        bool overtimeEnabled,
        bool shootoutEnabled,
        bool offsideEnabled,
        bool delayedOffsideEnabled,
        HockeyIcingRule icingRule,
        bool penaltyShotEnabled,
        bool goaliePullAllowed)
    {
        if (regularPeriodCount < 1 || regularPeriodCount > 5)
            throw new ArgumentOutOfRangeException(nameof(regularPeriodCount), "Regular period count must be between 1 and 5.");
        if (regularPeriodLengthMinutes < 1 || regularPeriodLengthMinutes > 60)
            throw new ArgumentOutOfRangeException(nameof(regularPeriodLengthMinutes), "Regular period length must be between 1 and 60 minutes.");
        if (overtimeLengthMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(overtimeLengthMinutes), "Overtime length cannot be negative.");
        if (overtimeEnabled && (overtimeLengthMinutes < 1 || overtimeLengthMinutes > 30))
            throw new ArgumentOutOfRangeException(nameof(overtimeLengthMinutes), "Overtime length must be between 1 and 30 minutes when overtime is enabled.");

        RegularPeriodCount = regularPeriodCount;
        RegularPeriodLengthMinutes = regularPeriodLengthMinutes;
        OvertimeLengthMinutes = overtimeEnabled ? overtimeLengthMinutes : 0;
        StopClock = stopClock;
        OvertimeEnabled = overtimeEnabled;
        ShootoutEnabled = shootoutEnabled && overtimeEnabled;
        OffsideEnabled = offsideEnabled;
        DelayedOffsideEnabled = delayedOffsideEnabled;
        IcingRule = icingRule;
        PenaltyShotEnabled = penaltyShotEnabled;
        GoaliePullAllowed = goaliePullAllowed;
    }

    public static HockeyMatchRules Default() =>
        new(3, 20, 5, stopClock: true, overtimeEnabled: true, shootoutEnabled: true,
            offsideEnabled: true, delayedOffsideEnabled: true, HockeyIcingRule.Hybrid,
            penaltyShotEnabled: true, goaliePullAllowed: true);

    public override bool Equals(object? obj) => Equals(obj as HockeyMatchRules);

    public bool Equals(HockeyMatchRules? other)
    {
        if (other is null) return false;
        return RegularPeriodCount == other.RegularPeriodCount
            && RegularPeriodLengthMinutes == other.RegularPeriodLengthMinutes
            && OvertimeLengthMinutes == other.OvertimeLengthMinutes
            && StopClock == other.StopClock
            && OvertimeEnabled == other.OvertimeEnabled
            && ShootoutEnabled == other.ShootoutEnabled
            && OffsideEnabled == other.OffsideEnabled
            && DelayedOffsideEnabled == other.DelayedOffsideEnabled
            && IcingRule == other.IcingRule
            && PenaltyShotEnabled == other.PenaltyShotEnabled
            && GoaliePullAllowed == other.GoaliePullAllowed;
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(RegularPeriodCount);
        hash.Add(RegularPeriodLengthMinutes);
        hash.Add(OvertimeLengthMinutes);
        hash.Add(StopClock);
        hash.Add(OvertimeEnabled);
        hash.Add(ShootoutEnabled);
        hash.Add(OffsideEnabled);
        hash.Add(DelayedOffsideEnabled);
        hash.Add(IcingRule);
        hash.Add(PenaltyShotEnabled);
        hash.Add(GoaliePullAllowed);
        return hash.ToHashCode();
    }

    public static bool operator ==(HockeyMatchRules? left, HockeyMatchRules? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(HockeyMatchRules? left, HockeyMatchRules? right) => !(left == right);
}

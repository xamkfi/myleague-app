namespace Domain.ValueObjects.Football;

/// <summary>
/// Configurable match rules for hobby football. All timings and player counts live here
/// so competitions can run 5v5 / 7v7 / 11v11 and any half length without code changes.
/// </summary>
public class FootballMatchRules : IEquatable<FootballMatchRules>
{
    /// <summary>
    /// Number of regular halves (typically 2).
    /// </summary>
    public int NumberOfHalves { get; private set; }

    /// <summary>
    /// Duration in minutes of each regular half.
    /// </summary>
    public int HalfDurationMinutes { get; private set; }

    /// <summary>
    /// Players on the field per team, including the goalkeeper when required (5–11).
    /// </summary>
    public int PlayersOnField { get; private set; }

    /// <summary>
    /// Whether each team must have a goalkeeper in the starting lineup.
    /// </summary>
    public bool RequireGoalkeeper { get; private set; }

    /// <summary>
    /// Maximum substitutions per team. 0 means unlimited rolling substitutions.
    /// </summary>
    public int MaxSubstitutions { get; private set; }

    /// <summary>
    /// Whether at least one official is required before the match can start.
    /// Hobby default is false.
    /// </summary>
    public bool RequireOfficialsToStart { get; private set; }

    /// <summary>
    /// Whether extra time is allowed when the match is tied after regular halves.
    /// </summary>
    public bool AllowExtraTime { get; private set; }

    /// <summary>
    /// Number of extra-time halves (1–2). Only relevant when extra time is allowed.
    /// </summary>
    public int ExtraTimeHalfCount { get; private set; }

    /// <summary>
    /// Duration in minutes of each extra-time half.
    /// </summary>
    public int ExtraTimeHalfDurationMinutes { get; private set; }

    /// <summary>
    /// Whether a penalty shootout is allowed to resolve a tie.
    /// </summary>
    public bool AllowPenaltyShootout { get; private set; }

    private FootballMatchRules()
    {
        NumberOfHalves = 2;
        HalfDurationMinutes = 45;
        PlayersOnField = 11;
        RequireGoalkeeper = true;
        MaxSubstitutions = 0;
        RequireOfficialsToStart = false;
        AllowExtraTime = false;
        ExtraTimeHalfCount = 2;
        ExtraTimeHalfDurationMinutes = 15;
        AllowPenaltyShootout = false;
    }

    public FootballMatchRules(
        int numberOfHalves,
        int halfDurationMinutes,
        int playersOnField,
        bool requireGoalkeeper,
        int maxSubstitutions,
        bool requireOfficialsToStart,
        bool allowExtraTime,
        int extraTimeHalfCount,
        int extraTimeHalfDurationMinutes,
        bool allowPenaltyShootout)
    {
        if (numberOfHalves < 1 || numberOfHalves > 4)
            throw new ArgumentOutOfRangeException(nameof(numberOfHalves), "Number of halves must be between 1 and 4.");
        if (halfDurationMinutes < 1 || halfDurationMinutes > 60)
            throw new ArgumentOutOfRangeException(nameof(halfDurationMinutes), "Half duration must be between 1 and 60 minutes.");
        if (playersOnField < 5 || playersOnField > 11)
            throw new ArgumentOutOfRangeException(nameof(playersOnField), "Players on field must be between 5 and 11.");
        if (maxSubstitutions < 0 || maxSubstitutions > 99)
            throw new ArgumentOutOfRangeException(nameof(maxSubstitutions), "Max substitutions must be between 0 (unlimited) and 99.");
        if (allowExtraTime)
        {
            if (extraTimeHalfCount < 1 || extraTimeHalfCount > 2)
                throw new ArgumentOutOfRangeException(nameof(extraTimeHalfCount), "Extra-time half count must be 1 or 2.");
            if (extraTimeHalfDurationMinutes < 1 || extraTimeHalfDurationMinutes > 30)
                throw new ArgumentOutOfRangeException(nameof(extraTimeHalfDurationMinutes), "Extra-time half duration must be between 1 and 30 minutes.");
        }

        NumberOfHalves = numberOfHalves;
        HalfDurationMinutes = halfDurationMinutes;
        PlayersOnField = playersOnField;
        RequireGoalkeeper = requireGoalkeeper;
        MaxSubstitutions = maxSubstitutions;
        RequireOfficialsToStart = requireOfficialsToStart;
        AllowExtraTime = allowExtraTime;
        ExtraTimeHalfCount = allowExtraTime ? extraTimeHalfCount : 0;
        ExtraTimeHalfDurationMinutes = allowExtraTime ? extraTimeHalfDurationMinutes : 0;
        AllowPenaltyShootout = allowPenaltyShootout;
    }

    /// <summary>
    /// Hobby default: 2 × 45 min, 11v11, draws allowed, no extra time, unlimited substitutions.
    /// </summary>
    public static FootballMatchRules Default() =>
        new(
            numberOfHalves: 2,
            halfDurationMinutes: 45,
            playersOnField: 11,
            requireGoalkeeper: true,
            maxSubstitutions: 0,
            requireOfficialsToStart: false,
            allowExtraTime: false,
            extraTimeHalfCount: 2,
            extraTimeHalfDurationMinutes: 15,
            allowPenaltyShootout: false);

    /// <summary>
    /// Typical knockout defaults: extra time (2 × 15) plus penalty shootout.
    /// </summary>
    public static FootballMatchRules KnockoutDefault() =>
        new(
            numberOfHalves: 2,
            halfDurationMinutes: 45,
            playersOnField: 11,
            requireGoalkeeper: true,
            maxSubstitutions: 0,
            requireOfficialsToStart: false,
            allowExtraTime: true,
            extraTimeHalfCount: 2,
            extraTimeHalfDurationMinutes: 15,
            allowPenaltyShootout: true);

    public bool HasUnlimitedSubstitutions => MaxSubstitutions == 0;

    public int HalfDurationSeconds => HalfDurationMinutes * 60;

    public int ExtraTimeHalfDurationSeconds => ExtraTimeHalfDurationMinutes * 60;

    /// <summary>
    /// First period number used for extra time (regular halves + 1).
    /// </summary>
    public int ExtraTimeStartPeriodNumber => NumberOfHalves + 1;

    /// <summary>
    /// Period number used for the penalty shootout.
    /// </summary>
    public int PenaltyShootoutPeriodNumber =>
        NumberOfHalves + (AllowExtraTime ? ExtraTimeHalfCount : 0) + 1;

    /// <summary>
    /// Highest period number that can legally appear on an event for these rules.
    /// </summary>
    public int MaxPeriodNumber =>
        NumberOfHalves
        + (AllowExtraTime ? ExtraTimeHalfCount : 0)
        + (AllowPenaltyShootout ? 1 : 0);

    public bool IsExtraTimePeriod(int periodNumber) =>
        AllowExtraTime
        && periodNumber >= ExtraTimeStartPeriodNumber
        && periodNumber < PenaltyShootoutPeriodNumber;

    public bool IsPenaltyShootoutPeriod(int periodNumber) =>
        AllowPenaltyShootout && periodNumber == PenaltyShootoutPeriodNumber;

    public override bool Equals(object? obj) => Equals(obj as FootballMatchRules);

    public bool Equals(FootballMatchRules? other)
    {
        if (other is null)
            return false;

        return NumberOfHalves == other.NumberOfHalves
            && HalfDurationMinutes == other.HalfDurationMinutes
            && PlayersOnField == other.PlayersOnField
            && RequireGoalkeeper == other.RequireGoalkeeper
            && MaxSubstitutions == other.MaxSubstitutions
            && RequireOfficialsToStart == other.RequireOfficialsToStart
            && AllowExtraTime == other.AllowExtraTime
            && ExtraTimeHalfCount == other.ExtraTimeHalfCount
            && ExtraTimeHalfDurationMinutes == other.ExtraTimeHalfDurationMinutes
            && AllowPenaltyShootout == other.AllowPenaltyShootout;
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(NumberOfHalves);
        hash.Add(HalfDurationMinutes);
        hash.Add(PlayersOnField);
        hash.Add(RequireGoalkeeper);
        hash.Add(MaxSubstitutions);
        hash.Add(RequireOfficialsToStart);
        hash.Add(AllowExtraTime);
        hash.Add(ExtraTimeHalfCount);
        hash.Add(ExtraTimeHalfDurationMinutes);
        hash.Add(AllowPenaltyShootout);
        return hash.ToHashCode();
    }

    public static bool operator ==(FootballMatchRules? left, FootballMatchRules? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(FootballMatchRules? left, FootballMatchRules? right) => !(left == right);

    public override string ToString() =>
        $"{NumberOfHalves}x{HalfDurationMinutes}min, {PlayersOnField}v{PlayersOnField}, ET: {AllowExtraTime}, PSO: {AllowPenaltyShootout}";
}

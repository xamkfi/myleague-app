namespace Domain.ValueObjects.Hockey.Rules;

/// <summary>
/// Contact and physical play rules for a hockey competition.
/// </summary>
public class HockeyContactRules : IEquatable<HockeyContactRules>
{
    public bool BodyCheckingAllowed { get; private set; }
    public bool OpenIceHitsAllowed { get; private set; }
    public bool FightingAllowed { get; private set; }
    public bool AutomaticGameMisconductForFight { get; private set; }
    public bool StrictHeadContactRule { get; private set; }

    private HockeyContactRules() { }

    public HockeyContactRules(
        bool bodyCheckingAllowed,
        bool openIceHitsAllowed,
        bool fightingAllowed,
        bool automaticGameMisconductForFight,
        bool strictHeadContactRule)
    {
        BodyCheckingAllowed = bodyCheckingAllowed;
        OpenIceHitsAllowed = openIceHitsAllowed;
        FightingAllowed = fightingAllowed;
        AutomaticGameMisconductForFight = automaticGameMisconductForFight;
        StrictHeadContactRule = strictHeadContactRule;
    }

    public static HockeyContactRules Default() =>
        new(bodyCheckingAllowed: true, openIceHitsAllowed: false, fightingAllowed: false,
            automaticGameMisconductForFight: true, strictHeadContactRule: true);

    public override bool Equals(object? obj) => Equals(obj as HockeyContactRules);

    public bool Equals(HockeyContactRules? other)
    {
        if (other is null) return false;
        return BodyCheckingAllowed == other.BodyCheckingAllowed
            && OpenIceHitsAllowed == other.OpenIceHitsAllowed
            && FightingAllowed == other.FightingAllowed
            && AutomaticGameMisconductForFight == other.AutomaticGameMisconductForFight
            && StrictHeadContactRule == other.StrictHeadContactRule;
    }

    public override int GetHashCode() =>
        HashCode.Combine(BodyCheckingAllowed, OpenIceHitsAllowed, FightingAllowed,
            AutomaticGameMisconductForFight, StrictHeadContactRule);

    public static bool operator ==(HockeyContactRules? left, HockeyContactRules? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(HockeyContactRules? left, HockeyContactRules? right) => !(left == right);
}

namespace Domain.ValueObjects.Hockey.Matches;

/// <summary>
/// Represents a hockey score as a value object.
/// </summary>
public class HockeyScore : IEquatable<HockeyScore>
{
    public int HomeGoals { get; }
    public int AwayGoals { get; }

    public HockeyScore(int homeGoals, int awayGoals)
    {
        if (homeGoals < 0)
            throw new ArgumentOutOfRangeException(nameof(homeGoals), "Home goals cannot be negative.");
        if (awayGoals < 0)
            throw new ArgumentOutOfRangeException(nameof(awayGoals), "Away goals cannot be negative.");

        HomeGoals = homeGoals;
        AwayGoals = awayGoals;
    }

    public HockeyScore WithUpdatedHomeGoals(int homeGoals) => new(homeGoals, AwayGoals);

    public HockeyScore WithUpdatedAwayGoals(int awayGoals) => new(HomeGoals, awayGoals);

    public HockeyScore WithIncrementedHomeGoals() => new(HomeGoals + 1, AwayGoals);

    public HockeyScore WithIncrementedAwayGoals() => new(HomeGoals, AwayGoals + 1);

    public int Winner
    {
        get
        {
            if (HomeGoals > AwayGoals) return 1;
            if (AwayGoals > HomeGoals) return 2;
            return 0;
        }
    }

    public override bool Equals(object? obj) => Equals(obj as HockeyScore);

    public bool Equals(HockeyScore? other)
    {
        if (other is null) return false;
        return HomeGoals == other.HomeGoals && AwayGoals == other.AwayGoals;
    }

    public override int GetHashCode() => HashCode.Combine(HomeGoals, AwayGoals);

    public static bool operator ==(HockeyScore? left, HockeyScore? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(HockeyScore? left, HockeyScore? right) => !(left == right);

    public override string ToString() => $"{HomeGoals} - {AwayGoals}";
}

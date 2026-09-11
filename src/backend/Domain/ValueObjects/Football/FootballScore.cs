namespace Domain.ValueObjects.Football;

/// <summary>
/// Home/away score as a value object.
/// </summary>
public class FootballScore : IEquatable<FootballScore>
{
    public int HomeScore { get; }
    public int AwayScore { get; }

    public FootballScore(int homeScore, int awayScore)
    {
        if (homeScore < 0)
            throw new ArgumentException("Home score cannot be negative.", nameof(homeScore));
        if (awayScore < 0)
            throw new ArgumentException("Away score cannot be negative.", nameof(awayScore));

        HomeScore = homeScore;
        AwayScore = awayScore;
    }

    public FootballScore WithIncrementedHomeScore() => new(HomeScore + 1, AwayScore);
    public FootballScore WithIncrementedAwayScore() => new(HomeScore, AwayScore + 1);

    /// <summary>
    /// 1 for home, 2 for away, 0 for draw.
    /// </summary>
    public int Winner
    {
        get
        {
            if (HomeScore > AwayScore)
                return 1;
            if (AwayScore > HomeScore)
                return 2;
            return 0;
        }
    }

    public override bool Equals(object? obj) => Equals(obj as FootballScore);

    public bool Equals(FootballScore? other)
    {
        if (other is null)
            return false;
        return HomeScore == other.HomeScore && AwayScore == other.AwayScore;
    }

    public override int GetHashCode() => HashCode.Combine(HomeScore, AwayScore);

    public static bool operator ==(FootballScore? left, FootballScore? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(FootballScore? left, FootballScore? right) => !(left == right);

    public override string ToString() => $"{HomeScore} - {AwayScore}";
}

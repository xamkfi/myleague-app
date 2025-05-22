using System;

namespace Domain.ValueObjects.Floorball;

/// <summary>
/// Represents a floorball score as a value object
/// </summary>
public class Score : IEquatable<Score>
{
    /// <summary>
    /// Gets the home team score
    /// </summary>
    public int HomeScore { get; }
    
    /// <summary>
    /// Gets the away team score
    /// </summary>
    public int AwayScore { get; }

    /// <summary>
    /// Creates a new score
    /// </summary>
    public Score(int homeScore, int awayScore)
    {
        if (homeScore < 0)
            throw new ArgumentException("Home score cannot be negative", nameof(homeScore));
        
        if (awayScore < 0)
            throw new ArgumentException("Away score cannot be negative", nameof(awayScore));
        
        HomeScore = homeScore;
        AwayScore = awayScore;
    }

    /// <summary>
    /// Creates a new Score object with updated home score
    /// </summary>
    public Score WithUpdatedHomeScore(int newHomeScore)
    {
        return new Score(newHomeScore, AwayScore);
    }

    /// <summary>
    /// Creates a new Score object with updated away score
    /// </summary>
    public Score WithUpdatedAwayScore(int newAwayScore)
    {
        return new Score(HomeScore, newAwayScore);
    }

    /// <summary>
    /// Creates a new Score object with incremented home score
    /// </summary>
    public Score WithIncrementedHomeScore()
    {
        return new Score(HomeScore + 1, AwayScore);
    }

    /// <summary>
    /// Creates a new Score object with incremented away score
    /// </summary>
    public Score WithIncrementedAwayScore()
    {
        return new Score(HomeScore, AwayScore + 1);
    }

    /// <summary>
    /// Determines the winner (1 for home, 2 for away, 0 for draw)
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

    public override bool Equals(object? obj)
    {
        return Equals(obj as Score);
    }

    public bool Equals(Score? other)
    {
        if (other is null)
            return false;

        return HomeScore == other.HomeScore && 
               AwayScore == other.AwayScore;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(HomeScore, AwayScore);
    }

    public static bool operator ==(Score? left, Score? right)
    {
        if (ReferenceEquals(left, null))
            return ReferenceEquals(right, null);

        return left.Equals(right);
    }

    public static bool operator !=(Score? left, Score? right) => !(left == right);

    public override string ToString()
    {
        return $"{HomeScore} - {AwayScore}";
    }
} 

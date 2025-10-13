using Domain.Entities;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents the score for a period in a floorball match
/// </summary>
public class FloorballPeriodScore : BaseEntity
{
    /// <summary>
    /// Gets the ID of the match this period score belongs to
    /// </summary>
    public Guid MatchId { get; private set; }
    
    /// <summary>
    /// Gets the period number
    /// </summary>
    public int PeriodNumber { get; private set; }
    
    /// <summary>
    /// Gets the ID of the home team
    /// </summary>
    public Guid HomeTeamId { get; private set; }
    
    /// <summary>
    /// Gets the ID of the away team
    /// </summary>
    public Guid AwayTeamId { get; private set; }
    
    /// <summary>
    /// Gets the home team's score for this period
    /// </summary>
    public int HomeScore { get; private set; }
    
    /// <summary>
    /// Gets the away team's score for this period
    /// </summary>
    public int AwayScore { get; private set; }
    
    /// <summary>
    /// Gets whether the period is completed
    /// </summary>
    public bool IsCompleted { get; private set; }
    
    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballPeriodScore() : base()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the FloorballPeriodScore class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="periodNumber">The period number</param>
    /// <param name="homeTeamId">The ID of the home team</param>
    /// <param name="awayTeamId">The ID of the away team</param>
    public FloorballPeriodScore(Guid matchId, int periodNumber, Guid homeTeamId, Guid awayTeamId) : base()
    {
        if (periodNumber <= 0)
            throw new ArgumentException("Period number must be positive.", nameof(periodNumber));
        
        MatchId = matchId;
        PeriodNumber = periodNumber;
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;
        HomeScore = 0;
        AwayScore = 0;
        IsCompleted = false;
    }
    
    /// <summary>
    /// Updates the home team's score
    /// </summary>
    /// <param name="score">The new score</param>
    public void UpdateHomeScore(int score)
    {
        if (score < 0)
            throw new ArgumentException("Score cannot be negative.", nameof(score));
            
        HomeScore = score;
    }
    
    /// <summary>
    /// Updates the away team's score
    /// </summary>
    /// <param name="score">The new score</param>
    public void UpdateAwayScore(int score)
    {
        if (score < 0)
            throw new ArgumentException("Score cannot be negative.", nameof(score));
            
        AwayScore = score;
    }
    
    /// <summary>
    /// Updates both team scores
    /// </summary>
    /// <param name="homeScore">The home team's score</param>
    /// <param name="awayScore">The away team's score</param>
    public void UpdateScore(int homeScore, int awayScore)
    {
        if (homeScore < 0)
            throw new ArgumentException("Home score cannot be negative.", nameof(homeScore));
        if (awayScore < 0)
            throw new ArgumentException("Away score cannot be negative.", nameof(awayScore));
        
        HomeScore = homeScore;
        AwayScore = awayScore;
    }
    
    /// <summary>
    /// Increments the home team's score by 1
    /// </summary>
    public void IncrementHomeScore()
    {
        HomeScore++;
    }
    
    /// <summary>
    /// Increments the away team's score by 1
    /// </summary>
    public void IncrementAwayScore()
    {
        AwayScore++;
    }
    
    /// <summary>
    /// Decrements the home team's score by 1
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the home score is already 0</exception>
    public void DecrementHomeScore()
    {
        if (HomeScore <= 0)
            throw new InvalidOperationException("Cannot decrement home score below 0.");
        
        HomeScore--;
    }
    
    /// <summary>
    /// Decrements the away team's score by 1
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the away score is already 0</exception>
    public void DecrementAwayScore()
    {
        if (AwayScore <= 0)
            throw new InvalidOperationException("Cannot decrement away score below 0.");
        
        AwayScore--;
    }
    
    /// <summary>
    /// Marks the period as completed
    /// </summary>
    public void Complete()
    {
        IsCompleted = true;
    }
    
    /// <summary>
    /// Reopens the period (marks as not completed)
    /// </summary>
    public void Reopen()
    {
        IsCompleted = false;
    }
} 

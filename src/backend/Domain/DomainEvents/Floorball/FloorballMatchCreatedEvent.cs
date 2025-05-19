using Domain2.Entities.Floorball;

namespace Domain2.DomainEvents.Floorball;

/// <summary>
/// Event raised when a floorball match is created
/// </summary>
public class FloorballMatchCreatedEvent : IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the event
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the date and time when the event occurred
    /// </summary>
    public DateTime OccurredOn { get; }
    
    /// <summary>
    /// Gets the ID of the match
    /// </summary>
    public Guid MatchId { get; }
    
    /// <summary>
    /// Gets the ID of the season
    /// </summary>
    public Guid SeasonId { get; }
    
    /// <summary>
    /// Gets the ID of the home team
    /// </summary>
    public Guid HomeTeamId { get; }
    
    /// <summary>
    /// Gets the ID of the away team
    /// </summary>
    public Guid AwayTeamId { get; }
    
    /// <summary>
    /// Gets the scheduled date and time
    /// </summary>
    public DateTime ScheduledDateTime { get; }
    
    /// <summary>
    /// Gets the venue
    /// </summary>
    public string Venue { get; }
    
    /// <summary>
    /// Initializes a new instance of the FloorballMatchCreatedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="seasonId">The ID of the season</param>
    /// <param name="homeTeamId">The ID of the home team</param>
    /// <param name="awayTeamId">The ID of the away team</param>
    /// <param name="scheduledDateTime">The scheduled date and time</param>
    /// <param name="venue">The venue</param>
    public FloorballMatchCreatedEvent(
        Guid matchId,
        Guid seasonId,
        Guid homeTeamId,
        Guid awayTeamId,
        DateTime scheduledDateTime,
        string venue)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = matchId;
        SeasonId = seasonId;
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;
        ScheduledDateTime = scheduledDateTime;
        Venue = venue;
    }
    
    /// <summary>
    /// Initializes a new instance of the FloorballMatchCreatedEvent class from entity objects
    /// </summary>
    /// <param name="match">The match that was created</param>
    /// <param name="season">The season of the match</param>
    /// <param name="homeTeam">The home team</param>
    /// <param name="awayTeam">The away team</param>
    public FloorballMatchCreatedEvent(
        FloorballMatch match,
        FloorballSeason season,
        FloorballTeam homeTeam,
        FloorballTeam awayTeam)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = match.Id;
        SeasonId = season.Id;
        HomeTeamId = homeTeam.Id;
        AwayTeamId = awayTeam.Id;
        ScheduledDateTime = match.ScheduledDateTime;
        Venue = match.Venue;
    }
} 
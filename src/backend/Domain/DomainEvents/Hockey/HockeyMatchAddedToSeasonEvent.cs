using Domain.DomainEvents;

namespace Domain.DomainEvents.Hockey;

/// <summary>
/// Event raised when a match is added to a hockey season
/// </summary>
public class HockeyMatchAddedToSeasonEvent : IDomainEvent
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
    /// Gets the ID of the season
    /// </summary>
    public Guid SeasonId { get; }

    /// <summary>
    /// Gets the ID of the match
    /// </summary>
    public Guid MatchId { get; }
    
    /// <summary>
    /// Gets the ID of the home team
    /// </summary>
    public Guid HomeTeamId { get; }
    
    /// <summary>
    /// Gets the ID of the away team
    /// </summary>
    public Guid AwayTeamId { get; }

    /// <summary>
    /// Initializes a new instance of the HockeyMatchAddedToSeasonEvent class
    /// </summary>
    /// <param name="seasonId">The ID of the season</param>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="homeTeamId">The ID of the home team</param>
    /// <param name="awayTeamId">The ID of the away team</param>
    public HockeyMatchAddedToSeasonEvent(Guid seasonId, Guid matchId, Guid homeTeamId, Guid awayTeamId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        SeasonId = seasonId;
        MatchId = matchId;
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;
    }
} 
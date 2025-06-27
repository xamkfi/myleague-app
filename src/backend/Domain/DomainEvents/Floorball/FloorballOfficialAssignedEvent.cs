using Domain.Entities.Floorball;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when an official is assigned to a floorball match
/// </summary>
public class FloorballOfficialAssignedEvent : FloorballDomainEvent
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
    /// Gets the ID of the referee
    /// </summary>
    public Guid RefereeId { get; }

    /// <summary>
    /// Gets the ID of the official
    /// </summary>
    public Guid OfficialId { get; }

    /// <summary>
    /// Gets the role of the official
    /// </summary>
    public string OfficialRole { get; }

    /// <summary>
    /// Initializes a new instance of the FloorballOfficialAssignedEvent class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="refereeId">The ID of the referee</param>
    /// <param name="officialId">The ID of the official</param>
    /// <param name="officialRole">The role of the official</param>
    public FloorballOfficialAssignedEvent(Guid matchId, Guid refereeId, Guid officialId, string officialRole)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = matchId;
        RefereeId = refereeId;
        OfficialId = officialId;
        OfficialRole = officialRole;
    }

    /// <summary>
    /// Private constructor for EF Core serialization.
    /// </summary>
    private FloorballOfficialAssignedEvent()
    {
        OfficialRole = null!;
    }
} 

using Domain.Entities.Floorball;
using Domain.Entities.Common;
using Domain.DomainEvents;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a floorball team is registered
/// </summary>
public class FloorballTeamRegisteredEvent : IDomainEvent
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
    /// Gets the ID of the team
    /// </summary>
    public Guid TeamId { get; }

    /// <summary>
    /// Gets the name of the team
    /// </summary>
    public string TeamName { get; }

    /// <summary>
    /// Gets the division of the team
    /// </summary>
    public Guid DivisionId { get; }

    /// <summary>
    /// Gets the ID of the club the team belongs to
    /// </summary>
    public Guid ClubId { get; }

    /// <summary>
    /// Gets the home arena of the team
    /// </summary>
    public string HomeArena { get; }

    /// <summary>
    /// Gets the primary jersey color of the team
    /// </summary>
    public string PrimaryJerseyColor { get; }

    /// <summary>
    /// Gets the secondary jersey color of the team
    /// </summary>
    public string? SecondaryJerseyColor { get; }

    /// <summary>
    /// Initializes a new instance of the FloorballTeamRegisteredEvent class
    /// </summary>
    /// <param name="teamId">The ID of the team</param>
    /// <param name="teamName">The name of the team</param>
    /// <param name="division">The division of the team</param>
    /// <param name="clubId">The ID of the club the team belongs to</param>
    /// <param name="homeArena">The home arena of the team</param>
    /// <param name="primaryJerseyColor">The primary jersey color of the team</param>
    /// <param name="secondaryJerseyColor">The secondary jersey color of the team</param>
    public FloorballTeamRegisteredEvent(
        Guid teamId, 
        string teamName, 
        Guid divisionId, 
        Guid clubId, 
        string homeArena, 
        string primaryJerseyColor, 
        string? secondaryJerseyColor)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        TeamId = teamId;
        TeamName = teamName;
        DivisionId = divisionId;
        ClubId = clubId;
        HomeArena = homeArena;
        PrimaryJerseyColor = primaryJerseyColor;
        SecondaryJerseyColor = secondaryJerseyColor;
    }
}

using Domain.Entities.Floorball;
using Domain.Enums.Floorball;

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
    /// Gets the ID of the club
    /// </summary>
    public Guid ClubId { get; }

    /// <summary>
    /// Gets the name of the club
    /// </summary>
    public string ClubName { get; }

    /// <summary>
    /// Gets the division of the team
    /// </summary>
    public FloorballDivision Division { get; }

    /// <summary>
    /// Gets the home arena of the team
    /// </summary>
    public string HomeArena { get; }

    /// <summary>
    /// Initializes a new instance of the FloorballTeamRegisteredEvent class
    /// </summary>
    /// <param name="team">The team that was registered</param>
    public FloorballTeamRegisteredEvent(FloorballTeam team)
    {
        if (team == null)
        {
            throw new ArgumentNullException(nameof(team));
        }

        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        TeamId = team.Id;
        TeamName = team.Name;
        ClubId = team.Club.Id;
        ClubName = team.Club.Name;
        Division = team.Division;
        HomeArena = team.HomeArena;
    }
} 

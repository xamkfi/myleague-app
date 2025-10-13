using System;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a save made during a floorball match
/// </summary>
public class FloorballSave : FloorballMatchEvent
{
    /// <summary>
    /// Gets the ID of the goalie who made the save
    /// </summary>
    public Guid GoalieId { get; private set; }

    /// <summary>
    /// Gets whether the save was made in overtime
    /// </summary>
    public bool WasInOvertime { get; private set; }

    /// <summary>
    /// Gets whether the save was made in shootout
    /// </summary>
    public bool WasInShootout { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballSave() : base() { }

    /// <summary>
    /// Initializes a new instance of the FloorballSave class
    /// </summary>
    /// <param name="id">The unique identifier for the save</param>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="teamId">The ID of the team whose goalie made the save</param>
    /// <param name="goalieId">The ID of the goalie who made the save</param>
    /// <param name="periodNumber">The period number</param>
    /// <param name="timeInSeconds">The time in seconds when the save was made</param>
    /// <param name="wasInOvertime">Whether the save was made in overtime</param>
    /// <param name="wasInShootout">Whether the save was made in shootout</param>
    public FloorballSave(
        Guid id,
        Guid matchId,
        Guid teamId,
        Guid goalieId,
        int periodNumber,
        int timeInSeconds,
        bool wasInOvertime,
        bool wasInShootout)
        : base(matchId, teamId, periodNumber, timeInSeconds)
    {
        Id = id;
        GoalieId = goalieId;
        WasInOvertime = wasInOvertime;
        WasInShootout = wasInShootout;
    }
}

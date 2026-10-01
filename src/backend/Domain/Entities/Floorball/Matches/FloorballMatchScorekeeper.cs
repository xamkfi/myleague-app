namespace Domain.Entities.Floorball.Matches;

/// <summary>
/// A scorekeeper (toimitsija) assigned to a floorball match. Stores only the <see cref="PersonId"/>
/// because persons live in the common context.
/// </summary>
public class FloorballMatchScorekeeper
{
    /// <summary>
    /// Gets the ID of the person acting as scorekeeper.
    /// </summary>
    public Guid PersonId { get; private set; }

    private FloorballMatchScorekeeper()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FloorballMatchScorekeeper"/> class.
    /// </summary>
    /// <param name="personId">The person acting as scorekeeper</param>
    /// <exception cref="ArgumentException">Thrown when the person ID is empty</exception>
    public FloorballMatchScorekeeper(Guid personId)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person ID cannot be empty.", nameof(personId));

        PersonId = personId;
    }
}

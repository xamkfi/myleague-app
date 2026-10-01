namespace Domain.Entities.Hockey.Matches;

/// <summary>
/// A scorekeeper (toimitsija) assigned to a hockey match. Stores only the <see cref="PersonId"/>
/// because persons live in the common context.
/// </summary>
public class HockeyMatchScorekeeper
{
    /// <summary>
    /// Gets the ID of the person acting as scorekeeper.
    /// </summary>
    public Guid PersonId { get; private set; }

    private HockeyMatchScorekeeper()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HockeyMatchScorekeeper"/> class.
    /// </summary>
    /// <param name="personId">The person acting as scorekeeper</param>
    /// <exception cref="ArgumentException">Thrown when the person ID is empty</exception>
    public HockeyMatchScorekeeper(Guid personId)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person ID cannot be empty.", nameof(personId));

        PersonId = personId;
    }
}

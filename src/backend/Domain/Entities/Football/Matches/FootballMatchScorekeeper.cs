namespace Domain.Entities.Football.Matches;

/// <summary>
/// A scorekeeper (toimitsija) assigned to a football match. Stores only the <see cref="PersonId"/>
/// because persons live in the common context.
/// </summary>
public class FootballMatchScorekeeper
{
    /// <summary>
    /// Gets the ID of the person acting as scorekeeper.
    /// </summary>
    public Guid PersonId { get; private set; }

    private FootballMatchScorekeeper()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FootballMatchScorekeeper"/> class.
    /// </summary>
    /// <param name="personId">The person acting as scorekeeper</param>
    /// <exception cref="ArgumentException">Thrown when the person ID is empty</exception>
    public FootballMatchScorekeeper(Guid personId)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person ID cannot be empty.", nameof(personId));

        PersonId = personId;
    }
}

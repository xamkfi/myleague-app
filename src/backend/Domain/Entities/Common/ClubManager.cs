namespace Domain.Entities.Common;

/// <summary>
/// Links a person to a club they administer. A club can have multiple active managers
/// and a person can manage multiple clubs.
/// </summary>
public class ClubManager : BaseEntity
{
    /// <summary>
    /// Gets the ID of the person this club manager row belongs to (FK)
    /// </summary>
    public Guid PersonId { get; private set; }

    /// <summary>
    /// Gets the ID of the club this manager is responsible for (FK)
    /// </summary>
    public Guid ClubId { get; private set; }

    /// <summary>
    /// Gets whether the club manager link is currently active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private ClubManager()
    {
        Id = Guid.NewGuid();
        PersonId = Guid.Empty;
        ClubId = Guid.Empty;
        IsActive = true;
    }

    /// <summary>
    /// Initializes a new instance of the ClubManager class
    /// </summary>
    /// <param name="personId">The ID of the person this club manager row belongs to</param>
    /// <param name="clubId">The ID of the club this manager is responsible for</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public ClubManager(Guid personId, Guid clubId)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person ID cannot be empty.", nameof(personId));

        if (clubId == Guid.Empty)
            throw new ArgumentException("Club ID cannot be empty.", nameof(clubId));

        Id = Guid.NewGuid();
        PersonId = personId;
        ClubId = clubId;
        IsActive = true;
    }

    /// <summary>
    /// Updates the club manager's active status
    /// </summary>
    /// <param name="isActive">The new active status</param>
    public void UpdateActiveStatus(bool isActive)
    {
        IsActive = isActive;
    }
}

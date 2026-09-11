namespace Domain.Entities.Football.Teams;

/// <summary>
/// A football team manager profile attached to a Person.
/// </summary>
public class FootballTeamManager : BaseEntity
{
    public Guid PersonId { get; private set; }
    public Guid TeamId { get; private set; }
    public bool IsActive { get; private set; }

    private FootballTeamManager()
    {
        PersonId = Guid.Empty;
        TeamId = Guid.Empty;
        IsActive = true;
    }

    public FootballTeamManager(Guid personId, Guid teamId)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person ID cannot be empty.", nameof(personId));
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team ID cannot be empty.", nameof(teamId));

        PersonId = personId;
        TeamId = teamId;
        IsActive = true;
    }

    public void UpdateActiveStatus(bool isActive) => IsActive = isActive;

    public void UpdateTeam(Guid teamId)
    {
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team ID cannot be empty.", nameof(teamId));
        TeamId = teamId;
    }
}

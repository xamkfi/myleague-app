using Domain.Entities.Common;
using Domain.Enums.Hockey.Teams;

namespace Domain.Entities.Hockey.Teams;

/// <summary>
/// Staff member on a hockey team, linked to a <see cref="Person"/> via <see cref="PersonId"/>
/// (same pattern as floorball team manager). Replaces the narrow single-manager model.
/// </summary>
public class HockeyTeamStaff : BaseEntity
{
    public Guid PersonId { get; private set; }
    public Person Person { get; private set; } = null!;
    public Guid TeamId { get; private set; }
    public HockeyTeam Team { get; private set; } = null!;
    public Guid? CompetitionId { get; private set; }
    public HockeyTeamStaffRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }
    public bool IsActive => LeftAt is null;

    private HockeyTeamStaff() { }

    internal HockeyTeamStaff(Guid personId, Guid teamId, HockeyTeamStaffRole role, Guid? competitionId = null)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person id cannot be empty.", nameof(personId));
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));

        PersonId = personId;
        TeamId = teamId;
        Role = role;
        CompetitionId = competitionId;
        JoinedAt = DateTime.UtcNow;
    }

    internal void Leave()
    {
        if (LeftAt is not null)
            return;

        LeftAt = DateTime.UtcNow;
    }

    internal void UpdateRole(HockeyTeamStaffRole role) => Role = role;
}

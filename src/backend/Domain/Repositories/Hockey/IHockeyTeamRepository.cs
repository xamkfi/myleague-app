using Domain.Entities.Hockey.Teams;

namespace Domain.Repositories.Hockey;

/// <summary>
/// Repository for hockey teams.
/// </summary>
public interface IHockeyTeamRepository
{
    Task AddAsync(HockeyTeam team);

    Task<HockeyTeam?> GetByIdAsync(Guid id);
}

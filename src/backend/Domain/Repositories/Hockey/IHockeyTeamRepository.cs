using Domain.Entities.Hockey.Teams;

namespace Domain.Repositories.Hockey;

/// <summary>
/// Repository for hockey teams.
/// </summary>
public interface IHockeyTeamRepository
{
    Task AddAsync(HockeyTeam team);

    Task<HockeyTeam?> GetByIdAsync(Guid id);

    Task<IReadOnlyList<HockeyTeam>> GetAllAsync();

    Task<IReadOnlyList<HockeyTeam>> GetByClubIdAsync(Guid clubId);

    Task<bool> HasAnyForClubAsync(Guid clubId, CancellationToken cancellationToken = default);

    Task<bool> HasAnyForDivisionAsync(Guid divisionId, CancellationToken cancellationToken = default);
}

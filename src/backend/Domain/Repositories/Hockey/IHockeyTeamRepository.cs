using Domain.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;

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

    Task<IReadOnlyList<HockeyTeam>> GetByPlayerIdAsync(Guid playerId);

    Task<PagedResult<HockeyTeam>> GetPagedAsync(
        int page,
        int pageSize,
        string searchTerm = "",
        Guid? clubId = null,
        TeamCategory? teamCategory = null,
        CancellationToken cancellationToken = default);
}

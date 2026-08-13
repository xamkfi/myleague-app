using Domain.Common;
using Domain.Entities.Football.Teams;

namespace Domain.Repositories.Football;

/// <summary>
/// Repository for football team managers.
/// </summary>
public interface IFootballTeamManagerRepository
{
    Task<FootballTeamManager?> GetByIdAsync(Guid id);
    Task<FootballTeamManager?> GetByPersonIdAsync(Guid personId);
    Task<IEnumerable<FootballTeamManager>> GetAllAsync();
    Task<PagedResult<FootballTeamManager>> GetPagedAsync(
        int page,
        int pageSize,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<FootballTeamManager>> GetActiveAsync();
    Task AddAsync(FootballTeamManager teamManager);
    Task UpdateAsync(FootballTeamManager teamManager);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByPersonIdAsync(Guid personId);
}

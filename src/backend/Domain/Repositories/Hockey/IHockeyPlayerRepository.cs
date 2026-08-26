using Domain.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Teams;

namespace Domain.Repositories.Hockey;

/// <summary>
/// Repository for hockey players.
/// </summary>
public interface IHockeyPlayerRepository
{
    Task AddAsync(HockeyPlayer player);

    Task<HockeyPlayer?> GetByIdAsync(Guid id);

    Task<HockeyPlayer?> GetByPersonIdAsync(Guid personId);

    Task<bool> ExistsAsync(Guid id);

    Task DeleteAsync(Guid id);

    Task<PagedResult<HockeyPlayer>> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null,
        HockeyPosition? position = null,
        Guid? clubId = null,
        Guid? teamId = null,
        TeamCategory? teamCategory = null,
        CancellationToken cancellationToken = default);
}

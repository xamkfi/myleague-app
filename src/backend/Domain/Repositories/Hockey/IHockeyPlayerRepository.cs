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

    /// <summary>
    /// Returns true when the player has roster games, statistics, or a non-scheduled match appearance.
    /// </summary>
    Task<bool> HasCompetitionHistoryAsync(Guid playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes unused roster rows and deletes the player profile.
    /// </summary>
    Task DeleteUnusedProfileAsync(Guid playerId, CancellationToken cancellationToken = default);
}

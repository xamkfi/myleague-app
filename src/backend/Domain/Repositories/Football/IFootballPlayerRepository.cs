using Domain.Common;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;

namespace Domain.Repositories.Football;

/// <summary>
/// Repository for football players.
/// </summary>
public interface IFootballPlayerRepository
{
    Task<FootballPlayer?> GetByIdAsync(Guid id);
    Task<FootballPlayer?> GetByPersonIdAsync(Guid personId);
    Task<IEnumerable<FootballPlayer>> GetAllAsync();
    Task<PagedResult<(FootballPlayer Player, FootballTeam? Team)>> GetPagedWithTeamsAsync(
        int page,
        int pageSize,
        bool? isActive = null,
        FootballPosition? position = null,
        Guid? teamId = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
    Task<PagedResult<FootballPlayer>> GetPagedAsync(
        int page,
        int pageSize,
        bool? isActive = null,
        FootballPosition? position = null,
        Guid? teamId = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(
        bool? isActive = null,
        FootballPosition? position = null,
        Guid? teamId = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<FootballPlayer>> GetByTeamIdAsync(Guid teamId);
    Task<IEnumerable<FootballPlayer>> GetActiveByPositionAsync(FootballPosition position);
    Task AddAsync(FootballPlayer player);
    Task UpdateAsync(FootballPlayer player);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<FootballPlayer>> SearchByNameAsync(string searchTerm);
    Task<Dictionary<Guid, FootballPlayer>> GetByPersonIdsAsync(IEnumerable<Guid> personIds, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Returns true when the player has roster games, statistics, match events, or a non-scheduled match appearance.
    /// </summary>
    Task<bool> HasCompetitionHistoryAsync(Guid playerId, CancellationToken cancellationToken = default);
}

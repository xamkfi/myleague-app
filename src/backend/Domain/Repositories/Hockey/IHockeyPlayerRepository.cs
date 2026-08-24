using Domain.Entities.Hockey.Teams;

namespace Domain.Repositories.Hockey;

/// <summary>
/// Repository for hockey players.
/// </summary>
public interface IHockeyPlayerRepository
{
    Task AddAsync(HockeyPlayer player);

    Task<HockeyPlayer?> GetByIdAsync(Guid id);

    Task<HockeyPlayer?> GetByPersonIdAsync(Guid personId);

    /// <summary>
    /// Returns true when the player has roster games, statistics, or a non-scheduled match appearance.
    /// </summary>
    Task<bool> HasCompetitionHistoryAsync(Guid playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes unused roster rows and deletes the player profile.
    /// </summary>
    Task DeleteUnusedProfileAsync(Guid playerId, CancellationToken cancellationToken = default);
}

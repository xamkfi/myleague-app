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
}

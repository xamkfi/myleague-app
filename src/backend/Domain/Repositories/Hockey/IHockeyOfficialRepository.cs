using Domain.Entities.Hockey.Teams;

namespace Domain.Repositories.Hockey;

/// <summary>
/// Repository for hockey official profiles.
/// </summary>
public interface IHockeyOfficialRepository
{
    Task AddAsync(HockeyOfficial official);

    Task<HockeyOfficial?> GetByIdAsync(Guid id);

    Task<HockeyOfficial?> GetByPersonIdAsync(Guid personId);

    Task<IReadOnlyList<HockeyOfficial>> GetAllAsync(bool? isActive = null);

    Task<bool> ExistsAsync(Guid id);

    Task<bool> IsAssignedToAnyMatchAsync(Guid officialId, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

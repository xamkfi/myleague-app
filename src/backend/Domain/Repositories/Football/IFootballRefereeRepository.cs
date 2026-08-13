using Domain.Common;
using Domain.Entities.Football.Teams;

namespace Domain.Repositories.Football;

/// <summary>
/// Repository for football referees.
/// </summary>
public interface IFootballRefereeRepository
{
    Task<FootballReferee?> GetByIdAsync(Guid id);
    Task<IEnumerable<FootballReferee>> GetAllAsync();
    Task<PagedResult<FootballReferee>> GetPagedAsync(
        int page,
        int pageSize,
        bool? isActive = null,
        string? searchTerm = null,
        int? licenseExpiringWithinDays = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<FootballReferee>> GetActiveAsync();
    Task<IEnumerable<FootballReferee>> GetByMatchIdAsync(Guid matchId);
    Task AddAsync(FootballReferee referee);
    Task UpdateAsync(FootballReferee referee);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<FootballReferee>> SearchByNameAsync(string searchTerm);
    Task<bool> ExistsAsync(Guid id);
}

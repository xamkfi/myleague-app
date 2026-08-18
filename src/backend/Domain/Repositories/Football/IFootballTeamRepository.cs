using Domain.Common;
using Domain.Entities.Football.Teams;

namespace Domain.Repositories.Football;

/// <summary>
/// Repository for football teams.
/// </summary>
public interface IFootballTeamRepository
{
    Task<FootballTeam?> GetByIdAsync(Guid? id);
    Task<FootballTeam?> GetByNameAsync(string name);
    Task<IEnumerable<FootballTeam>> GetAllAsync();
    Task<PagedResult<FootballTeam>> GetPagedAsync(
        int page,
        int pageSize,
        string searchTerm = "",
        Guid? clubId = null,
        Guid? divisionId = null,
        IReadOnlyCollection<Domain.Enums.Common.TeamCategory>? teamCategories = null,
        CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(
        Guid? clubId = null,
        Guid? divisionId = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<FootballTeam?>> GetByClubIdAsync(Guid clubId);
    Task<IEnumerable<FootballTeam>> GetByPlayerIdAsync(Guid playerId);
    Task<IEnumerable<FootballTeam>> GetByDivisionAsync(Guid divisionId);
    Task<IEnumerable<FootballTeam>> GetByCompetitionIdAsync(Guid competitionId);
    Task AddAsync(FootballTeam team);
    Task UpdateAsync(FootballTeam team);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<FootballTeam>> SearchByNameAsync(string searchTerm, int count, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<FootballTeam>> GetTeamsByPlayerIdAsync(Guid playerId);
    Task<Dictionary<Guid, FootballTeam>> GetTeamsByPlayerIdsAsync(IEnumerable<Guid> playerIds, CancellationToken cancellationToken = default);
    Task<IEnumerable<FootballTeam>> GetByNameFilterAsync(string? nameFilter, CancellationToken cancellationToken);
    Task<PagedResult<FootballTeam>> GetAllTeamsWithoutRosterAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        Domain.Enums.Common.TeamCategory? teamCategory = null,
        CancellationToken cancellationToken = default);
}

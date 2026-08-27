using Domain.Common;
using Domain.Entities.Football.Competitions;

namespace Domain.Repositories.Football;

/// <summary>
/// Repository for football competitions (seasons).
/// </summary>
public interface IFootballCompetitionRepository
{
    Task<FootballCompetition?> GetByIdAsync(Guid? id);
    Task<FootballCompetition?> GetByNameAsync(string name);
    Task<IEnumerable<FootballCompetition>> GetAllAsync();
    Task<IEnumerable<FootballCompetition>> GetActiveAsync();
    Task<IEnumerable<FootballCompetition>> GetCompletedAsync();
    Task<IEnumerable<FootballCompetition>> GetByDivisionAsync(Guid divisionId);
    Task<IEnumerable<FootballCompetition>> GetByTeamIdAsync(Guid teamId);
    Task AddAsync(FootballCompetition competition);
    Task UpdateAsync(FootballCompetition competition);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IReadOnlyList<FootballSeasonDateSummary>> GetSeasonDateSummariesAsync(
        CancellationToken cancellationToken = default);
    Task<PagedResult<FootballSeason>> GetSeasonsPagedAsync(
        int page,
        int pageSize,
        int? startYear,
        int? endYear,
        Domain.Enums.Common.TeamCategory? teamCategory = null,
        CancellationToken cancellationToken = default);

    Task<FootballSeason?> GetSeasonWithContentBlocksAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<FootballSeason?> GetFeaturedSeasonWithContentBlocksAsync(
        int? startYear,
        int? endYear,
        CancellationToken cancellationToken = default);
}

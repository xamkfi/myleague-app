using Domain.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Enums.Football;

namespace Domain.Repositories.Football;

/// <summary>
/// Repository for football matches.
/// </summary>
public interface IFootballMatchRepository
{
    Task<FootballMatch?> GetByIdAsync(Guid id);
    Task<IEnumerable<FootballMatch>> GetAllAsync();
    Task<PagedResult<FootballMatch>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? competitionId = null,
        Guid? teamId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        FootballMatchStatus? status = null,
        string sortOrder = "desc",
        string? searchQuery = null,
        Guid? tournamentGroupId = null,
        FootballCompetitionType? competitionType = null,
        Domain.Enums.Common.TeamCategory? teamCategory = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<FootballMatch>> GetByCompetitionIdAsync(Guid competitionId);
    Task<IEnumerable<FootballMatch>> GetByTournamentGroupAsync(
        Guid tournamentGroupId,
        FootballMatchStatus? status = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<FootballMatch>> GetByTeamIdAsync(Guid teamId);
    Task<IEnumerable<FootballMatch>> GetUpcomingByTeamIdAsync(Guid teamId, int count = 5);
    Task<IEnumerable<FootballMatch>> GetPastByTeamIdAsync(Guid teamId, int count = 5);
    Task<IEnumerable<FootballMatch>> GetByStatusAsync(FootballMatchStatus status);
    Task<IEnumerable<FootballMatch>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<FootballMatch>> GetTodaysMatchesByTeamAsync(Guid teamId, CancellationToken cancellationToken);
    Task AddAsync(FootballMatch match);
    Task UpdateAsync(FootballMatch match);
    Task DeleteAsync(Guid id);
    Task<int> DeleteAllByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id);
    void MarkEventAsAdded(FootballMatchEvent matchEvent);
    Task<IEnumerable<FootballMatch>> GetLastCompletedByTeamAsync(Guid teamId, Guid? competitionId = null, int count = 5);
}

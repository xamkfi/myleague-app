using Domain.Entities.Football.Competitions;

namespace Domain.Repositories.Football;

/// <summary>
/// Repository for football tournaments.
/// </summary>
public interface IFootballTournamentRepository
{
    Task<FootballTournament?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<FootballTournament?> GetByIdWithGroupsAsync(Guid id, CancellationToken ct = default);
    Task<FootballTournament?> GetByIdWithGroupsAsNoTrackingAsync(Guid id, CancellationToken ct = default);
    Task<FootballTournamentGroup?> GetGroupByIdAsync(Guid groupId, CancellationToken ct = default);
    Task AddGroupAsync(FootballTournamentGroup group, CancellationToken ct = default);
    Task AddGroupTeamAsync(FootballTournamentGroupTeam groupTeam, CancellationToken ct = default);
    Task RemoveGroupTeamAsync(FootballTournamentGroupTeam groupTeam, CancellationToken ct = default);
    Task AddCompetitionTeamAsync(Guid competitionId, Guid teamId, CancellationToken ct = default);
    Task RemoveCompetitionTeamAsync(Guid competitionId, Guid teamId, CancellationToken ct = default);
    Task<bool> ExistsCompetitionTeamAsync(Guid competitionId, Guid teamId, CancellationToken ct = default);
    Task<List<FootballTournament>> GetAllAsync(
        Domain.Enums.Common.TeamCategory? teamCategory = null,
        CancellationToken ct = default);
    Task<List<FootballTournament>> GetActiveAsync(
        Domain.Enums.Common.TeamCategory? teamCategory = null,
        CancellationToken ct = default);
    Task AddAsync(FootballTournament tournament, CancellationToken ct = default);
    Task UpdateAsync(FootballTournament tournament, CancellationToken ct = default);
    Task DeleteAsync(FootballTournament tournament, CancellationToken ct = default);
}

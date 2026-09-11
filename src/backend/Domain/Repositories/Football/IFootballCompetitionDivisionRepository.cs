using Domain.Entities.Football.Competitions;

namespace Domain.Repositories.Football;

/// <summary>
/// Repository for competition-division links and team memberships.
/// </summary>
public interface IFootballCompetitionDivisionRepository
{
    Task<IEnumerable<FootballCompetitionDivision>> GetCompetitionDivisionsAsync(Guid competitionId);
    Task<FootballCompetitionDivision?> GetCompetitionDivisionAsync(Guid competitionId, Guid divisionId);
    Task<IEnumerable<FootballCompetition>> GetCompetitionsByDivisionAsync(Guid divisionId);
    Task<IEnumerable<FootballCompetition>> GetCompetitionsByTeamAsync(Guid teamId);
    Task<IEnumerable<FootballCompetitionDivisionTeam>> GetCompetitionDivisionTeamsAsync(Guid competitionId);
    Task AddCompetitionDivisionAsync(Guid competitionId, Guid divisionId);
    Task RemoveCompetitionDivisionAsync(Guid competitionId, Guid divisionId);
    Task AddTeamToCompetitionDivisionAsync(Guid competitionId, Guid divisionId, Guid teamId);
    Task RemoveTeamFromCompetitionDivisionAsync(Guid competitionId, Guid divisionId, Guid teamId);
}

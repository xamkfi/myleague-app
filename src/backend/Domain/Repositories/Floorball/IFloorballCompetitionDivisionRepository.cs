using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;

namespace Domain.Repositories.Floorball;

/// <summary>
/// Repository for managing competition-division links and team memberships in floorball
/// </summary>
public interface IFloorballCompetitionDivisionRepository
{
    Task<IEnumerable<FloorballCompetitionDivision>> GetCompetitionDivisionsAsync(Guid competitionId);
    Task<FloorballCompetitionDivision?> GetCompetitionDivisionAsync(Guid competitionId, Guid divisionId);
    Task<IEnumerable<FloorballCompetition>> GetCompetitionsByDivisionAsync(Guid divisionId);
    Task<IEnumerable<FloorballCompetition>> GetCompetitionsByTeamAsync(Guid teamId);
    Task<IEnumerable<FloorballCompetitionDivisionTeam>> GetCompetitionDivisionTeamsAsync(Guid competitionId);

    Task AddCompetitionDivisionAsync(Guid competitionId, Guid divisionId);
    Task RemoveCompetitionDivisionAsync(Guid competitionId, Guid divisionId);

    Task AddTeamToCompetitionDivisionAsync(Guid competitionId, Guid divisionId, Guid teamId);
    Task RemoveTeamFromCompetitionDivisionAsync(Guid competitionId, Guid divisionId, Guid teamId);
}

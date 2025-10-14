using Domain.Entities.Floorball;

namespace Domain.Repositories.Floorball;

/// <summary>
/// Repository for managing season-division links and team memberships in floorball
/// </summary>
public interface IFloorballSeasonDivisionRepository
{
    Task<IEnumerable<FloorballSeasonDivision>> GetSeasonDivisionsAsync(Guid seasonId);
    Task<FloorballSeasonDivision?> GetSeasonDivisionAsync(Guid seasonId, Guid divisionId);
    Task<IEnumerable<FloorballSeason>> GetSeasonsByDivisionAsync(Guid divisionId);
    Task<IEnumerable<FloorballSeason>> GetSeasonsByTeamAsync(Guid teamId);

    Task AddSeasonDivisionAsync(Guid seasonId, Guid divisionId);
    Task RemoveSeasonDivisionAsync(Guid seasonId, Guid divisionId);

    Task AddTeamToSeasonDivisionAsync(Guid seasonId, Guid divisionId, Guid teamId);
    Task RemoveTeamFromSeasonDivisionAsync(Guid seasonId, Guid divisionId, Guid teamId);
}



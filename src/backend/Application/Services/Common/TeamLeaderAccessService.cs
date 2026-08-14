using Domain.Repositories.Floorball;
using Domain.Repositories.Football;

namespace Application.Services.Common;

/// <summary>
/// Default implementation of <see cref="ITeamLeaderAccessService"/> backed by the
/// floorball and football team manager repositories.
/// </summary>
public class TeamLeaderAccessService : ITeamLeaderAccessService
{
    private readonly IFloorballTeamManagerRepository _floorballTeamManagerRepository;
    private readonly IFootballTeamManagerRepository _footballTeamManagerRepository;

    /// <summary>
    /// Initializes a new instance of the TeamLeaderAccessService class
    /// </summary>
    /// <param name="floorballTeamManagerRepository">The floorball team manager repository</param>
    /// <param name="footballTeamManagerRepository">The football team manager repository</param>
    public TeamLeaderAccessService(
        IFloorballTeamManagerRepository floorballTeamManagerRepository,
        IFootballTeamManagerRepository footballTeamManagerRepository)
    {
        _floorballTeamManagerRepository = floorballTeamManagerRepository;
        _footballTeamManagerRepository = footballTeamManagerRepository;
    }

    /// <inheritdoc />
    public Task<bool> CanManageFloorballTeamAsync(Guid personId, Guid teamId)
    {
        return _floorballTeamManagerRepository.IsActiveManagerOfTeamAsync(personId, teamId);
    }

    /// <inheritdoc />
    public Task<bool> CanManageFootballTeamAsync(Guid personId, Guid teamId)
    {
        return _footballTeamManagerRepository.IsActiveManagerOfTeamAsync(personId, teamId);
    }
}

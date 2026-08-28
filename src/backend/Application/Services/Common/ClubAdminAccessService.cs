using Domain.Entities.Floorball;
using Domain.Entities.Football.Teams;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;

namespace Application.Services.Common;

/// <summary>
/// Default implementation of <see cref="IClubAdminAccessService"/> backed by the club manager
/// repository. Team access is resolved by looking up the team's owning club.
/// </summary>
public class ClubAdminAccessService : IClubAdminAccessService
{
    private readonly IClubManagerRepository _clubManagerRepository;
    private readonly IFloorballTeamRepository _floorballTeamRepository;
    private readonly IFootballTeamRepository _footballTeamRepository;
    private readonly IHockeyTeamRepository _hockeyTeamRepository;

    /// <summary>
    /// Initializes a new instance of the ClubAdminAccessService class
    /// </summary>
    /// <param name="clubManagerRepository">The club manager repository</param>
    /// <param name="floorballTeamRepository">The floorball team repository</param>
    /// <param name="footballTeamRepository">The football team repository</param>
    /// <param name="hockeyTeamRepository">The hockey team repository</param>
    public ClubAdminAccessService(
        IClubManagerRepository clubManagerRepository,
        IFloorballTeamRepository floorballTeamRepository,
        IFootballTeamRepository footballTeamRepository,
        IHockeyTeamRepository hockeyTeamRepository)
    {
        _clubManagerRepository = clubManagerRepository;
        _floorballTeamRepository = floorballTeamRepository;
        _footballTeamRepository = footballTeamRepository;
        _hockeyTeamRepository = hockeyTeamRepository;
    }

    /// <inheritdoc />
    public Task<bool> CanManageClubAsync(Guid personId, Guid clubId)
    {
        return _clubManagerRepository.IsActiveManagerOfClubAsync(personId, clubId);
    }

    /// <inheritdoc />
    public async Task<bool> CanManageFloorballTeamAsync(Guid personId, Guid teamId)
    {
        FloorballTeam? team = await _floorballTeamRepository.GetByIdAsync(teamId);
        if (team == null)
        {
            return false;
        }

        return await _clubManagerRepository.IsActiveManagerOfClubAsync(personId, team.ClubId);
    }

    /// <inheritdoc />
    public async Task<bool> CanManageFootballTeamAsync(Guid personId, Guid teamId)
    {
        FootballTeam? team = await _footballTeamRepository.GetByIdAsync(teamId);
        if (team == null)
        {
            return false;
        }

        return await _clubManagerRepository.IsActiveManagerOfClubAsync(personId, team.ClubId);
    }

    /// <inheritdoc />
    public async Task<bool> CanManageHockeyTeamAsync(Guid personId, Guid teamId)
    {
        HockeyTeam? team = await _hockeyTeamRepository.GetByIdAsync(teamId);
        if (team == null)
        {
            return false;
        }

        return await _clubManagerRepository.IsActiveManagerOfClubAsync(personId, team.ClubId);
    }
}

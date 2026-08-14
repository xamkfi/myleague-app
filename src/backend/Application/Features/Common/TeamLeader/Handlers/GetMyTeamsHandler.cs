using Application.Common;
using Application.Features.Common.TeamLeader.DTOs;
using Application.Features.Common.TeamLeader.Queries;
using Domain.Entities.Floorball;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.TeamLeader.Handlers;

/// <summary>
/// Handler that resolves all floorball and football teams the person actively manages
/// via the team manager link entities.
/// </summary>
public class GetMyTeamsHandler : IRequestHandler<GetMyTeamsQuery, Result<IEnumerable<TeamLeaderTeamDto>>>
{
    private readonly IFloorballTeamManagerRepository _floorballTeamManagerRepository;
    private readonly IFootballTeamManagerRepository _footballTeamManagerRepository;
    private readonly IFloorballTeamRepository _floorballTeamRepository;
    private readonly IFootballTeamRepository _footballTeamRepository;
    private readonly ILogger<GetMyTeamsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetMyTeamsHandler class
    /// </summary>
    public GetMyTeamsHandler(
        IFloorballTeamManagerRepository floorballTeamManagerRepository,
        IFootballTeamManagerRepository footballTeamManagerRepository,
        IFloorballTeamRepository floorballTeamRepository,
        IFootballTeamRepository footballTeamRepository,
        ILogger<GetMyTeamsHandler> logger)
    {
        _floorballTeamManagerRepository = floorballTeamManagerRepository;
        _footballTeamManagerRepository = footballTeamManagerRepository;
        _floorballTeamRepository = floorballTeamRepository;
        _footballTeamRepository = footballTeamRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetMyTeamsQuery request
    /// </summary>
    public async Task<Result<IEnumerable<TeamLeaderTeamDto>>> Handle(GetMyTeamsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            List<TeamLeaderTeamDto> teams = new();

            IEnumerable<FloorballTeamManager> floorballManagerRows =
                await _floorballTeamManagerRepository.GetAllByPersonIdAsync(request.PersonId);
            foreach (FloorballTeamManager managerRow in floorballManagerRows.Where(m => m.IsActive))
            {
                FloorballTeam? team = await _floorballTeamRepository.GetByIdAsync(managerRow.TeamId);
                if (team != null)
                {
                    teams.Add(new TeamLeaderTeamDto(
                        "floorball",
                        team.Id,
                        team.Name,
                        team.ShortName,
                        team.LogoUrl?.ToString()));
                }
            }

            IEnumerable<FootballTeamManager> footballManagerRows =
                await _footballTeamManagerRepository.GetAllByPersonIdAsync(request.PersonId);
            foreach (FootballTeamManager managerRow in footballManagerRows.Where(m => m.IsActive))
            {
                FootballTeam? team = await _footballTeamRepository.GetByIdAsync(managerRow.TeamId);
                if (team != null)
                {
                    teams.Add(new TeamLeaderTeamDto(
                        "football",
                        team.Id,
                        team.Name,
                        team.ShortName,
                        team.LogoUrl?.ToString()));
                }
            }

            return Result<IEnumerable<TeamLeaderTeamDto>>.Success(teams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teams for person {PersonId}", request.PersonId);
            return Result<IEnumerable<TeamLeaderTeamDto>>.Failure("An error occurred while retrieving the teams.");
        }
    }
}

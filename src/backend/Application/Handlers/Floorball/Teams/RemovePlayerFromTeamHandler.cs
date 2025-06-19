using Application.Commands.Floorball.Team;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;

namespace Application.Handlers.Floorball.Teams;

/// <summary>
/// Handler for removing a player from a floorball team
/// </summary>
public class RemovePlayerFromTeamHandler : IRequestHandler<RemovePlayerFromTeamCommand, Result<FloorballTeamDto>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IFloorballUnitOfWork _floorballUnitOfWork;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemovePlayerFromTeamHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the RemovePlayerFromTeamHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public RemovePlayerFromTeamHandler(
        IFloorballTeamRepository teamRepository,
        IClubRepository clubRepository,
        IFloorballUnitOfWork floorballUnitOfWork,
        IUnitOfWork unitOfWork,
        ILogger<RemovePlayerFromTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _floorballUnitOfWork = floorballUnitOfWork;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the RemovePlayerFromTeamCommand request
    /// </summary>
    /// <param name="request">The command containing player and team information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated team as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballTeamDto>> Handle(RemovePlayerFromTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the team
            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team not found with ID: {TeamId}", request.TeamId);
                return Result<FloorballTeamDto>.Failure($"Team with ID {request.TeamId} not found.");
            }

            _logger.LogInformation("Removing player {PlayerId} from team {TeamId}", request.PlayerId, request.TeamId);
            team.RemovePlayer(request.PlayerId);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _floorballUnitOfWork.SaveChangesAsync(cancellationToken);

            // Load the club for the team
            Club? club = await _clubRepository.GetByIdAsync(team.ClubId);
            if (club == null)
            {
                _logger.LogWarning("Club with ID {ClubId} not found for team {TeamId}", team.ClubId, team.Id);
                return Result<FloorballTeamDto>.Failure("Associated club not found");
            }

            FloorballTeamDto teamDto = FloorballTeamMapper.ToDto(team, club);
            _logger.LogInformation("Successfully removed player {PlayerId} from team {TeamId}", request.PlayerId, request.TeamId);

            return Result<FloorballTeamDto>.Success(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing player {PlayerId} from team {TeamId}", request.PlayerId, request.TeamId);
            return Result<FloorballTeamDto>.Failure("An error occurred while removing the player from the team.");
        }
    }
} 

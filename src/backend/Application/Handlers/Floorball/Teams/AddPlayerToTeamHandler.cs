using Application.Commands.Floorball.Team;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;

namespace Application.Handlers.Floorball.Teams;

/// <summary>
/// Handler for adding a player to a floorball team
/// </summary>
public class AddPlayerToTeamHandler : IRequestHandler<AddPlayerToTeamCommand, Result<FloorballTeamDto>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddPlayerToTeamHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the AddPlayerToTeamHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public AddPlayerToTeamHandler(
        IFloorballTeamRepository teamRepository,
        IFloorballPlayerRepository playerRepository,
        IUnitOfWork unitOfWork,
        ILogger<AddPlayerToTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the AddPlayerToTeamCommand request
    /// </summary>
    /// <param name="request">The command containing player and team information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated team as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballTeamDto>> Handle(AddPlayerToTeamCommand request, CancellationToken cancellationToken)
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

            // Get the player
            FloorballPlayer? player = await _playerRepository.GetByIdAsync(request.PlayerId);
            if (player == null)
            {
                _logger.LogWarning("Player not found with ID: {PlayerId}", request.PlayerId);
                return Result<FloorballTeamDto>.Failure($"Player with ID {request.PlayerId} not found.");
            }

            _logger.LogInformation("Adding player {PlayerId} to team {TeamId}", request.PlayerId, request.TeamId);
            team.AddPlayer(player, request.Position, request.JerseyNumber);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballTeamDto teamDto = FloorballTeamMapper.ToDto(team);
            _logger.LogInformation("Successfully added player {PlayerId} to team {TeamId}", request.PlayerId, request.TeamId);

            return Result<FloorballTeamDto>.Success(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding player {PlayerId} to team {TeamId}", request.PlayerId, request.TeamId);
            return Result<FloorballTeamDto>.Failure("An error occurred while adding the player to the team.");
        }
    }
} 

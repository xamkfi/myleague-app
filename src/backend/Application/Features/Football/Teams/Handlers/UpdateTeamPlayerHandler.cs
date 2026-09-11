using Application.Features.Football.Teams.Commands;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Application.Features.Football.Teams.Mappings;
using Application.Features.Football.Players.Mappings;
using Application.Features.Football.Referees.Mappings;
using Application.Features.Football.TeamManagers.Mappings;
using Application.Common;
using Domain.Entities.Football.Teams;
using Domain.Entities.Common;
using Domain.Repositories.Football;
using Domain.ValueObjects.Football;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;
using System.Linq;

namespace Application.Features.Football.Teams.Handlers;

/// <summary>
/// Handler for updating a player's information within a football team
/// </summary>
public class UpdateTeamPlayerHandler : IRequestHandler<UpdateTeamPlayerCommand, Result<FootballTeamPlayerDto>>
{
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateTeamPlayerHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateTeamPlayerHandler class
    /// </summary>
    /// <param name="teamRepository">The football team repository</param>
    /// <param name="playerRepository">The football player repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateTeamPlayerHandler(
        IFootballTeamRepository teamRepository,
        IFootballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<UpdateTeamPlayerHandler> logger)
    {
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateTeamPlayerCommand request
    /// </summary>
    /// <param name="request">The command containing updated player information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated team player as a DTO wrapped in a Result</returns>
    public async Task<Result<FootballTeamPlayerDto>> Handle(UpdateTeamPlayerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the team
            FootballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team not found with ID: {TeamId}", request.TeamId);
                return Result<FootballTeamPlayerDto>.Failure($"Team with ID {request.TeamId} not found.");
            }

            // Check if player exists in the team roster
            FootballTeamPlayer? teamPlayer = team.Roster.FirstOrDefault(p => p.PlayerId == request.PlayerId);
            if (teamPlayer == null)
            {
                _logger.LogWarning("Player {PlayerId} not found in team {TeamId} roster", request.PlayerId, request.TeamId);
                return Result<FootballTeamPlayerDto>.Failure($"Player with ID {request.PlayerId} is not in the team roster.");
            }

            // Check if player with given jersey number exists
            if (team.Roster.Any(p => p.JerseyNumber == request.JerseyNumber && p.PlayerId != request.PlayerId))
            {
                _logger.LogWarning("Jersey number {number} is already in use", teamPlayer.JerseyNumber);
                return Result<FootballTeamPlayerDto>.Failure($"This team already uses jersey number '{request.JerseyNumber}'");
            }    

            _logger.LogInformation("Updating player {PlayerId} in team {TeamId}", request.PlayerId, request.TeamId);
            
            // Update the team player information
            team.UpdateTeamPlayer(request.PlayerId, request.Position, request.JerseyNumber, request.IsActive);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Get the player and person information for the response
            FootballPlayer? player = await _playerRepository.GetByIdAsync(request.PlayerId);
            if (player == null)
            {
                _logger.LogWarning("Player with ID {PlayerId} not found", request.PlayerId);
                return Result<FootballTeamPlayerDto>.Failure($"Player with ID {request.PlayerId} not found.");
            }

            Person? person = await _personRepository.GetByIdAsync(player.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found for player {PlayerId}", player.PersonId, request.PlayerId);
                return Result<FootballTeamPlayerDto>.Failure("Associated person not found");
            }

            // Get the updated team player from the roster
            FootballTeamPlayer updatedTeamPlayer = team.Roster.First(p => p.PlayerId == request.PlayerId);
            
            // Create the DTO manually since there's no dedicated mapper
            FootballTeamPlayerDto teamPlayerDto = new FootballTeamPlayerDto(
                updatedTeamPlayer.TeamId,
                updatedTeamPlayer.PlayerId,
                person.FullName,
                updatedTeamPlayer.Position,
                updatedTeamPlayer.JerseyNumber,
                updatedTeamPlayer.IsActive,
                null, // Player DTO not needed for this response
                updatedTeamPlayer.GamesPlayed,
                updatedTeamPlayer.Goals,
                updatedTeamPlayer.Assists,
                updatedTeamPlayer.YellowCards,
                updatedTeamPlayer.RedCards
            );
            
            _logger.LogInformation("Successfully updated player {PlayerId} in team {TeamId}", request.PlayerId, request.TeamId);

            return Result<FootballTeamPlayerDto>.Success(teamPlayerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating player {PlayerId} in team {TeamId}", request.PlayerId, request.TeamId);
            return Result<FootballTeamPlayerDto>.Failure("An error occurred while updating the player in the team.");
        }
    }
} 

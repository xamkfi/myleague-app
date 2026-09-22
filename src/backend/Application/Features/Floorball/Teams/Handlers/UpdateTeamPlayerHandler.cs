using Application.Features.Floorball.Teams.Commands;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Common;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Common;
using Domain.Repositories.Floorball;
using Domain.ValueObjects.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;
using System.Linq;

namespace Application.Features.Floorball.Teams.Handlers;

/// <summary>
/// Handler for updating a player's information within a floorball team
/// </summary>
public class UpdateTeamPlayerHandler : IRequestHandler<UpdateFloorballTeamPlayerCommand, Result<FloorballTeamPlayerDto>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateTeamPlayerHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateTeamPlayerHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateTeamPlayerHandler(
        IFloorballTeamRepository teamRepository,
        IFloorballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<UpdateTeamPlayerHandler> logger)
    {
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateFloorballTeamPlayerCommand request
    /// </summary>
    /// <param name="request">The command containing updated player information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated team player as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballTeamPlayerDto>> Handle(UpdateFloorballTeamPlayerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the team
            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team not found with ID: {TeamId}", request.TeamId);
                return Result<FloorballTeamPlayerDto>.Failure($"Team with ID {request.TeamId} not found.");
            }

            // Check if player exists in the team roster
            FloorballTeamPlayer? teamPlayer = team.Roster.FirstOrDefault(p =>
                p.PlayerId == request.PlayerId && p.CompetitionId == request.CompetitionId);
            if (teamPlayer == null)
            {
                _logger.LogWarning("Player {PlayerId} not found in team {TeamId} roster", request.PlayerId, request.TeamId);
                return Result<FloorballTeamPlayerDto>.Failure($"Player with ID {request.PlayerId} is not in the team roster.");
            }

            _logger.LogInformation("Updating player {PlayerId} in team {TeamId}", request.PlayerId, request.TeamId);
            
            team.UpdateTeamPlayer(request.PlayerId, request.Position, request.JerseyNumber, request.IsActive, request.CompetitionId);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Get the player and person information for the response
            FloorballPlayer? player = await _playerRepository.GetByIdAsync(request.PlayerId);
            if (player == null)
            {
                _logger.LogWarning("Player with ID {PlayerId} not found", request.PlayerId);
                return Result<FloorballTeamPlayerDto>.Failure($"Player with ID {request.PlayerId} not found.");
            }

            Person? person = await _personRepository.GetByIdAsync(player.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found for player {PlayerId}", player.PersonId, request.PlayerId);
                return Result<FloorballTeamPlayerDto>.Failure("Associated person not found");
            }

            // Get the updated team player from the roster
            FloorballTeamPlayer updatedTeamPlayer = team.Roster.First(p =>
                p.PlayerId == request.PlayerId && p.CompetitionId == request.CompetitionId);
            
            // Create the DTO manually since there's no dedicated mapper
            FloorballTeamPlayerDto teamPlayerDto = new FloorballTeamPlayerDto(
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
                updatedTeamPlayer.PenaltyMinutes
            );
            
            _logger.LogInformation("Successfully updated player {PlayerId} in team {TeamId}", request.PlayerId, request.TeamId);

            return Result<FloorballTeamPlayerDto>.Success(teamPlayerDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected updating player {PlayerId} in team {TeamId}", request.PlayerId, request.TeamId);
            return Result<FloorballTeamPlayerDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid update for player {PlayerId} in team {TeamId}", request.PlayerId, request.TeamId);
            return Result<FloorballTeamPlayerDto>.Failure(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database rejected updating player {PlayerId} in team {TeamId}", request.PlayerId, request.TeamId);
            return Result<FloorballTeamPlayerDto>.Failure(
                "Jersey number is already used by another player on this team in this competition.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
    }
} 

using Application.Common;
using Application.Features.Floorball.Teams.Commands;
using Application.Features.Floorball.Teams.DTOs;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Teams.Handlers;

/// <summary>
/// Handler for updating only the jersey number of a player in a floorball team roster.
/// The player's position and active status are preserved.
/// </summary>
public class UpdateTeamPlayerJerseyNumberHandler : IRequestHandler<UpdateTeamPlayerJerseyNumberCommand, Result<FloorballTeamPlayerDto>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateTeamPlayerJerseyNumberHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateTeamPlayerJerseyNumberHandler class
    /// </summary>
    public UpdateTeamPlayerJerseyNumberHandler(
        IFloorballTeamRepository teamRepository,
        IFloorballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<UpdateTeamPlayerJerseyNumberHandler> logger)
    {
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateTeamPlayerJerseyNumberCommand request
    /// </summary>
    public async Task<Result<FloorballTeamPlayerDto>> Handle(UpdateTeamPlayerJerseyNumberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                return Result<FloorballTeamPlayerDto>.Failure($"Team with ID {request.TeamId} not found.");
            }

            FloorballTeamPlayer? teamPlayer = team.Roster.FirstOrDefault(p =>
                p.PlayerId == request.PlayerId && p.CompetitionId == request.CompetitionId);
            if (teamPlayer == null)
            {
                return Result<FloorballTeamPlayerDto>.Failure($"Player with ID {request.PlayerId} is not in the team roster.");
            }

            _logger.LogInformation(
                "Updating jersey number for player {PlayerId} in team {TeamId} to {JerseyNumber}",
                request.PlayerId, request.TeamId, request.JerseyNumber);

            team.UpdateTeamPlayer(request.PlayerId, teamPlayer.Position, request.JerseyNumber, teamPlayer.IsActive, request.CompetitionId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballPlayer? player = await _playerRepository.GetByIdAsync(request.PlayerId);
            Person? person = player != null ? await _personRepository.GetByIdAsync(player.PersonId) : null;

            FloorballTeamPlayer updatedTeamPlayer = team.Roster.First(p =>
                p.PlayerId == request.PlayerId && p.CompetitionId == request.CompetitionId);
            FloorballTeamPlayerDto teamPlayerDto = new FloorballTeamPlayerDto(
                updatedTeamPlayer.TeamId,
                updatedTeamPlayer.PlayerId,
                person?.FullName ?? string.Empty,
                updatedTeamPlayer.Position,
                updatedTeamPlayer.JerseyNumber,
                updatedTeamPlayer.IsActive,
                null,
                updatedTeamPlayer.GamesPlayed,
                updatedTeamPlayer.Goals,
                updatedTeamPlayer.Assists,
                updatedTeamPlayer.PenaltyMinutes);

            return Result<FloorballTeamPlayerDto>.Success(teamPlayerDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating jersey number for player {PlayerId} in team {TeamId}", request.PlayerId, request.TeamId);
            return Result<FloorballTeamPlayerDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid jersey number for player {PlayerId} in team {TeamId}", request.PlayerId, request.TeamId);
            return Result<FloorballTeamPlayerDto>.Failure(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database rejected jersey number for player {PlayerId} in team {TeamId}", request.PlayerId, request.TeamId);
            return Result<FloorballTeamPlayerDto>.Failure(
                "Jersey number is already used by another player on this team in this competition.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
    }
}

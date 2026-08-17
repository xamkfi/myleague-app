using Application.Common;
using Application.Features.Football.Teams.Commands;
using Application.Features.Football.Teams.DTOs;
using Domain.Entities.Common;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Teams.Handlers;

/// <summary>
/// Handler for updating only the jersey number of a player in a football team roster.
/// The player's position and active status are preserved.
/// </summary>
public class UpdateTeamPlayerJerseyNumberHandler : IRequestHandler<UpdateTeamPlayerJerseyNumberCommand, Result<FootballTeamPlayerDto>>
{
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateTeamPlayerJerseyNumberHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateTeamPlayerJerseyNumberHandler class
    /// </summary>
    public UpdateTeamPlayerJerseyNumberHandler(
        IFootballTeamRepository teamRepository,
        IFootballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IFootballUnitOfWork unitOfWork,
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
    public async Task<Result<FootballTeamPlayerDto>> Handle(UpdateTeamPlayerJerseyNumberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                return Result<FootballTeamPlayerDto>.Failure($"Team with ID {request.TeamId} not found.");
            }

            FootballTeamPlayer? teamPlayer = team.Roster.FirstOrDefault(p => p.PlayerId == request.PlayerId);
            if (teamPlayer == null)
            {
                return Result<FootballTeamPlayerDto>.Failure($"Player with ID {request.PlayerId} is not in the team roster.");
            }

            if (request.JerseyNumber.HasValue &&
                team.Roster.Any(p => p.JerseyNumber == request.JerseyNumber && p.PlayerId != request.PlayerId))
            {
                return Result<FootballTeamPlayerDto>.Failure($"This team already uses jersey number '{request.JerseyNumber}'");
            }

            _logger.LogInformation(
                "Updating jersey number for player {PlayerId} in team {TeamId} to {JerseyNumber}",
                request.PlayerId, request.TeamId, request.JerseyNumber);

            team.UpdateTeamPlayer(request.PlayerId, teamPlayer.Position, request.JerseyNumber, teamPlayer.IsActive);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FootballPlayer? player = await _playerRepository.GetByIdAsync(request.PlayerId);
            Person? person = player != null ? await _personRepository.GetByIdAsync(player.PersonId) : null;

            FootballTeamPlayer updatedTeamPlayer = team.Roster.First(p => p.PlayerId == request.PlayerId);
            FootballTeamPlayerDto teamPlayerDto = new FootballTeamPlayerDto(
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
                updatedTeamPlayer.YellowCards,
                updatedTeamPlayer.RedCards);

            return Result<FootballTeamPlayerDto>.Success(teamPlayerDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating jersey number for player {PlayerId} in team {TeamId}", request.PlayerId, request.TeamId);
            return Result<FootballTeamPlayerDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating jersey number for player {PlayerId} in team {TeamId}", request.PlayerId, request.TeamId);
            return Result<FootballTeamPlayerDto>.Failure("An error occurred while updating the jersey number.");
        }
    }
}

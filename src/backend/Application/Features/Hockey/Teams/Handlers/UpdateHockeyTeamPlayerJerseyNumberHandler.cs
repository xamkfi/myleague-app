using Application.Common;
using Application.Features.Hockey.Teams.Commands;
using Application.Features.Hockey.Teams.DTOs;
using Application.Features.Hockey.Teams.Mappings;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Teams.Handlers;

/// <summary>
/// Handler for updating only the jersey number of a player in a hockey team roster.
/// Position, roster status, and captain role are preserved.
/// </summary>
public class UpdateHockeyTeamPlayerJerseyNumberHandler
    : IRequestHandler<UpdateHockeyTeamPlayerJerseyNumberCommand, Result<HockeyTeamPlayerDto>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateHockeyTeamPlayerJerseyNumberHandler> _logger;

    public UpdateHockeyTeamPlayerJerseyNumberHandler(
        IHockeyTeamRepository teamRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<UpdateHockeyTeamPlayerJerseyNumberHandler> logger)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTeamPlayerDto>> Handle(
        UpdateHockeyTeamPlayerJerseyNumberCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team is null)
            {
                return Result<HockeyTeamPlayerDto>.NotFound("HockeyTeam", request.TeamId);
            }

            HockeyTeamPlayer? teamPlayer = team.Roster.FirstOrDefault(p => p.PlayerId == request.PlayerId && p.IsActive);
            if (teamPlayer is null)
            {
                return Result<HockeyTeamPlayerDto>.Failure(
                    $"Player with ID {request.PlayerId} is not in the team roster.");
            }

            _logger.LogInformation(
                "Updating jersey number for hockey player {PlayerId} in team {TeamId} to {JerseyNumber}",
                request.PlayerId, request.TeamId, request.JerseyNumber);

            team.UpdateTeamPlayer(
                request.PlayerId,
                teamPlayer.Position,
                request.JerseyNumber,
                teamPlayer.RosterStatus,
                teamPlayer.CaptainRole,
                teamPlayer.CompetitionId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            HockeyTeamPlayer updated = team.Roster.First(p => p.Id == teamPlayer.Id);
            return Result<HockeyTeamPlayerDto>.Success(HockeyTeamMapper.ToTeamPlayerDto(updated));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                ex,
                "Invalid operation while updating jersey number for hockey player {PlayerId} in team {TeamId}",
                request.PlayerId,
                request.TeamId);
            return Result<HockeyTeamPlayerDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating jersey number for hockey player {PlayerId} in team {TeamId}",
                request.PlayerId,
                request.TeamId);
            return Result<HockeyTeamPlayerDto>.Failure("An error occurred while updating the jersey number.");
        }
    }
}

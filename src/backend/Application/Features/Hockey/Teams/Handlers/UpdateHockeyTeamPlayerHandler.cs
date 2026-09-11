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
/// Handles updating a hockey team roster membership.
/// </summary>
public class UpdateHockeyTeamPlayerHandler : IRequestHandler<UpdateHockeyTeamPlayerCommand, Result<HockeyTeamDto>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateHockeyTeamPlayerHandler> _logger;

    public UpdateHockeyTeamPlayerHandler(
        IHockeyTeamRepository teamRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<UpdateHockeyTeamPlayerHandler> logger)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTeamDto>> Handle(UpdateHockeyTeamPlayerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team is null)
            {
                return Result<HockeyTeamDto>.NotFound("HockeyTeam", request.TeamId);
            }

            team.UpdateTeamPlayer(
                request.PlayerId,
                request.Position,
                request.JerseyNumber,
                request.RosterStatus,
                request.CaptainRole,
                request.CompetitionId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated player {PlayerId} on hockey team {TeamId}", request.PlayerId, request.TeamId);
            return Result<HockeyTeamDto>.Success(HockeyTeamMapper.ToDto(team));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected UpdateHockeyTeamPlayer for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid UpdateHockeyTeamPlayer for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed UpdateHockeyTeamPlayer for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure("An error occurred while updating the team player.", ex.Flatten());
        }
    }
}

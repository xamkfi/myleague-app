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
/// Handles removing a team player from a hockey line.
/// </summary>
public class RemovePlayerFromHockeyLineHandler : IRequestHandler<RemovePlayerFromHockeyLineCommand, Result<HockeyTeamDto>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RemovePlayerFromHockeyLineHandler> _logger;

    public RemovePlayerFromHockeyLineHandler(
        IHockeyTeamRepository teamRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RemovePlayerFromHockeyLineHandler> logger)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTeamDto>> Handle(
        RemovePlayerFromHockeyLineCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team is null)
            {
                return Result<HockeyTeamDto>.NotFound("HockeyTeam", request.TeamId);
            }

            team.RemovePlayerFromLine(request.LineId, request.TeamPlayerId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Removed team player {TeamPlayerId} from line {LineId} on team {TeamId}",
                request.TeamPlayerId,
                request.LineId,
                request.TeamId);

            return Result<HockeyTeamDto>.Success(HockeyTeamMapper.ToDto(team));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected RemovePlayerFromHockeyLine for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed RemovePlayerFromHockeyLine for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure("An error occurred while removing the player from the line.", ex.Flatten());
        }
    }
}

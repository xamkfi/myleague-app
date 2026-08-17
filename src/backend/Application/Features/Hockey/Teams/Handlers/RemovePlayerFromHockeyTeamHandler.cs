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
/// Handles removing a player from a hockey team roster.
/// </summary>
public class RemovePlayerFromHockeyTeamHandler : IRequestHandler<RemovePlayerFromHockeyTeamCommand, Result<HockeyTeamDto>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RemovePlayerFromHockeyTeamHandler> _logger;

    public RemovePlayerFromHockeyTeamHandler(
        IHockeyTeamRepository teamRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RemovePlayerFromHockeyTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTeamDto>> Handle(
        RemovePlayerFromHockeyTeamCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team is null)
            {
                return Result<HockeyTeamDto>.NotFound("HockeyTeam", request.TeamId);
            }

            team.RemovePlayer(request.PlayerId, request.CompetitionId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Removed player {PlayerId} from hockey team {TeamId}", request.PlayerId, request.TeamId);
            return Result<HockeyTeamDto>.Success(HockeyTeamMapper.ToDto(team));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected RemovePlayerFromHockeyTeam for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid RemovePlayerFromHockeyTeam for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed RemovePlayerFromHockeyTeam for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure("An error occurred while removing the player from the team.", ex.Flatten());
        }
    }
}

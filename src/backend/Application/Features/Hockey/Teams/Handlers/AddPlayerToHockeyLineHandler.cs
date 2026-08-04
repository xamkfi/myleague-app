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
/// Handles adding a team player to a hockey line.
/// </summary>
public class AddPlayerToHockeyLineHandler : IRequestHandler<AddPlayerToHockeyLineCommand, Result<HockeyTeamDto>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<AddPlayerToHockeyLineHandler> _logger;

    public AddPlayerToHockeyLineHandler(
        IHockeyTeamRepository teamRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<AddPlayerToHockeyLineHandler> logger)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTeamDto>> Handle(AddPlayerToHockeyLineCommand request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team is null)
            {
                return Result<HockeyTeamDto>.NotFound("HockeyTeam", request.TeamId);
            }

            team.AddPlayerToLine(request.LineId, request.TeamPlayerId, request.Slot, request.Order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Added team player {TeamPlayerId} to line {LineId} on team {TeamId}",
                request.TeamPlayerId,
                request.LineId,
                request.TeamId);

            return Result<HockeyTeamDto>.Success(HockeyTeamMapper.ToDto(team));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected AddPlayerToHockeyLine for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid AddPlayerToHockeyLine for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed AddPlayerToHockeyLine for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure("An error occurred while adding the player to the line.", ex.Flatten());
        }
    }
}

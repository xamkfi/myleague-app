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
/// Handles SetHockeyTeamActiveStatus.
/// </summary>
public class SetHockeyTeamActiveStatusHandler : IRequestHandler<SetHockeyTeamActiveStatusCommand, Result<HockeyTeamDto>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<SetHockeyTeamActiveStatusHandler> _logger;

    public SetHockeyTeamActiveStatusHandler(
        IHockeyTeamRepository teamRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<SetHockeyTeamActiveStatusHandler> logger)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTeamDto>> Handle(
        SetHockeyTeamActiveStatusCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team is null)
            {
                return Result<HockeyTeamDto>.NotFound("HockeyTeam", request.TeamId);
            }

            team.SetActiveStatus(request.IsActive);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Set active={IsActive} on hockey team {TeamId}", request.IsActive, request.TeamId);
            return Result<HockeyTeamDto>.Success(HockeyTeamMapper.ToDto(team));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed SetHockeyTeamActiveStatus for {TeamId}", request.TeamId);
            return Result<HockeyTeamDto>.Failure("An error occurred while updating team active status.", ex.Flatten());
        }
    }
}

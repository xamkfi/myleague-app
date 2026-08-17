using Application.Common;
using Application.Features.Football.TeamManagers.Commands;
using Application.Features.Football.TeamManagers.DTOs;
using Application.Features.Football.TeamManagers.Mappings;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.TeamManagers.Handlers;

/// <summary>
/// Handler for assigning a football team manager to a different team.
/// </summary>
public class UpdateFootballTeamManagerTeamHandler : IRequestHandler<UpdateFootballTeamManagerTeamCommand, Result<FootballTeamManagerDto>>
{
    private readonly IFootballTeamManagerRepository _teamManagerRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFootballTeamManagerTeamHandler> _logger;

    public UpdateFootballTeamManagerTeamHandler(
        IFootballTeamManagerRepository teamManagerRepository,
        IFootballTeamRepository teamRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<UpdateFootballTeamManagerTeamHandler> logger)
    {
        _teamManagerRepository = teamManagerRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballTeamManagerDto>> Handle(UpdateFootballTeamManagerTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballTeamManager? teamManager = await _teamManagerRepository.GetByIdAsync(request.Id);
            if (teamManager == null)
            {
                return Result<FootballTeamManagerDto>.NotFound("FootballTeamManager", request.Id);
            }

            bool teamExists = await _teamRepository.ExistsAsync(request.TeamId);
            if (!teamExists)
            {
                return Result<FootballTeamManagerDto>.Failure("Team not found");
            }

            teamManager.UpdateTeam(request.TeamId);
            await _teamManagerRepository.UpdateAsync(teamManager);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FootballTeamManagerDto>.Success(FootballTeamManagerMapper.ToDto(teamManager));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating football team manager team: {Id}", request.Id);
            return Result<FootballTeamManagerDto>.Failure("An error occurred while updating the football team manager team.");
        }
    }
}

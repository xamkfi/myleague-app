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
/// Handler for updating a football team manager's active status.
/// </summary>
public class UpdateFootballTeamManagerHandler : IRequestHandler<UpdateFootballTeamManagerCommand, Result<FootballTeamManagerDto>>
{
    private readonly IFootballTeamManagerRepository _teamManagerRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFootballTeamManagerHandler> _logger;

    public UpdateFootballTeamManagerHandler(
        IFootballTeamManagerRepository teamManagerRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<UpdateFootballTeamManagerHandler> logger)
    {
        _teamManagerRepository = teamManagerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballTeamManagerDto>> Handle(UpdateFootballTeamManagerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballTeamManager? teamManager = await _teamManagerRepository.GetByIdAsync(request.Id);
            if (teamManager == null)
            {
                _logger.LogWarning("Football team manager not found with ID: {Id}", request.Id);
                return Result<FootballTeamManagerDto>.NotFound("FootballTeamManager", request.Id);
            }

            FootballTeamManagerMapper.UpdateFromCommand(teamManager, request);
            await _teamManagerRepository.UpdateAsync(teamManager);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FootballTeamManagerDto>.Success(FootballTeamManagerMapper.ToDto(teamManager));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating football team manager: {Id}", request.Id);
            return Result<FootballTeamManagerDto>.Failure("An error occurred while updating the football team manager.");
        }
    }
}

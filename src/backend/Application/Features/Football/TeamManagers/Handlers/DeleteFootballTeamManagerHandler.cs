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
/// Handler for deleting a football team manager.
/// </summary>
public class DeleteFootballTeamManagerHandler : IRequestHandler<DeleteFootballTeamManagerCommand, Result<FootballTeamManagerDto>>
{
    private readonly IFootballTeamManagerRepository _teamManagerRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFootballTeamManagerHandler> _logger;

    public DeleteFootballTeamManagerHandler(
        IFootballTeamManagerRepository teamManagerRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<DeleteFootballTeamManagerHandler> logger)
    {
        _teamManagerRepository = teamManagerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballTeamManagerDto>> Handle(DeleteFootballTeamManagerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballTeamManager? teamManager = await _teamManagerRepository.GetByIdAsync(request.Id);
            if (teamManager == null)
            {
                _logger.LogWarning("Football team manager not found with ID: {Id}", request.Id);
                return Result<FootballTeamManagerDto>.NotFound("FootballTeamManager", request.Id);
            }

            FootballTeamManagerDto dto = FootballTeamManagerMapper.ToDto(teamManager);
            await _teamManagerRepository.DeleteAsync(request.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FootballTeamManagerDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting football team manager: {Id}", request.Id);
            return Result<FootballTeamManagerDto>.Failure("An error occurred while deleting the football team manager.");
        }
    }
}

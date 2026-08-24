using Application.Common;
using Application.Features.Floorball.TeamManagers.Commands;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.TeamManagers.Mappings;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.TeamManagers.Handlers;

/// <summary>
/// Handler for deleting a floorball team manager.
/// </summary>
public class DeleteFloorballTeamManagerHandler : IRequestHandler<DeleteFloorballTeamManagerCommand, Result<FloorballTeamManagerDto>>
{
    private readonly IFloorballTeamManagerRepository _teamManagerRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFloorballTeamManagerHandler> _logger;

    public DeleteFloorballTeamManagerHandler(
        IFloorballTeamManagerRepository teamManagerRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<DeleteFloorballTeamManagerHandler> logger)
    {
        _teamManagerRepository = teamManagerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballTeamManagerDto>> Handle(
        DeleteFloorballTeamManagerCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            FloorballTeamManager? teamManager = await _teamManagerRepository.GetByIdAsync(request.Id);
            if (teamManager == null)
            {
                _logger.LogWarning("Floorball team manager not found with ID: {Id}", request.Id);
                return Result<FloorballTeamManagerDto>.NotFound("FloorballTeamManager", request.Id);
            }

            FloorballTeamManagerDto dto = FloorballTeamManagerMapper.ToDto(teamManager);
            await _teamManagerRepository.DeleteAsync(request.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FloorballTeamManagerDto>.Success(dto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error occurred while deleting floorball team manager: {Id}", request.Id);
            return Result<FloorballTeamManagerDto>.Failure("An error occurred while deleting the floorball team manager.");
        }
    }
}

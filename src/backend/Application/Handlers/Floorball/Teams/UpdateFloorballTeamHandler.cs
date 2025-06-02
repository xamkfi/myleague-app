using Application.Commands.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Teams;

/// <summary>
/// Handler for updating an existing floorball team
/// </summary>
public class UpdateFloorballTeamHandler : IRequestHandler<UpdateFloorballTeamCommand, Result<FloorballTeamDto>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFloorballTeamHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateFloorballTeamHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateFloorballTeamHandler(
        IFloorballTeamRepository teamRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateFloorballTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateFloorballTeamCommand request
    /// </summary>
    /// <param name="request">The command containing updated team information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated team as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballTeamDto>> Handle(UpdateFloorballTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find the existing team
            FloorballTeam? existingTeam = await _teamRepository.GetByIdAsync(request.TeamId);
            if (existingTeam == null)
            {
                _logger.LogWarning("Attempt to update non-existent floorball team with ID: {TeamId}", request.TeamId);
                return Result<FloorballTeamDto>.NotFound("FloorballTeam", request.TeamId);
            }

            // Update the team
            FloorballTeamMapper.UpdateFromCommand(existingTeam, request);
            
            _logger.LogInformation("Updating floorball team: {TeamId}", existingTeam.Id);
            await _teamRepository.UpdateAsync(existingTeam);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballTeamDto teamDto = FloorballTeamMapper.ToDto(existingTeam);
            _logger.LogInformation("Successfully updated floorball team with ID: {TeamId}", existingTeam.Id);

            return Result<FloorballTeamDto>.Success(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating floorball team: {TeamId}", request.TeamId);
            return Result<FloorballTeamDto>.Failure("An error occurred while updating the floorball team.");
        }
    }
} 
using Application.Commands.Floorball.Team;
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
using Domain.Repositories.Common;

namespace Application.Handlers.Floorball.Teams;

/// <summary>
/// Handler for creating a new floorball team
/// </summary>
public class CreateFloorballTeamHandler : IRequestHandler<CreateFloorballTeamCommand, Result<FloorballTeamDto>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFloorballTeamHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateFloorballTeamHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateFloorballTeamHandler(
        IFloorballTeamRepository teamRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateFloorballTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateFloorballTeamCommand request
    /// </summary>
    /// <param name="request">The command containing team information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created team as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballTeamDto>> Handle(CreateFloorballTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create the team entity
            FloorballTeam team = FloorballTeamMapper.ToEntity(request);

            _logger.LogInformation("Creating new floorball team: {TeamName}", request.Name);
            await _teamRepository.AddAsync(team);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballTeamDto teamDto = FloorballTeamMapper.ToDto(team);
            _logger.LogInformation("Successfully created floorball team with ID: {TeamId}", team.Id);

            return Result<FloorballTeamDto>.Success(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating floorball team: {TeamName}", request.Name);
            return Result<FloorballTeamDto>.Failure("An error occurred while creating the floorball team.");
        }
    }
} 

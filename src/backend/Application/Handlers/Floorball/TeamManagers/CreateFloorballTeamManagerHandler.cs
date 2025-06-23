using Application.Commands.Floorball.TeamManager;
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
using Domain.Entities.Common;

namespace Application.Handlers.Floorball.TeamManagers;

/// <summary>
/// Handler for creating a new floorball team manager
/// </summary>
public class CreateFloorballTeamManagerHandler : IRequestHandler<CreateFloorballTeamManagerCommand, Result<FloorballTeamManagerDto>>
{
    private readonly IFloorballTeamManagerRepository _teamManagerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFloorballTeamManagerHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateFloorballTeamManagerHandler class
    /// </summary>
    /// <param name="teamManagerRepository">The floorball team manager repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateFloorballTeamManagerHandler(
        IFloorballTeamManagerRepository teamManagerRepository, 
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork, 
        ILogger<CreateFloorballTeamManagerHandler> logger)
    {
        _teamManagerRepository = teamManagerRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateFloorballTeamManagerCommand request
    /// </summary>
    /// <param name="request">The command containing team manager information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created team manager as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballTeamManagerDto>> Handle(CreateFloorballTeamManagerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the person exists
            Person? person = await _personRepository.GetByIdAsync(request.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found", request.PersonId);
                return Result<FloorballTeamManagerDto>.Failure("Person not found");
            }

            // Check if a team manager profile already exists for this person
            bool teamManagerExists = await _teamManagerRepository.ExistsByPersonIdAsync(request.PersonId);
            if (teamManagerExists)
            {
                _logger.LogWarning("Team manager profile already exists for person ID {PersonId}", request.PersonId);
                return Result<FloorballTeamManagerDto>.Failure("A team manager profile already exists for this person");
            }

            // Create the team manager entity
            FloorballTeamManager teamManager = FloorballTeamManagerMapper.ToEntity(request);

            _logger.LogInformation("Creating new floorball team manager for person: {PersonId}", request.PersonId);
            await _teamManagerRepository.AddAsync(teamManager);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballTeamManagerDto teamManagerDto = FloorballTeamManagerMapper.ToDto(teamManager);
            _logger.LogInformation("Successfully created floorball team manager with ID: {TeamManagerId}", teamManager.Id);

            return Result<FloorballTeamManagerDto>.Success(teamManagerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating floorball team manager for person: {PersonId}", request.PersonId);
            return Result<FloorballTeamManagerDto>.Failure("An error occurred while creating the floorball team manager.");
        }
    }
} 
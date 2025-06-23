using Application.Commands.Floorball.Coach;
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

namespace Application.Handlers.Floorball.Coaches;

/// <summary>
/// Handler for creating a new floorball coach
/// </summary>
public class CreateFloorballCoachHandler : IRequestHandler<CreateFloorballCoachCommand, Result<FloorballCoachDto>>
{
    private readonly IFloorballCoachRepository _coachRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFloorballCoachHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateFloorballCoachHandler class
    /// </summary>
    /// <param name="coachRepository">The floorball coach repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateFloorballCoachHandler(
        IFloorballCoachRepository coachRepository, 
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork, 
        ILogger<CreateFloorballCoachHandler> logger)
    {
        _coachRepository = coachRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateFloorballCoachCommand request
    /// </summary>
    /// <param name="request">The command containing coach information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created coach as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballCoachDto>> Handle(CreateFloorballCoachCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the person exists
            Person? person = await _personRepository.GetByIdAsync(request.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found", request.PersonId);
                return Result<FloorballCoachDto>.Failure("Person not found");
            }

            // Check if a coach profile already exists for this person
            bool coachExists = await _coachRepository.ExistsByPersonIdAsync(request.PersonId);
            if (coachExists)
            {
                _logger.LogWarning("Coach profile already exists for person ID {PersonId}", request.PersonId);
                return Result<FloorballCoachDto>.Failure("A coach profile already exists for this person");
            }

            // Create the coach entity
            FloorballCoach coach = FloorballCoachMapper.ToEntity(request);

            _logger.LogInformation("Creating new floorball coach for person: {PersonId}", request.PersonId);
            await _coachRepository.AddAsync(coach);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballCoachDto coachDto = FloorballCoachMapper.ToDto(coach);
            _logger.LogInformation("Successfully created floorball coach with ID: {CoachId}", coach.Id);

            return Result<FloorballCoachDto>.Success(coachDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating floorball coach for person: {PersonId}", request.PersonId);
            return Result<FloorballCoachDto>.Failure("An error occurred while creating the floorball coach.");
        }
    }
} 
using Application.Commands.Floorball.Referee;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Application.Mappings.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;
using Domain.Entities.Common;

namespace Application.Handlers.Floorball.Referees;

/// <summary>
/// Handler for creating a new floorball referee
/// </summary>
public class CreateFloorballRefereeHandler : IRequestHandler<CreateFloorballRefereeCommand, Result<FloorballRefereeDto>>
{
    private readonly IFloorballRefereeRepository _refereeRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFloorballRefereeHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateFloorballRefereeHandler class
    /// </summary>
    /// <param name="refereeRepository">The floorball referee repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateFloorballRefereeHandler(
        IFloorballRefereeRepository refereeRepository, 
        IPersonRepository personRepository,
        IFloorballUnitOfWork unitOfWork, 
        ILogger<CreateFloorballRefereeHandler> logger)
    {
        _refereeRepository = refereeRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateFloorballRefereeCommand request
    /// </summary>
    /// <param name="request">The command containing referee information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created referee as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballRefereeDto>> Handle(CreateFloorballRefereeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating floorball referee for person: {PersonId}", request.PersonId);

            // Check if the person exists
            Person? person = await _personRepository.GetByIdAsync(request.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found", request.PersonId);
                return Result<FloorballRefereeDto>.Failure("Person not found");
            }

            // Create the referee entity using the mapper
            FloorballReferee referee = FloorballRefereeMapper.ToEntity(request);

            // AddAsync automatically saves changes to FloorballDbContext
            await _refereeRepository.AddAsync(referee);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Note: Since Person navigation property is ignored in EF config, 
            // we need to manually set it for the DTO mapping
            FloorballRefereeDto refereeDto = new FloorballRefereeDto(
                referee.Id,
                referee.PersonId,
                PersonMapper.ToDto(person),
                referee.IsActive,
                referee.LicenseIssueDate,
                referee.LicenseExpiryDate,
                referee.MatchesOfficiated
            );

            _logger.LogInformation("Successfully created floorball referee with ID: {RefereeId}", referee.Id);

            return Result<FloorballRefereeDto>.Success(refereeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating floorball referee for person: {PersonId}", request.PersonId);
            return Result<FloorballRefereeDto>.Failure("An error occurred while creating the floorball referee.");
        }
    }
} 

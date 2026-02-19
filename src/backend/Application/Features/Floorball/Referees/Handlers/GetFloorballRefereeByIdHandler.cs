using Application.Queries.Floorball.Referee;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Domain.Entities.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Mappings.Common;

namespace Application.Handlers.Floorball.Referees;

/// <summary>
/// Handler for retrieving a single floorball referee by ID
/// </summary>
public class GetFloorballRefereeByIdHandler : IRequestHandler<GetFloorballRefereeByIdQuery, Result<FloorballRefereeDto>>
{
    private readonly IFloorballRefereeRepository _refereeRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetFloorballRefereeByIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballRefereeByIdHandler class
    /// </summary>
    /// <param name="refereeRepository">The floorball referee repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballRefereeByIdHandler(
        IFloorballRefereeRepository refereeRepository,
        IPersonRepository personRepository,
        ILogger<GetFloorballRefereeByIdHandler> logger)
    {
        _refereeRepository = refereeRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFloorballRefereeByIdQuery request
    /// </summary>
    /// <param name="request">The query containing the referee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The floorball referee as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballRefereeDto>> Handle(GetFloorballRefereeByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving floorball referee with ID: {RefereeId}", request.Id);

            // Get the referee
            FloorballReferee? referee = await _refereeRepository.GetByIdAsync(request.Id);
            if (referee == null)
            {
                _logger.LogWarning("Floorball referee with ID {RefereeId} not found", request.Id);
                return Result<FloorballRefereeDto>.NotFound("FloorballReferee", request.Id);
            }

            // Get the associated person
            Person? person = await _personRepository.GetByIdAsync(referee.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found for referee {RefereeId}", referee.PersonId, referee.Id);
                return Result<FloorballRefereeDto>.Failure("Associated person not found");
            }

            // Create the DTO
            FloorballRefereeDto refereeDto = new FloorballRefereeDto(
                referee.Id,
                referee.PersonId,
                PersonMapper.ToDto(person),
                referee.IsActive,
                referee.LicenseIssueDate,
                referee.LicenseExpiryDate,
                referee.MatchesOfficiated
            );

            _logger.LogInformation("Successfully retrieved floorball referee with ID: {RefereeId}", request.Id);
            return Result<FloorballRefereeDto>.Success(refereeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball referee with ID: {RefereeId}", request.Id);
            return Result<FloorballRefereeDto>.Failure("An error occurred while retrieving the floorball referee.");
        }
    }
} 
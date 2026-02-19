using Application.Features.Floorball.Referees.Commands;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;
using Domain.Entities.Common;

namespace Application.Features.Floorball.Referees.Handlers;

/// <summary>
/// Handler for creating a new floorball referee
/// </summary>
public class CreateFloorballRefereeHandler : IRequestHandler<CreateFloorballRefereeCommand, Result<FloorballRefereeDto>>
{
    private readonly IFloorballRefereeRepository _refereeRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFloorballUnitOfWork _floorballUnitOfWork;
    private readonly ILogger<CreateFloorballRefereeHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateFloorballRefereeHandler class
    /// </summary>
    /// <param name="refereeRepository">The floorball referee repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="floorballUnitOfWork">The floorball unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateFloorballRefereeHandler(
        IFloorballRefereeRepository refereeRepository, 
        IPersonRepository personRepository,
        IFloorballUnitOfWork floorballUnitOfWork, 
        ILogger<CreateFloorballRefereeHandler> logger)
    {
        _refereeRepository = refereeRepository;
        _personRepository = personRepository;
        _floorballUnitOfWork = floorballUnitOfWork;
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
            _logger.LogInformation("Starting referee creation for person: {PersonId}, LicenseIssueDate: {LicenseIssueDate}, LicenseExpiryDate: {LicenseExpiryDate}", 
                request.PersonId, request.LicenseIssueDate, request.LicenseExpiryDate);

            // Check if the person exists
            Person? person = await _personRepository.GetByIdAsync(request.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found", request.PersonId);
                return Result<FloorballRefereeDto>.Failure("Person not found");
            }
            _logger.LogInformation("Found person: {PersonName} (ID: {PersonId})", person.FullName, person.Id);

            // Check if this person is already a referee
            IEnumerable<FloorballReferee> existingReferees = await _refereeRepository.GetAllAsync();
            FloorballReferee? existingReferee = existingReferees.FirstOrDefault(r => r.PersonId == request.PersonId);
            if (existingReferee != null)
            {
                _logger.LogWarning("Person {PersonId} is already a referee with ID {RefereeId}", request.PersonId, existingReferee.Id);
                return Result<FloorballRefereeDto>.Failure("This person is already a referee");
            }
            _logger.LogInformation("Confirmed person is not already a referee");

            // Create the referee entity using the mapper
            _logger.LogInformation("Creating referee entity from command");
            FloorballReferee referee = FloorballRefereeMapper.ToEntity(request);
            _logger.LogInformation("Created referee entity with ID: {RefereeId}", referee.Id);
            
            referee.SetPerson(person); // Set the person to the referee
            _logger.LogInformation("Set person on referee entity");

            // AddAsync automatically saves changes to FloorballDbContext
            _logger.LogInformation("Adding referee to repository");
            await _refereeRepository.AddAsync(referee);
            _logger.LogInformation("Added referee to repository, now saving changes");
            
            int changesSaved = await _floorballUnitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("SaveChanges completed. Changes saved: {ChangesSaved}", changesSaved);

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

            _logger.LogInformation("Successfully created floorball referee with ID: {RefereeId}, returning DTO", referee.Id);

            return Result<FloorballRefereeDto>.Success(refereeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating floorball referee for person: {PersonId}. Exception: {ExceptionMessage}", 
                request.PersonId, ex.Message);
            return Result<FloorballRefereeDto>.Failure($"An error occurred while creating the floorball referee: {ex.Message}");
        }
    }
} 

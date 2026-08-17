using Application.Features.Football.Referees.Commands;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Application.Features.Football.Teams.Mappings;
using Application.Features.Football.Players.Mappings;
using Application.Features.Football.Referees.Mappings;
using Application.Features.Football.TeamManagers.Mappings;
using Application.Common;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
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

namespace Application.Features.Football.Referees.Handlers;

/// <summary>
/// Handler for creating a new football referee
/// </summary>
public class CreateFootballRefereeHandler : IRequestHandler<CreateFootballRefereeCommand, Result<FootballRefereeDto>>
{
    private readonly IFootballRefereeRepository _refereeRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFootballUnitOfWork _footballUnitOfWork;
    private readonly ILogger<CreateFootballRefereeHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateFootballRefereeHandler class
    /// </summary>
    /// <param name="refereeRepository">The football referee repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="footballUnitOfWork">The football unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateFootballRefereeHandler(
        IFootballRefereeRepository refereeRepository, 
        IPersonRepository personRepository,
        IFootballUnitOfWork footballUnitOfWork, 
        ILogger<CreateFootballRefereeHandler> logger)
    {
        _refereeRepository = refereeRepository;
        _personRepository = personRepository;
        _footballUnitOfWork = footballUnitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateFootballRefereeCommand request
    /// </summary>
    /// <param name="request">The command containing referee information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created referee as a DTO wrapped in a Result</returns>
    public async Task<Result<FootballRefereeDto>> Handle(CreateFootballRefereeCommand request, CancellationToken cancellationToken)
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
                return Result<FootballRefereeDto>.Failure("Person not found");
            }
            _logger.LogInformation("Found person: {PersonName} (ID: {PersonId})", person.FullName, person.Id);

            // Check if this person is already a referee
            IEnumerable<FootballReferee> existingReferees = await _refereeRepository.GetAllAsync();
            FootballReferee? existingReferee = existingReferees.FirstOrDefault(r => r.PersonId == request.PersonId);
            if (existingReferee != null)
            {
                _logger.LogWarning("Person {PersonId} is already a referee with ID {RefereeId}", request.PersonId, existingReferee.Id);
                return Result<FootballRefereeDto>.Failure("This person is already a referee");
            }
            _logger.LogInformation("Confirmed person is not already a referee");

            // Create the referee entity using the mapper
            _logger.LogInformation("Creating referee entity from command");
            FootballReferee referee = FootballRefereeMapper.ToEntity(request);
            _logger.LogInformation("Created referee entity with ID: {RefereeId}", referee.Id);
            
            referee.SetPerson(person); // Set the person to the referee
            _logger.LogInformation("Set person on referee entity");

            // AddAsync automatically saves changes to FootballDbContext
            _logger.LogInformation("Adding referee to repository");
            await _refereeRepository.AddAsync(referee);
            _logger.LogInformation("Added referee to repository, now saving changes");
            
            int changesSaved = await _footballUnitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("SaveChanges completed. Changes saved: {ChangesSaved}", changesSaved);

            // Note: Since Person navigation property is ignored in EF config, 
            // we need to manually set it for the DTO mapping
            FootballRefereeDto refereeDto = new FootballRefereeDto(
                referee.Id,
                referee.PersonId,
                PersonMapper.ToDto(person),
                referee.IsActive,
                referee.LicenseIssueDate,
                referee.LicenseExpiryDate,
                referee.MatchesOfficiated
            );

            _logger.LogInformation("Successfully created football referee with ID: {RefereeId}, returning DTO", referee.Id);

            return Result<FootballRefereeDto>.Success(refereeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating football referee for person: {PersonId}. Exception: {ExceptionMessage}", 
                request.PersonId, ex.Message);
            return Result<FootballRefereeDto>.Failure($"An error occurred while creating the football referee: {ex.Message}");
        }
    }
} 

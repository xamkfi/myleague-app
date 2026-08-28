using Application.Features.Football.Referees.Commands;
using Application.Features.Football.Referees.DTOs;
using Application.Common;
using Application.Features.Common.Deletion;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
using Domain.Repositories.Common;
using Domain.Entities.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using Application.Features.Common.Persons.Mappings;

namespace Application.Features.Football.Referees.Handlers;

/// <summary>
/// Handler for deleting a football referee
/// </summary>
public class DeleteFootballRefereeHandler : IRequestHandler<DeleteFootballRefereeCommand, Result<FootballRefereeDto>>
{
    private readonly IFootballRefereeRepository _refereeRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFootballUnitOfWork _footballUnitOfWork;
    private readonly ILogger<DeleteFootballRefereeHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the DeleteFootballRefereeHandler class
    /// </summary>
    /// <param name="refereeRepository">The football referee repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="footballUnitOfWork">The football unit of work</param>
    /// <param name="logger">The logger</param>
    public DeleteFootballRefereeHandler(
        IFootballRefereeRepository refereeRepository,
        IPersonRepository personRepository,
        IFootballUnitOfWork footballUnitOfWork,
        ILogger<DeleteFootballRefereeHandler> logger)
    {
        _refereeRepository = refereeRepository;
        _personRepository = personRepository;
        _footballUnitOfWork = footballUnitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeleteFootballRefereeCommand request
    /// </summary>
    /// <param name="request">The command containing the referee ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The deleted referee as a DTO wrapped in a Result</returns>
    public async Task<Result<FootballRefereeDto>> Handle(DeleteFootballRefereeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting football referee with ID: {RefereeId}", request.Id);

            // Find the existing referee
            FootballReferee? existingReferee = await _refereeRepository.GetByIdAsync(request.Id);
            if (existingReferee == null)
            {
                _logger.LogWarning("Football referee with ID {RefereeId} not found", request.Id);
                return Result<FootballRefereeDto>.NotFound("FootballReferee", request.Id);
            }

            // Get the associated person for DTO mapping before deletion
            Person? person = await _personRepository.GetByIdAsync(existingReferee.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found for referee {RefereeId}", existingReferee.PersonId, existingReferee.Id);
                return Result<FootballRefereeDto>.Failure("Associated person not found");
            }

            if (await _refereeRepository.IsAssignedToAnyMatchAsync(existingReferee.Id, cancellationToken))
            {
                _logger.LogWarning("Blocked football referee delete for {RefereeId}: assigned to a match", request.Id);
                return Result<FootballRefereeDto>.Failure(DeletionReasons.RefereeAssignedToMatch);
            }

            // Create the DTO before deletion
            FootballRefereeDto refereeDto = new FootballRefereeDto(
                existingReferee.Id,
                existingReferee.PersonId,
                PersonMapper.ToDto(person),
                existingReferee.IsActive,
                existingReferee.LicenseIssueDate,
                existingReferee.LicenseExpiryDate,
                existingReferee.MatchesOfficiated
            );

            // Delete the referee
            await _refereeRepository.DeleteAsync(request.Id);
            await _footballUnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted football referee with ID: {RefereeId}", request.Id);
            return Result<FootballRefereeDto>.Success(refereeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting football referee with ID: {RefereeId}", request.Id);
            return Result<FootballRefereeDto>.Failure("An error occurred while deleting the football referee.");
        }
    }
} 

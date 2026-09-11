using Application.Features.Floorball.Referees.Commands;
using Application.Features.Floorball.Referees.DTOs;
using Application.Common;
using Application.Features.Common.Deletion;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Domain.Entities.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using Application.Features.Common.Persons.Mappings;

namespace Application.Features.Floorball.Referees.Handlers;

/// <summary>
/// Handler for deleting a floorball referee
/// </summary>
public class DeleteFloorballRefereeHandler : IRequestHandler<DeleteFloorballRefereeCommand, Result<FloorballRefereeDto>>
{
    private readonly IFloorballRefereeRepository _refereeRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFloorballUnitOfWork _floorballUnitOfWork;
    private readonly ILogger<DeleteFloorballRefereeHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the DeleteFloorballRefereeHandler class
    /// </summary>
    /// <param name="refereeRepository">The floorball referee repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="floorballUnitOfWork">The floorball unit of work</param>
    /// <param name="logger">The logger</param>
    public DeleteFloorballRefereeHandler(
        IFloorballRefereeRepository refereeRepository,
        IPersonRepository personRepository,
        IFloorballUnitOfWork floorballUnitOfWork,
        ILogger<DeleteFloorballRefereeHandler> logger)
    {
        _refereeRepository = refereeRepository;
        _personRepository = personRepository;
        _floorballUnitOfWork = floorballUnitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeleteFloorballRefereeCommand request
    /// </summary>
    /// <param name="request">The command containing the referee ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The deleted referee as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballRefereeDto>> Handle(DeleteFloorballRefereeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting floorball referee with ID: {RefereeId}", request.Id);

            // Find the existing referee
            FloorballReferee? existingReferee = await _refereeRepository.GetByIdAsync(request.Id);
            if (existingReferee == null)
            {
                _logger.LogWarning("Floorball referee with ID {RefereeId} not found", request.Id);
                return Result<FloorballRefereeDto>.NotFound("FloorballReferee", request.Id);
            }

            // Get the associated person for DTO mapping before deletion
            Person? person = await _personRepository.GetByIdAsync(existingReferee.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found for referee {RefereeId}", existingReferee.PersonId, existingReferee.Id);
                return Result<FloorballRefereeDto>.Failure("Associated person not found");
            }

            if (await _refereeRepository.IsAssignedToAnyMatchAsync(existingReferee.Id, cancellationToken))
            {
                _logger.LogWarning("Blocked floorball referee delete for {RefereeId}: assigned to a match", request.Id);
                return Result<FloorballRefereeDto>.Failure(DeletionReasons.RefereeAssignedToMatch);
            }

            // Create the DTO before deletion
            FloorballRefereeDto refereeDto = new FloorballRefereeDto(
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
            await _floorballUnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted floorball referee with ID: {RefereeId}", request.Id);
            return Result<FloorballRefereeDto>.Success(refereeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting floorball referee with ID: {RefereeId}", request.Id);
            return Result<FloorballRefereeDto>.Failure("An error occurred while deleting the floorball referee.");
        }
    }
} 

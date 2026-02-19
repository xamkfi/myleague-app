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
using Domain.Repositories.Common;
using Domain.Entities.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;

namespace Application.Features.Floorball.Referees.Handlers;

/// <summary>
/// Handler for updating an existing floorball referee
/// </summary>
public class UpdateFloorballRefereeHandler : IRequestHandler<UpdateFloorballRefereeCommand, Result<FloorballRefereeDto>>
{
    private readonly IFloorballRefereeRepository _refereeRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFloorballUnitOfWork _floorballUnitOfWork;
    private readonly ILogger<UpdateFloorballRefereeHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateFloorballRefereeHandler class
    /// </summary>
    /// <param name="refereeRepository">The floorball referee repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="floorballUnitOfWork">The floorball unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateFloorballRefereeHandler(
        IFloorballRefereeRepository refereeRepository,
        IPersonRepository personRepository,
        IFloorballUnitOfWork floorballUnitOfWork,
        ILogger<UpdateFloorballRefereeHandler> logger)
    {
        _refereeRepository = refereeRepository;
        _personRepository = personRepository;
        _floorballUnitOfWork = floorballUnitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateFloorballRefereeCommand request
    /// </summary>
    /// <param name="request">The command containing updated referee information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated referee as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballRefereeDto>> Handle(UpdateFloorballRefereeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating floorball referee with ID: {RefereeId}", request.Id);

            // Find the existing referee
            FloorballReferee? existingReferee = await _refereeRepository.GetByIdAsync(request.Id);
            if (existingReferee == null)
            {
                _logger.LogWarning("Floorball referee with ID {RefereeId} not found", request.Id);
                return Result<FloorballRefereeDto>.NotFound("FloorballReferee", request.Id);
            }

            // Update the referee using the mapper
            FloorballRefereeMapper.UpdateFromCommand(existingReferee, request);

            // Update the referee
            await _refereeRepository.UpdateAsync(existingReferee);
            await _floorballUnitOfWork.SaveChangesAsync(cancellationToken);

            // Get the associated person for DTO mapping
            Person? person = await _personRepository.GetByIdAsync(existingReferee.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found for referee {RefereeId}", existingReferee.PersonId, existingReferee.Id);
                return Result<FloorballRefereeDto>.Failure("Associated person not found");
            }

            // Create the DTO
            FloorballRefereeDto refereeDto = new FloorballRefereeDto(
                existingReferee.Id,
                existingReferee.PersonId,
                PersonMapper.ToDto(person),
                existingReferee.IsActive,
                existingReferee.LicenseIssueDate,
                existingReferee.LicenseExpiryDate,
                existingReferee.MatchesOfficiated
            );

            _logger.LogInformation("Successfully updated floorball referee with ID: {RefereeId}", request.Id);
            return Result<FloorballRefereeDto>.Success(refereeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating floorball referee with ID: {RefereeId}", request.Id);
            return Result<FloorballRefereeDto>.Failure("An error occurred while updating the floorball referee.");
        }
    }
} 

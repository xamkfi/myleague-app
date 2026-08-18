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

namespace Application.Features.Football.Referees.Handlers;

/// <summary>
/// Handler for updating an existing football referee
/// </summary>
public class UpdateFootballRefereeHandler : IRequestHandler<UpdateFootballRefereeCommand, Result<FootballRefereeDto>>
{
    private readonly IFootballRefereeRepository _refereeRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFootballUnitOfWork _footballUnitOfWork;
    private readonly ILogger<UpdateFootballRefereeHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateFootballRefereeHandler class
    /// </summary>
    /// <param name="refereeRepository">The football referee repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="footballUnitOfWork">The football unit of work</param>
    /// <param name="logger">The logger</param>
    public UpdateFootballRefereeHandler(
        IFootballRefereeRepository refereeRepository,
        IPersonRepository personRepository,
        IFootballUnitOfWork footballUnitOfWork,
        ILogger<UpdateFootballRefereeHandler> logger)
    {
        _refereeRepository = refereeRepository;
        _personRepository = personRepository;
        _footballUnitOfWork = footballUnitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateFootballRefereeCommand request
    /// </summary>
    /// <param name="request">The command containing updated referee information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated referee as a DTO wrapped in a Result</returns>
    public async Task<Result<FootballRefereeDto>> Handle(UpdateFootballRefereeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating football referee with ID: {RefereeId}", request.Id);

            // Find the existing referee
            FootballReferee? existingReferee = await _refereeRepository.GetByIdAsync(request.Id);
            if (existingReferee == null)
            {
                _logger.LogWarning("Football referee with ID {RefereeId} not found", request.Id);
                return Result<FootballRefereeDto>.NotFound("FootballReferee", request.Id);
            }

            // Update the referee using the mapper
            FootballRefereeMapper.UpdateFromCommand(existingReferee, request);

            // Update the referee
            await _refereeRepository.UpdateAsync(existingReferee);
            await _footballUnitOfWork.SaveChangesAsync(cancellationToken);

            // Get the associated person for DTO mapping
            Person? person = await _personRepository.GetByIdAsync(existingReferee.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found for referee {RefereeId}", existingReferee.PersonId, existingReferee.Id);
                return Result<FootballRefereeDto>.Failure("Associated person not found");
            }

            // Create the DTO
            FootballRefereeDto refereeDto = new FootballRefereeDto(
                existingReferee.Id,
                existingReferee.PersonId,
                PersonMapper.ToDto(person),
                existingReferee.IsActive,
                existingReferee.LicenseIssueDate,
                existingReferee.LicenseExpiryDate,
                existingReferee.MatchesOfficiated
            );

            _logger.LogInformation("Successfully updated football referee with ID: {RefereeId}", request.Id);
            return Result<FootballRefereeDto>.Success(refereeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating football referee with ID: {RefereeId}", request.Id);
            return Result<FootballRefereeDto>.Failure("An error occurred while updating the football referee.");
        }
    }
} 

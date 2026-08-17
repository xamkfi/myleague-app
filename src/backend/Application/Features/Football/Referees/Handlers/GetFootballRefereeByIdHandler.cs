using Application.Features.Football.Referees.Queries;
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
/// Handler for retrieving a single football referee by ID
/// </summary>
public class GetFootballRefereeByIdHandler : IRequestHandler<GetFootballRefereeByIdQuery, Result<FootballRefereeDto>>
{
    private readonly IFootballRefereeRepository _refereeRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetFootballRefereeByIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFootballRefereeByIdHandler class
    /// </summary>
    /// <param name="refereeRepository">The football referee repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="logger">The logger</param>
    public GetFootballRefereeByIdHandler(
        IFootballRefereeRepository refereeRepository,
        IPersonRepository personRepository,
        ILogger<GetFootballRefereeByIdHandler> logger)
    {
        _refereeRepository = refereeRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFootballRefereeByIdQuery request
    /// </summary>
    /// <param name="request">The query containing the referee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The football referee as a DTO wrapped in a Result</returns>
    public async Task<Result<FootballRefereeDto>> Handle(GetFootballRefereeByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving football referee with ID: {RefereeId}", request.Id);

            // Get the referee
            FootballReferee? referee = await _refereeRepository.GetByIdAsync(request.Id);
            if (referee == null)
            {
                _logger.LogWarning("Football referee with ID {RefereeId} not found", request.Id);
                return Result<FootballRefereeDto>.NotFound("FootballReferee", request.Id);
            }

            // Get the associated person
            Person? person = await _personRepository.GetByIdAsync(referee.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found for referee {RefereeId}", referee.PersonId, referee.Id);
                return Result<FootballRefereeDto>.Failure("Associated person not found");
            }

            // Create the DTO
            FootballRefereeDto refereeDto = new FootballRefereeDto(
                referee.Id,
                referee.PersonId,
                PersonMapper.ToDto(person),
                referee.IsActive,
                referee.LicenseIssueDate,
                referee.LicenseExpiryDate,
                referee.MatchesOfficiated
            );

            _logger.LogInformation("Successfully retrieved football referee with ID: {RefereeId}", request.Id);
            return Result<FootballRefereeDto>.Success(refereeDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving football referee with ID: {RefereeId}", request.Id);
            return Result<FootballRefereeDto>.Failure("An error occurred while retrieving the football referee.");
        }
    }
} 

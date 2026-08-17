using Application.Features.Football.TeamManagers.Commands;
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
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;
using Domain.Entities.Common;

namespace Application.Features.Football.TeamManagers.Handlers;

/// <summary>
/// Handler for creating a new football team manager
/// </summary>
public class CreateFootballTeamManagerHandler : IRequestHandler<CreateFootballTeamManagerCommand, Result<FootballTeamManagerDto>>
{
    private readonly IFootballTeamManagerRepository _teamManagerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFootballTeamManagerHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateFootballTeamManagerHandler class
    /// </summary>
    /// <param name="teamManagerRepository">The football team manager repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateFootballTeamManagerHandler(
        IFootballTeamManagerRepository teamManagerRepository, 
        IPersonRepository personRepository,
        IFootballUnitOfWork unitOfWork, 
        ILogger<CreateFootballTeamManagerHandler> logger)
    {
        _teamManagerRepository = teamManagerRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateFootballTeamManagerCommand request
    /// </summary>
    /// <param name="request">The command containing team manager information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created team manager as a DTO wrapped in a Result</returns>
    public async Task<Result<FootballTeamManagerDto>> Handle(CreateFootballTeamManagerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the person exists
            Person? person = await _personRepository.GetByIdAsync(request.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found", request.PersonId);
                return Result<FootballTeamManagerDto>.Failure("Person not found");
            }

            // Check if a team manager profile already exists for this person
            bool teamManagerExists = await _teamManagerRepository.ExistsByPersonIdAsync(request.PersonId);
            if (teamManagerExists)
            {
                _logger.LogWarning("Team manager profile already exists for person ID {PersonId}", request.PersonId);
                return Result<FootballTeamManagerDto>.Failure("A team manager profile already exists for this person");
            }

            // Create the team manager entity
            FootballTeamManager teamManager = FootballTeamManagerMapper.ToEntity(request);

            _logger.LogInformation("Creating new football team manager for person: {PersonId}", request.PersonId);
            await _teamManagerRepository.AddAsync(teamManager);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FootballTeamManagerDto teamManagerDto = FootballTeamManagerMapper.ToDto(teamManager);
            _logger.LogInformation("Successfully created football team manager with ID: {TeamManagerId}", teamManager.Id);

            return Result<FootballTeamManagerDto>.Success(teamManagerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating football team manager for person: {PersonId}", request.PersonId);
            return Result<FootballTeamManagerDto>.Failure("An error occurred while creating the football team manager.");
        }
    }
} 

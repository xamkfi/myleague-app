using Application.Features.Football.Players.Commands;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Application.Features.Football.Teams.Mappings;
using Application.Features.Football.Players.Mappings;
using Application.Features.Football.Referees.Mappings;
using Application.Features.Football.TeamManagers.Mappings;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Application.Common;
using Domain.Entities.Football.Teams;
using Domain.Entities.Common;
using Domain.Repositories.Football;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Football.Players.Handlers;

/// <summary>
/// Handler for creating a new football player
/// </summary>
public class CreateFootballPlayerHandler : IRequestHandler<CreateFootballPlayerCommand, Result<FootballPlayerDto>>
{
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFootballUnitOfWork _footballUnitOfWork;
    private readonly ILogger<CreateFootballPlayerHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateFootballPlayerHandler class
    /// </summary>
    /// <param name="playerRepository">The football player repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="footballUnitOfWork">The football unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateFootballPlayerHandler(
        IFootballPlayerRepository playerRepository, 
        IPersonRepository personRepository,
        IFootballUnitOfWork footballUnitOfWork, 
        ILogger<CreateFootballPlayerHandler> logger)
    {
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _footballUnitOfWork = footballUnitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateFootballPlayerCommand request
    /// </summary>
    /// <param name="request">The command containing the person ID for the new player</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created player as a DTO wrapped in a Result</returns>
    public async Task<Result<FootballPlayerDto>> Handle(CreateFootballPlayerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the person exists
            Person? person = await _personRepository.GetByIdAsync(request.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found", request.PersonId);
                return Result<FootballPlayerDto>.Failure("Person not found");
            }

            // Create the player entity
            FootballPlayer player = FootballPlayerMapper.ToEntity(request);
            player.SetPerson(person); // Set the person to the player

            _logger.LogInformation("Creating new football player for person: {PersonId}", request.PersonId);
            await _playerRepository.AddAsync(player);
            
            // Save changes explicitly to trigger domain events
            await _footballUnitOfWork.SaveChangesAsync(cancellationToken);

            // Create DTO with real person data (consistent with other handlers)
            FootballPlayerDto playerDto = new FootballPlayerDto(
                player.Id,
                player.PersonId,
                PersonMapper.ToDto(person),
                player.IsActive,
                player.Position.PrimaryPosition,
                player.CareerGoals,
                player.CareerAssists,
                null // New players don't have team assignments yet
            );
            
            _logger.LogInformation("Successfully created football player with ID: {PlayerId}", player.Id);

            return Result<FootballPlayerDto>.Success(playerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating football player for person: {PersonId}", request.PersonId);
            return Result<FootballPlayerDto>.Failure("An error occurred while creating the football player.");
        }
    }
} 

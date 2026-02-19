using Application.Features.Floorball.Players.Commands;
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
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Floorball.Players.Handlers;

/// <summary>
/// Handler for creating a new floorball player
/// </summary>
public class CreateFloorballPlayerHandler : IRequestHandler<CreateFloorballPlayerCommand, Result<FloorballPlayerDto>>
{
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFloorballUnitOfWork _floorballUnitOfWork;
    private readonly ILogger<CreateFloorballPlayerHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateFloorballPlayerHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="floorballUnitOfWork">The floorball unit of work</param>
    /// <param name="logger">The logger</param>
    public CreateFloorballPlayerHandler(
        IFloorballPlayerRepository playerRepository, 
        IPersonRepository personRepository,
        IFloorballUnitOfWork floorballUnitOfWork, 
        ILogger<CreateFloorballPlayerHandler> logger)
    {
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _floorballUnitOfWork = floorballUnitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateFloorballPlayerCommand request
    /// </summary>
    /// <param name="request">The command containing the person ID for the new player</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created player as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballPlayerDto>> Handle(CreateFloorballPlayerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the person exists
            Person? person = await _personRepository.GetByIdAsync(request.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found", request.PersonId);
                return Result<FloorballPlayerDto>.Failure("Person not found");
            }

            // Create the player entity
            FloorballPlayer player = FloorballPlayerMapper.ToEntity(request);
            player.SetPerson(person); // Set the person to the player

            _logger.LogInformation("Creating new floorball player for person: {PersonId}", request.PersonId);
            await _playerRepository.AddAsync(player);
            
            // Save changes explicitly to trigger domain events
            await _floorballUnitOfWork.SaveChangesAsync(cancellationToken);

            // Create DTO with real person data (consistent with other handlers)
            FloorballPlayerDto playerDto = new FloorballPlayerDto(
                player.Id,
                player.PersonId,
                PersonMapper.ToDto(person),
                player.IsActive,
                player.Position.PrimaryPosition,
                player.CareerGoals,
                player.CareerAssists,
                null // New players don't have team assignments yet
            );
            
            _logger.LogInformation("Successfully created floorball player with ID: {PlayerId}", player.Id);

            return Result<FloorballPlayerDto>.Success(playerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating floorball player for person: {PersonId}", request.PersonId);
            return Result<FloorballPlayerDto>.Failure("An error occurred while creating the floorball player.");
        }
    }
} 

using Application.Features.Football.Players.Queries;
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

namespace Application.Features.Football.Players.Handlers;

/// <summary>
/// Handler for retrieving a football player by ID
/// </summary>
public class GetFootballPlayerByIdHandler : IRequestHandler<GetFootballPlayerByIdQuery, Result<FootballPlayerDto>>
{
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetFootballPlayerByIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFootballPlayerByIdHandler class
    /// </summary>
    /// <param name="playerRepository">The football player repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="logger">The logger</param>
    public GetFootballPlayerByIdHandler(
        IFootballPlayerRepository playerRepository, 
        IPersonRepository personRepository,
        ILogger<GetFootballPlayerByIdHandler> logger)
    {
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFootballPlayerByIdQuery request
    /// </summary>
    /// <param name="request">The query containing the player ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The player as a DTO wrapped in a Result, or a not found result</returns>
    public async Task<Result<FootballPlayerDto>> Handle(GetFootballPlayerByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving football player with ID: {PlayerId}", request.Id);
            
            FootballPlayer? player = await _playerRepository.GetByIdAsync(request.Id);
            if (player == null)
            {
                _logger.LogWarning("Football player with ID {PlayerId} not found", request.Id);
                return Result<FootballPlayerDto>.NotFound("FootballPlayer", request.Id);
            }

            // Get the associated person
            Person? person = await _personRepository.GetByIdAsync(player.PersonId);
            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found for player {PlayerId}", player.PersonId, player.Id);
                return Result<FootballPlayerDto>.Failure("Associated person not found");
            }

            // Create DTO with real person data
            FootballPlayerDto playerDto = new FootballPlayerDto(
                player.Id,
                player.PersonId,
                PersonMapper.ToDto(person),
                player.IsActive,
                player.Position.PrimaryPosition,
                player.CareerGoals,
                player.CareerAssists
            );

            _logger.LogInformation("Successfully retrieved football player: {PlayerId}", player.Id);

            return Result<FootballPlayerDto>.Success(playerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving football player: {PlayerId}", request.Id);
            return Result<FootballPlayerDto>.Failure("An error occurred while retrieving the football player.");
        }
    }
} 

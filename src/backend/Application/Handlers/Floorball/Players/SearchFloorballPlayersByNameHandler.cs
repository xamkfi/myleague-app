using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Players;

/// <summary>
/// Handler for searching floorball players by name
/// </summary>
public class SearchFloorballPlayersByNameHandler : IRequestHandler<SearchFloorballPlayersByNameQuery, Result<IEnumerable<FloorballPlayerDto>>>
{
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly ILogger<SearchFloorballPlayersByNameHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the SearchFloorballPlayersByNameHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="logger">The logger</param>
    public SearchFloorballPlayersByNameHandler(
        IFloorballPlayerRepository playerRepository, 
        ILogger<SearchFloorballPlayersByNameHandler> logger)
    {
        _playerRepository = playerRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the SearchFloorballPlayersByNameQuery request
    /// </summary>
    /// <param name="request">The query containing the search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching floorball players as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballPlayerDto>>> Handle(SearchFloorballPlayersByNameQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Searching for floorball players with name containing: {SearchTerm}", request.SearchTerm);
            
            IEnumerable<FloorballPlayer> players = await _playerRepository.SearchByNameAsync(request.SearchTerm);
            if (!players.Any())
            {
                _logger.LogWarning("No floorball players found matching search term: {SearchTerm}", request.SearchTerm);
                return Result<IEnumerable<FloorballPlayerDto>>.NotFound("FloorballPlayer", request.SearchTerm);
            }

            IEnumerable<FloorballPlayerDto> playerDtos = FloorballPlayerMapper.ToDtos(players);
            _logger.LogInformation("Found {PlayerCount} floorball players matching search term: {SearchTerm}", 
                playerDtos.Count(), request.SearchTerm);

            return Result<IEnumerable<FloorballPlayerDto>>.Success(playerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching for floorball players with term: {SearchTerm}", request.SearchTerm);
            return Result<IEnumerable<FloorballPlayerDto>>.Failure("An error occurred while searching for floorball players.");
        }
    }
} 
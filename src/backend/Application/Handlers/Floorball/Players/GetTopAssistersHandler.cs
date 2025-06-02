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
/// Handler for retrieving top assisters in a season
/// </summary>
public class GetTopAssistersHandler : IRequestHandler<GetTopAssistersQuery, Result<IEnumerable<FloorballPlayerDto>>>
{
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly ILogger<GetTopAssistersHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetTopAssistersHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="logger">The logger</param>
    public GetTopAssistersHandler(
        IFloorballPlayerRepository playerRepository,
        IFloorballSeasonRepository seasonRepository,
        ILogger<GetTopAssistersHandler> logger)
    {
        _playerRepository = playerRepository;
        _seasonRepository = seasonRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetTopAssistersQuery request
    /// </summary>
    /// <param name="request">The query containing the season ID and count</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Top assisting players as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballPlayerDto>>> Handle(GetTopAssistersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify season exists
            bool seasonExists = await _seasonRepository.ExistsAsync(request.SeasonId);
            if (!seasonExists)
            {
                _logger.LogWarning("Attempt to get top assisters for non-existent season with ID: {SeasonId}", request.SeasonId);
                return Result<IEnumerable<FloorballPlayerDto>>.NotFound("FloorballSeason", request.SeasonId);
            }

            _logger.LogInformation("Retrieving top {Count} assisters for season: {SeasonId}", request.Count, request.SeasonId);
            
            IEnumerable<FloorballPlayer> players = await _playerRepository.GetTopAssistersAsync(request.SeasonId, request.Count);
            IEnumerable<FloorballPlayerDto> playerDtos = FloorballPlayerMapper.ToDtos(players);
            
            _logger.LogInformation("Successfully retrieved {PlayerCount} top assisters for season {SeasonId}", 
                playerDtos.Count(), request.SeasonId);
            
            return Result<IEnumerable<FloorballPlayerDto>>.Success(playerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving top assisters for season: {SeasonId}", request.SeasonId);
            return Result<IEnumerable<FloorballPlayerDto>>.Failure("An error occurred while retrieving top assisters.");
        }
    }
} 
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
/// Handler for retrieving top scorers in a season
/// </summary>
public class GetTopScorersHandler : IRequestHandler<GetTopScorersQuery, Result<IEnumerable<FloorballPlayerDto>>>
{
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly ILogger<GetTopScorersHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetTopScorersHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="logger">The logger</param>
    public GetTopScorersHandler(
        IFloorballPlayerRepository playerRepository,
        IFloorballSeasonRepository seasonRepository,
        ILogger<GetTopScorersHandler> logger)
    {
        _playerRepository = playerRepository;
        _seasonRepository = seasonRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetTopScorersQuery request
    /// </summary>
    /// <param name="request">The query containing the season ID and count</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Top scoring players as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballPlayerDto>>> Handle(GetTopScorersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify season exists
            bool seasonExists = await _seasonRepository.ExistsAsync(request.SeasonId);
            if (!seasonExists)
            {
                _logger.LogWarning("Attempt to get top scorers for non-existent season with ID: {SeasonId}", request.SeasonId);
                return Result<IEnumerable<FloorballPlayerDto>>.NotFound("FloorballSeason", request.SeasonId);
            }

            _logger.LogInformation("Retrieving top {Count} scorers for season: {SeasonId}", request.Count, request.SeasonId);
            
            IEnumerable<FloorballPlayer> players = await _playerRepository.GetTopScorersAsync(request.SeasonId, request.Count);
            IEnumerable<FloorballPlayerDto> playerDtos = FloorballPlayerMapper.ToDtos(players);
            
            _logger.LogInformation("Successfully retrieved {PlayerCount} top scorers for season {SeasonId}", 
                playerDtos.Count(), request.SeasonId);
            
            return Result<IEnumerable<FloorballPlayerDto>>.Success(playerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving top scorers for season: {SeasonId}", request.SeasonId);
            return Result<IEnumerable<FloorballPlayerDto>>.Failure("An error occurred while retrieving top scorers.");
        }
    }
} 
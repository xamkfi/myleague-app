using Application.Queries.Floorball.Season;
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

namespace Application.Handlers.Floorball.Seasons;

/// <summary>
/// Handler for retrieving active floorball seasons
/// </summary>
public class GetActiveFloorballSeasonsHandler : IRequestHandler<GetActiveFloorballSeasonsQuery, Result<IEnumerable<FloorballSeasonDto>>>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly ILogger<GetActiveFloorballSeasonsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetActiveFloorballSeasonsHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="logger">The logger</param>
    public GetActiveFloorballSeasonsHandler(
        IFloorballSeasonRepository seasonRepository,
        ILogger<GetActiveFloorballSeasonsHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetActiveFloorballSeasonsQuery request
    /// </summary>
    /// <param name="request">The query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active floorball seasons as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballSeasonDto>>> Handle(GetActiveFloorballSeasonsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving active floorball seasons");
            
            IEnumerable<FloorballSeason> seasons = await _seasonRepository.GetActiveAsync();
            IEnumerable<FloorballSeasonDto> seasonDtos = FloorballSeasonMapper.ToDtos(seasons);
            
            _logger.LogInformation("Successfully retrieved {SeasonCount} active floorball seasons", seasonDtos.Count());
            
            return Result<IEnumerable<FloorballSeasonDto>>.Success(seasonDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active floorball seasons");
            return Result<IEnumerable<FloorballSeasonDto>>.Failure("An error occurred while retrieving active floorball seasons.");
        }
    }
} 
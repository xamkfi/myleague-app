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

namespace Application.Handlers.Floorball.Seasons;

/// <summary>
/// Handler for retrieving all floorball seasons
/// </summary>
public class GetAllFloorballSeasonsHandler : IRequestHandler<GetAllFloorballSeasonsQuery, Result<IEnumerable<FloorballSeasonDto>>>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly ILogger<GetAllFloorballSeasonsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetAllFloorballSeasonsHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="logger">The logger</param>
    public GetAllFloorballSeasonsHandler(
        IFloorballSeasonRepository seasonRepository,
        ILogger<GetAllFloorballSeasonsHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetAllFloorballSeasonsQuery request
    /// </summary>
    /// <param name="request">The query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All floorball seasons as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballSeasonDto>>> Handle(GetAllFloorballSeasonsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving all floorball seasons");
            
            IEnumerable<FloorballSeason> seasons = await _seasonRepository.GetAllAsync();
            IEnumerable<FloorballSeasonDto> seasonDtos = FloorballSeasonMapper.ToDtos(seasons);
            
            _logger.LogInformation("Successfully retrieved {SeasonCount} floorball seasons", seasonDtos.Count());
            
            return Result<IEnumerable<FloorballSeasonDto>>.Success(seasonDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all floorball seasons");
            return Result<IEnumerable<FloorballSeasonDto>>.Failure("An error occurred while retrieving floorball seasons.");
        }
    }
} 
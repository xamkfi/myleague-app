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
/// Handler for retrieving floorball seasons by division
/// </summary>
public class GetFloorballSeasonsByDivisionHandler : IRequestHandler<GetFloorballSeasonsByDivisionQuery, Result<IEnumerable<FloorballSeasonDto>>>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly ILogger<GetFloorballSeasonsByDivisionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballSeasonsByDivisionHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballSeasonsByDivisionHandler(
        IFloorballSeasonRepository seasonRepository,
        ILogger<GetFloorballSeasonsByDivisionHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFloorballSeasonsByDivisionQuery request
    /// </summary>
    /// <param name="request">The query containing division</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Floorball seasons by division as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballSeasonDto>>> Handle(GetFloorballSeasonsByDivisionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving floorball seasons for division: {Division}", request.Division);
            
            IEnumerable<FloorballSeason> seasons = await _seasonRepository.GetByDivisionAsync(request.Division);
            IEnumerable<FloorballSeasonDto> seasonDtos = FloorballSeasonMapper.ToDtos(seasons);
            
            _logger.LogInformation("Successfully retrieved {SeasonCount} floorball seasons for division: {Division}", seasonDtos.Count(), request.Division);
            
            return Result<IEnumerable<FloorballSeasonDto>>.Success(seasonDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball seasons for division: {Division}", request.Division);
            return Result<IEnumerable<FloorballSeasonDto>>.Failure("An error occurred while retrieving floorball seasons.");
        }
    }
} 
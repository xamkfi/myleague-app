using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Seasons;

/// <summary>
/// Handler for retrieving the current floorball season
/// </summary>
public class GetCurrentFloorballSeasonHandler : IRequestHandler<GetCurrentFloorballSeasonQuery, Result<FloorballSeasonDto>>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly ILogger<GetCurrentFloorballSeasonHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetCurrentFloorballSeasonHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="logger">The logger</param>
    public GetCurrentFloorballSeasonHandler(
        IFloorballSeasonRepository seasonRepository,
        ILogger<GetCurrentFloorballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetCurrentFloorballSeasonQuery request
    /// </summary>
    /// <param name="request">The query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The current season as a DTO wrapped in a Result, or a not found result</returns>
    public async Task<Result<FloorballSeasonDto>> Handle(GetCurrentFloorballSeasonQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving current floorball season");
            
            FloorballSeason? currentSeason = await _seasonRepository.GetCurrentSeasonAsync();
            if (currentSeason == null)
            {
                _logger.LogWarning("No current floorball season found");
                return Result<FloorballSeasonDto>.NotFound("FloorballSeason", "current");
            }

            FloorballSeasonDto seasonDto = FloorballSeasonMapper.ToDto(currentSeason);
            _logger.LogInformation("Successfully retrieved current floorball season: {SeasonId}", currentSeason.Id);

            return Result<FloorballSeasonDto>.Success(seasonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving current floorball season");
            return Result<FloorballSeasonDto>.Failure("An error occurred while retrieving the current floorball season.");
        }
    }
} 
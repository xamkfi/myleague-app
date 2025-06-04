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
using Application.Queries.Floorball.Season;

namespace Application.Handlers.Floorball.Seasons;

/// <summary>
/// Handler for retrieving a floorball season by ID
/// </summary>
public class GetFloorballSeasonByIdHandler : IRequestHandler<GetFloorballSeasonByIdQuery, Result<FloorballSeasonDto>>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly ILogger<GetFloorballSeasonByIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballSeasonByIdHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballSeasonByIdHandler(
        IFloorballSeasonRepository seasonRepository,
        ILogger<GetFloorballSeasonByIdHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFloorballSeasonByIdQuery request
    /// </summary>
    /// <param name="request">The query containing the season ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The season as a DTO wrapped in a Result, or a not found result</returns>
    public async Task<Result<FloorballSeasonDto>> Handle(GetFloorballSeasonByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving floorball season with ID: {SeasonId}", request.Id);
            
            FloorballSeason? season = await _seasonRepository.GetByIdAsync(request.Id);
            if (season == null)
            {
                _logger.LogWarning("Floorball season with ID {SeasonId} not found", request.Id);
                return Result<FloorballSeasonDto>.NotFound("FloorballSeason", request.Id);
            }

            FloorballSeasonDto seasonDto = FloorballSeasonMapper.ToDto(season);
            _logger.LogInformation("Successfully retrieved floorball season: {SeasonId}", season.Id);

            return Result<FloorballSeasonDto>.Success(seasonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball season: {SeasonId}", request.Id);
            return Result<FloorballSeasonDto>.Failure("An error occurred while retrieving the floorball season.");
        }
    }
} 

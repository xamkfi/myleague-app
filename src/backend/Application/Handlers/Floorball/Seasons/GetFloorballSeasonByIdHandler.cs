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
using Domain.Entities.Common;
using Domain.Repositories.Common;

namespace Application.Handlers.Floorball.Seasons;

/// <summary>
/// Handler for retrieving a floorball season by ID
/// </summary>
public class GetFloorballSeasonByIdHandler : IRequestHandler<GetFloorballSeasonByIdQuery, Result<FloorballSeasonDto>>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IFloorballSeasonDivisionRepository _seasonDivisionRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetFloorballSeasonByIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballSeasonByIdHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballSeasonByIdHandler(
        IFloorballSeasonRepository seasonRepository,
        IFloorballSeasonDivisionRepository seasonDivisionRepository,
        IClubRepository clubRepository,
        ILogger<GetFloorballSeasonByIdHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _clubRepository = clubRepository;
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

            // Load clubs for all teams in the season
            Dictionary<Guid, Club> clubsDict = new Dictionary<Guid, Club>();
            foreach (FloorballTeam team in season.Teams)
            {
                Club? club = await _clubRepository.GetByIdAsync(team.ClubId);
                if (club != null)
                {
                    clubsDict[team.ClubId] = club;
                }
            }

            FloorballSeasonDto seasonDto = await FloorballSeasonMapper.ToDtoAsync(season, _seasonDivisionRepository, clubsDict);
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

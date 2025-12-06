using Application.Queries.Floorball.Season;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Seasons;

/// <summary>
/// Handler for retrieving active floorball seasons
/// </summary>
public class GetActiveFloorballSeasonsHandler : IRequestHandler<GetActiveFloorballSeasonsQuery, Result<IEnumerable<FloorballSeasonDto>>>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IFloorballSeasonDivisionRepository _seasonDivisionRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetActiveFloorballSeasonsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetActiveFloorballSeasonsHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="logger">The logger</param>
    public GetActiveFloorballSeasonsHandler(
        IFloorballSeasonRepository seasonRepository,
        IFloorballSeasonDivisionRepository seasonDivisionRepository,
        IClubRepository clubRepository,
        ILogger<GetActiveFloorballSeasonsHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _clubRepository = clubRepository;
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

            // Load clubs for all teams across all seasons
            Dictionary<Guid, Club> clubsDict = new Dictionary<Guid, Club>();
            HashSet<Guid> allClubIds = seasons
                .SelectMany(s => s.Teams)
                .Select(t => t.ClubId)
                .Distinct()
                .ToHashSet();

            foreach (Guid clubId in allClubIds)
            {
                Club? club = await _clubRepository.GetByIdAsync(clubId);
                if (club != null)
                {
                    clubsDict[clubId] = club;
                }
            }

            IEnumerable<FloorballSeasonDto> seasonDtos = await FloorballSeasonMapper.ToDtosAsync(seasons, _seasonDivisionRepository, clubsDict);
            
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
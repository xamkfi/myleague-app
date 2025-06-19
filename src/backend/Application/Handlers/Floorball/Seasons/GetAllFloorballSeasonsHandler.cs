using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Application.Queries.Floorball.Season;

namespace Application.Handlers.Floorball.Seasons;

/// <summary>
/// Handler for retrieving all floorball seasons
/// </summary>
public class GetAllFloorballSeasonsHandler : IRequestHandler<GetAllFloorballSeasonsQuery, Result<IEnumerable<FloorballSeasonDto>>>
{
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetAllFloorballSeasonsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetAllFloorballSeasonsHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public GetAllFloorballSeasonsHandler(
        IFloorballSeasonRepository seasonRepository,
        IClubRepository clubRepository,
        ILogger<GetAllFloorballSeasonsHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _clubRepository = clubRepository;
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
            
            IEnumerable<FloorballSeasonDto> seasonDtos = FloorballSeasonMapper.ToDtos(seasons, clubsDict);
            
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

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
using Domain.Entities.Common;
using Domain.Repositories.Common;
using System.Linq;

namespace Application.Handlers.Floorball.Seasons;

/// <summary>
/// Handler for retrieving floorball seasons by division
/// </summary>
public class GetFloorballSeasonsByDivisionHandler : IRequestHandler<GetFloorballSeasonsByDivisionQuery, Result<IEnumerable<FloorballSeasonDto>>>
{
    private readonly IFloorballSeasonDivisionRepository _seasonDivisionRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetFloorballSeasonsByDivisionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballSeasonsByDivisionHandler class
    /// </summary>
    /// <param name="seasonDivisionRepository">The floorball season division repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballSeasonsByDivisionHandler(
        IFloorballSeasonDivisionRepository seasonDivisionRepository,
        IClubRepository clubRepository,
        ILogger<GetFloorballSeasonsByDivisionHandler> logger)
    {
        _seasonDivisionRepository = seasonDivisionRepository;
        _clubRepository = clubRepository;
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
            _logger.LogInformation("Retrieving floorball seasons for division: {DivisionId}", request.DivisionId);
            
            IEnumerable<FloorballSeason> seasons = await _seasonDivisionRepository.GetSeasonsByDivisionAsync(request.DivisionId);

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
            
            _logger.LogInformation("Successfully retrieved {SeasonCount} floorball seasons for division: {DivisionId}", seasonDtos.Count(), request.DivisionId);
            
            return Result<IEnumerable<FloorballSeasonDto>>.Success(seasonDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball seasons for division: {DivisionId}", request.DivisionId);
            return Result<IEnumerable<FloorballSeasonDto>>.Failure("An error occurred while retrieving floorball seasons.");
        }
    }
} 

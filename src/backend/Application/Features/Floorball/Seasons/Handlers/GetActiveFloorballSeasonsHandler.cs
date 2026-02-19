using Application.Features.Floorball.Seasons.Queries;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
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

namespace Application.Features.Floorball.Seasons.Handlers;

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
            List<FloorballSeason> seasonList = seasons.ToList();

            // Load clubs for all teams across all seasons
            Dictionary<Guid, Club> clubsDict = new Dictionary<Guid, Club>();
            HashSet<Guid> allClubIds = seasonList
                .SelectMany(s => s.Teams)
                .Select(t => t.ClubId)
                .Distinct()
                .ToHashSet();

            // Include clubs for teams linked via season divisions
            Dictionary<Guid, List<FloorballTeam>> seasonTeamsBySeason = new Dictionary<Guid, List<FloorballTeam>>();
            foreach (FloorballSeason season in seasonList)
            {
                IEnumerable<FloorballSeasonDivisionTeam> seasonDivisionTeams = await _seasonDivisionRepository.GetSeasonDivisionTeamsAsync(season.Id);
                List<FloorballTeam> divisionTeams = seasonDivisionTeams.Select(sdt => sdt.Team).Where(team => team != null).ToList();
                seasonTeamsBySeason[season.Id] = divisionTeams;
                foreach (FloorballTeam team in divisionTeams)
                {
                    allClubIds.Add(team.ClubId);
                }
            }

            foreach (Guid clubId in allClubIds)
            {
                Club? club = await _clubRepository.GetByIdAsync(clubId);
                if (club != null)
                {
                    clubsDict[clubId] = club;
                }
            }

            Dictionary<Guid, IReadOnlyCollection<FloorballSeasonDivisionDto>> seasonDivisionsBySeason = new Dictionary<Guid, IReadOnlyCollection<FloorballSeasonDivisionDto>>();
            foreach (FloorballSeason season in seasonList)
            {
                IEnumerable<FloorballSeasonDivision> seasonDivisions = await _seasonDivisionRepository.GetSeasonDivisionsAsync(season.Id);
                seasonDivisionsBySeason[season.Id] = FloorballSeasonMapper.ToDivisionDtos(seasonDivisions);
            }

            List<FloorballSeasonDto> seasonDtos = new List<FloorballSeasonDto>();
            foreach (FloorballSeason season in seasonList)
            {
                seasonDivisionsBySeason.TryGetValue(season.Id, out IReadOnlyCollection<FloorballSeasonDivisionDto>? seasonDivisions);
                IReadOnlyCollection<FloorballSeasonDivisionDto> safeSeasonDivisions = seasonDivisions ?? Array.Empty<FloorballSeasonDivisionDto>();
                seasonTeamsBySeason.TryGetValue(season.Id, out List<FloorballTeam>? seasonTeams);
                IEnumerable<FloorballTeam> safeSeasonTeams = (seasonTeams ?? Enumerable.Empty<FloorballTeam>()).Concat(season.Teams).Distinct();
                FloorballSeasonDto dto = FloorballSeasonMapper.ToDto(season, safeSeasonDivisions, clubsDict, safeSeasonTeams);
                seasonDtos.Add(dto);
            }
            
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
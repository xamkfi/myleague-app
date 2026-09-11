using Application.Features.Floorball.Players.Queries;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
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
using Application.Features.Floorball.Seasons.Queries;

namespace Application.Features.Floorball.Seasons.Handlers;

/// <summary>
/// Handler for retrieving all floorball seasons
/// </summary>
public class GetAllFloorballSeasonsHandler : IRequestHandler<GetAllFloorballSeasonsQuery, Result<IEnumerable<FloorballSeasonDto>>>
{
    private readonly IFloorballCompetitionRepository _seasonRepository;
    private readonly IFloorballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetAllFloorballSeasonsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetAllFloorballSeasonsHandler class
    /// </summary>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="seasonDivisionRepository">The floorball season division repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public GetAllFloorballSeasonsHandler(
        IFloorballCompetitionRepository seasonRepository,
        IFloorballCompetitionDivisionRepository seasonDivisionRepository,
        IClubRepository clubRepository,
        ILogger<GetAllFloorballSeasonsHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
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
            
            IEnumerable<FloorballCompetition> competitions = await _seasonRepository.GetAllAsync();
            // FloorballCompetition uses Table-Per-Hierarchy; only return league seasons here,
            // not tournaments (which are managed via FloorballTournamentController).
            List<FloorballCompetition> seasonList = competitions.OfType<FloorballSeason>().Cast<FloorballCompetition>().ToList();

            // Load clubs for all teams across all seasons
            Dictionary<Guid, Club> clubsDict = new Dictionary<Guid, Club>();
            HashSet<Guid> allClubIds = seasonList
                .SelectMany(s => s.Teams)
                .Select(t => t.ClubId)
                .Distinct()
                .ToHashSet();

            // Load season division teams to include teams assigned via FloorballCompetitionDivisionTeam
            Dictionary<Guid, List<FloorballTeam>> seasonTeamsBySeason = new Dictionary<Guid, List<FloorballTeam>>();
            foreach (FloorballCompetition season in seasonList)
            {
                IEnumerable<FloorballCompetitionDivisionTeam> seasonDivisionTeams = await _seasonDivisionRepository.GetCompetitionDivisionTeamsAsync(season.Id);
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
            foreach (FloorballCompetition season in seasonList)
            {
                IEnumerable<FloorballCompetitionDivision> seasonDivisions = await _seasonDivisionRepository.GetCompetitionDivisionsAsync(season.Id);
                seasonDivisionsBySeason[season.Id] = FloorballSeasonMapper.ToDivisionDtos(seasonDivisions);
            }

            List<FloorballSeasonDto> seasonDtos = new List<FloorballSeasonDto>();
            foreach (FloorballCompetition season in seasonList)
            {
                seasonDivisionsBySeason.TryGetValue(season.Id, out IReadOnlyCollection<FloorballSeasonDivisionDto>? seasonDivisions);
                IReadOnlyCollection<FloorballSeasonDivisionDto> safeSeasonDivisions = seasonDivisions ?? Array.Empty<FloorballSeasonDivisionDto>();
                seasonTeamsBySeason.TryGetValue(season.Id, out List<FloorballTeam>? seasonTeams);
                IEnumerable<FloorballTeam> safeSeasonTeams = (seasonTeams ?? Enumerable.Empty<FloorballTeam>()).Concat(season.Teams).Distinct();
                FloorballSeasonDto dto = FloorballSeasonMapper.ToDto(season, safeSeasonDivisions, clubsDict, safeSeasonTeams);
                seasonDtos.Add(dto);
            }
            
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

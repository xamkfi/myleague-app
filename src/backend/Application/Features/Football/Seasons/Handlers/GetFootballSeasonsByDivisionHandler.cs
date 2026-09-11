using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Seasons.Mappings;
using Application.Features.Football.Seasons.Queries;
using Domain.Entities.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Seasons.Handlers;

public class GetFootballSeasonsByDivisionHandler
    : IRequestHandler<GetFootballSeasonsByDivisionQuery, Result<IEnumerable<FootballSeasonDto>>>
{
    private readonly IFootballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetFootballSeasonsByDivisionHandler> _logger;

    public GetFootballSeasonsByDivisionHandler(
        IFootballCompetitionDivisionRepository seasonDivisionRepository,
        IClubRepository clubRepository,
        ILogger<GetFootballSeasonsByDivisionHandler> logger)
    {
        _seasonDivisionRepository = seasonDivisionRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<FootballSeasonDto>>> Handle(
        GetFootballSeasonsByDivisionQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving football seasons for division: {DivisionId}", request.DivisionId);

            IEnumerable<FootballCompetition> competitions =
                await _seasonDivisionRepository.GetCompetitionsByDivisionAsync(request.DivisionId);
            List<FootballCompetition> seasonList = competitions.OfType<FootballSeason>().Cast<FootballCompetition>().ToList();

            Dictionary<Guid, Club> clubsDict = new();
            HashSet<Guid> allClubIds = seasonList.SelectMany(s => s.Teams).Select(t => t.ClubId).ToHashSet();
            Dictionary<Guid, List<FootballTeam>> seasonTeamsBySeason = new();

            foreach (FootballCompetition season in seasonList)
            {
                IEnumerable<FootballCompetitionDivisionTeam> seasonDivisionTeams =
                    await _seasonDivisionRepository.GetCompetitionDivisionTeamsAsync(season.Id);
                List<FootballTeam> divisionTeams = seasonDivisionTeams.Select(sdt => sdt.Team).Where(team => team != null).ToList();
                seasonTeamsBySeason[season.Id] = divisionTeams;
                foreach (FootballTeam team in divisionTeams)
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

            Dictionary<Guid, IReadOnlyCollection<FootballSeasonDivisionDto>> seasonDivisionsBySeason = new();
            foreach (FootballCompetition season in seasonList)
            {
                IEnumerable<FootballCompetitionDivision> seasonDivisions =
                    await _seasonDivisionRepository.GetCompetitionDivisionsAsync(season.Id);
                seasonDivisionsBySeason[season.Id] = FootballSeasonMapper.ToDivisionDtos(seasonDivisions);
            }

            List<FootballSeasonDto> seasonDtos = new();
            foreach (FootballCompetition season in seasonList)
            {
                seasonDivisionsBySeason.TryGetValue(season.Id, out IReadOnlyCollection<FootballSeasonDivisionDto>? seasonDivisions);
                IReadOnlyCollection<FootballSeasonDivisionDto> safeSeasonDivisions =
                    seasonDivisions ?? Array.Empty<FootballSeasonDivisionDto>();
                seasonTeamsBySeason.TryGetValue(season.Id, out List<FootballTeam>? seasonTeams);
                IEnumerable<FootballTeam> safeSeasonTeams =
                    (seasonTeams ?? Enumerable.Empty<FootballTeam>()).Concat(season.Teams).Distinct();
                seasonDtos.Add(FootballSeasonMapper.ToDto(season, safeSeasonDivisions, clubsDict, safeSeasonTeams));
            }

            _logger.LogInformation(
                "Successfully retrieved {SeasonCount} football seasons for division: {DivisionId}",
                seasonDtos.Count,
                request.DivisionId);

            return Result<IEnumerable<FootballSeasonDto>>.Success(seasonDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving football seasons for division: {DivisionId}", request.DivisionId);
            return Result<IEnumerable<FootballSeasonDto>>.Failure("An error occurred while retrieving football seasons.");
        }
    }
}

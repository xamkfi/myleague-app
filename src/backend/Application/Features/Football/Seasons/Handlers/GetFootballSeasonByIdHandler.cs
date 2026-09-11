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

public class GetFootballSeasonByIdHandler : IRequestHandler<GetFootballSeasonByIdQuery, Result<FootballSeasonDto>>
{
    private readonly IFootballCompetitionRepository _seasonRepository;
    private readonly IFootballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetFootballSeasonByIdHandler> _logger;

    public GetFootballSeasonByIdHandler(
        IFootballCompetitionRepository seasonRepository,
        IFootballCompetitionDivisionRepository seasonDivisionRepository,
        IClubRepository clubRepository,
        ILogger<GetFootballSeasonByIdHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    public async Task<Result<FootballSeasonDto>> Handle(GetFootballSeasonByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving football season with ID: {SeasonId}", request.Id);

            FootballCompetition? season = await _seasonRepository.GetByIdAsync(request.Id);
            if (season == null)
            {
                _logger.LogWarning("Football season with ID {SeasonId} not found", request.Id);
                return Result<FootballSeasonDto>.NotFound("FootballSeason", request.Id);
            }

            Dictionary<Guid, Club> clubsDict = new();
            foreach (FootballTeam team in season.Teams)
            {
                Club? club = await _clubRepository.GetByIdAsync(team.ClubId);
                if (club != null)
                {
                    clubsDict[team.ClubId] = club;
                }
            }

            IEnumerable<FootballCompetitionDivision> seasonDivisions =
                await _seasonDivisionRepository.GetCompetitionDivisionsAsync(season.Id);
            IReadOnlyCollection<FootballSeasonDivisionDto> seasonDivisionDtos =
                FootballSeasonMapper.ToDivisionDtos(seasonDivisions);

            IEnumerable<FootballCompetitionDivisionTeam> seasonDivisionTeams =
                await _seasonDivisionRepository.GetCompetitionDivisionTeamsAsync(season.Id);
            List<FootballTeam> seasonTeams = seasonDivisionTeams
                .Select(sdt => sdt.Team)
                .Where(team => team != null)
                .ToList();

            foreach (FootballTeam team in seasonTeams)
            {
                if (team.Club != null && !clubsDict.ContainsKey(team.ClubId))
                {
                    clubsDict[team.ClubId] = team.Club;
                }
                else if (!clubsDict.ContainsKey(team.ClubId))
                {
                    Club? club = await _clubRepository.GetByIdAsync(team.ClubId);
                    if (club != null)
                    {
                        clubsDict[team.ClubId] = club;
                    }
                }
            }

            FootballSeasonDto seasonDto = FootballSeasonMapper.ToDto(season, seasonDivisionDtos, clubsDict, seasonTeams);
            _logger.LogInformation("Successfully retrieved football season: {SeasonId}", season.Id);

            return Result<FootballSeasonDto>.Success(seasonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving football season: {SeasonId}", request.Id);
            return Result<FootballSeasonDto>.Failure("An error occurred while retrieving the football season.");
        }
    }
}

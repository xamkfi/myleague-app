using Application.Common;
using Application.Features.Football.Competitions.Commands;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Seasons.Mappings;
using Domain.Entities.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Statistics;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Competitions.Handlers;

public class AddTeamToSeasonHandler : IRequestHandler<AddFootballTeamToSeasonCommand, Result<FootballSeasonDto>>
{
    private readonly IFootballCompetitionRepository _seasonRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IFootballUnitOfWork _footballUnitOfWork;
    private readonly IFootballStatisticsRepository _footballStatisticsRepository;
    private readonly ILogger<AddTeamToSeasonHandler> _logger;

    public AddTeamToSeasonHandler(
        IFootballCompetitionRepository seasonRepository,
        IFootballTeamRepository teamRepository,
        IFootballCompetitionDivisionRepository seasonDivisionRepository,
        IClubRepository clubRepository,
        IFootballUnitOfWork footballUnitOfWork,
        IFootballStatisticsRepository footballStatisticsRepository,
        ILogger<AddTeamToSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _teamRepository = teamRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _clubRepository = clubRepository;
        _footballUnitOfWork = footballUnitOfWork;
        _footballStatisticsRepository = footballStatisticsRepository;
        _logger = logger;
    }

    public async Task<Result<FootballSeasonDto>> Handle(AddFootballTeamToSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballCompetition? season = await _seasonRepository.GetByIdAsync(request.CompetitionId);
            if (season == null)
            {
                _logger.LogWarning("Season not found with ID: {SeasonId}", request.CompetitionId);
                return Result<FootballSeasonDto>.NotFound("FootballSeason", request.CompetitionId);
            }

            FootballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team not found with ID: {TeamId}", request.TeamId);
                return Result<FootballSeasonDto>.NotFound("FootballTeam", request.TeamId);
            }

            _logger.LogInformation("Adding team {TeamId} to season {SeasonId}", request.TeamId, request.CompetitionId);
            season.AddTeam(team);
            Application.Features.Common.Shared.RosterEnrollment.Apply(team, request.CompetitionId, request.RosterMode);

            FootballTeamSeasonStatistics teamStatistics = new(team.Id, request.CompetitionId);
            await _footballStatisticsRepository.SaveTeamSeasonStatisticsAsync(teamStatistics, cancellationToken);

            foreach (FootballTeamPlayer player in team.GetActiveRoster(request.CompetitionId))
            {
                FootballPlayerSeasonStatistics playerSeasonStatistics = new(
                    player.PlayerId,
                    request.TeamId,
                    request.CompetitionId);
                await _footballStatisticsRepository.SavePlayerSeasonStatisticsAsync(playerSeasonStatistics, cancellationToken);
            }

            await _footballUnitOfWork.SaveChangesAsync(cancellationToken);

            Dictionary<Guid, Club> clubsDict = new();
            foreach (FootballTeam seasonTeam in season.Teams)
            {
                Club? club = await _clubRepository.GetByIdAsync(seasonTeam.ClubId);
                if (club != null)
                {
                    clubsDict[seasonTeam.ClubId] = club;
                }
            }

            IEnumerable<FootballCompetitionDivision> seasonDivisions =
                await _seasonDivisionRepository.GetCompetitionDivisionsAsync(season.Id);
            IReadOnlyCollection<FootballSeasonDivisionDto> seasonDivisionDtos =
                FootballSeasonMapper.ToDivisionDtos(seasonDivisions);
            FootballSeasonDto seasonDto = FootballSeasonMapper.ToDto(season, seasonDivisionDtos, clubsDict);
            _logger.LogInformation("Successfully added team {TeamId} to season {SeasonId}", request.TeamId, request.CompetitionId);

            return Result<FootballSeasonDto>.Success(seasonDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while adding team {TeamId} to season {SeasonId}", request.TeamId, request.CompetitionId);
            return Result<FootballSeasonDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding team {TeamId} to season {SeasonId}", request.TeamId, request.CompetitionId);
            return Result<FootballSeasonDto>.Failure("An error occurred while adding the team to the season.");
        }
    }
}

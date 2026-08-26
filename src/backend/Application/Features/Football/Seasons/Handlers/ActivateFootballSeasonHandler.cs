using Application.Common;
using Application.Features.Football.Seasons.Commands;
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

namespace Application.Features.Football.Seasons.Handlers;

public class ActivateFootballSeasonHandler : IRequestHandler<ActivateFootballSeasonCommand, Result<FootballSeasonDto>>
{
    private readonly IFootballCompetitionRepository _seasonRepository;
    private readonly IFootballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFootballUnitOfWork _footballUnitOfWork;
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly ILogger<ActivateFootballSeasonHandler> _logger;

    public ActivateFootballSeasonHandler(
        IFootballCompetitionRepository seasonRepository,
        IFootballCompetitionDivisionRepository seasonDivisionRepository,
        IClubRepository clubRepository,
        IUnitOfWork unitOfWork,
        IFootballUnitOfWork footballUnitOfWork,
        IFootballStatisticsRepository statisticsRepository,
        ILogger<ActivateFootballSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _footballUnitOfWork = footballUnitOfWork;
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public async Task<Result<FootballSeasonDto>> Handle(ActivateFootballSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballCompetition? season = await _seasonRepository.GetByIdAsync(request.Id);
            if (season == null)
            {
                _logger.LogWarning("Season not found with ID: {SeasonId}", request.Id);
                return Result<FootballSeasonDto>.NotFound("FootballSeason", request.Id);
            }

            _logger.LogInformation("Activating football season: {SeasonId}", request.Id);
            season.Activate();

            foreach (FootballTeam team in season.Teams)
            {
                FootballTeamSeasonStatistics teamStatistics = new(team.Id, request.Id);
                await _statisticsRepository.SaveTeamSeasonStatisticsAsync(teamStatistics, cancellationToken);
            }

            await _seasonRepository.UpdateAsync(season);
            await _footballUnitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            Dictionary<Guid, Club> clubsDict = await LoadClubsAsync(season.Teams);
            IEnumerable<FootballCompetitionDivision> seasonDivisions =
                await _seasonDivisionRepository.GetCompetitionDivisionsAsync(season.Id);
            IReadOnlyCollection<FootballSeasonDivisionDto> seasonDivisionDtos =
                FootballSeasonMapper.ToDivisionDtos(seasonDivisions);
            FootballSeasonDto seasonDto = FootballSeasonMapper.ToDto(season, seasonDivisionDtos, clubsDict);
            _logger.LogInformation("Successfully activated football season: {SeasonId}", request.Id);

            return Result<FootballSeasonDto>.Success(seasonDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while activating football season: {SeasonId}", request.Id);
            return Result<FootballSeasonDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while activating football season: {SeasonId}", request.Id);
            return Result<FootballSeasonDto>.Failure("An error occurred while activating the season.");
        }
    }

    private async Task<Dictionary<Guid, Club>> LoadClubsAsync(IEnumerable<FootballTeam> teams)
    {
        Dictionary<Guid, Club> clubsDict = new();
        foreach (FootballTeam team in teams)
        {
            Club? club = await _clubRepository.GetByIdAsync(team.ClubId);
            if (club != null)
            {
                clubsDict[team.ClubId] = club;
            }
        }

        return clubsDict;
    }
}

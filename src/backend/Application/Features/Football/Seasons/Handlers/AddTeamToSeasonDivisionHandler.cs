using Application.Common;
using Application.Features.Common.Shared;
using Application.Features.Football.Seasons.Commands;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Statistics;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Seasons.Handlers;

public class AddTeamToSeasonDivisionHandler : IRequestHandler<AddTeamToSeasonDivisionCommand, Result>
{
    private readonly IFootballCompetitionRepository _seasonRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<AddTeamToSeasonDivisionHandler> _logger;

    public AddTeamToSeasonDivisionHandler(
        IFootballCompetitionRepository seasonRepository,
        IFootballTeamRepository teamRepository,
        IFootballCompetitionDivisionRepository seasonDivisionRepository,
        IFootballStatisticsRepository statisticsRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<AddTeamToSeasonDivisionHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _teamRepository = teamRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(AddTeamToSeasonDivisionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballCompetition? season = await _seasonRepository.GetByIdAsync(request.CompetitionId);
            if (season == null)
                return Result.NotFound("FootballSeason", request.CompetitionId);

            FootballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
                return Result.NotFound("FootballTeam", request.TeamId);

            _logger.LogInformation(
                "Adding team {TeamId} to season {SeasonId} division {DivisionId} ({RosterMode})",
                request.TeamId, request.CompetitionId, request.DivisionId, request.RosterMode);

            season.AddTeam(team);
            RosterEnrollment.Apply(team, request.CompetitionId, request.RosterMode);

            if (season.IsActive)
            {
                FootballTeamSeasonStatistics teamStatistics = new(team.Id, request.CompetitionId);
                await _statisticsRepository.SaveTeamSeasonStatisticsAsync(teamStatistics, cancellationToken);
            }

            foreach (FootballTeamPlayer player in team.GetActiveRoster(request.CompetitionId))
            {
                FootballPlayerSeasonStatistics playerSeasonStatistics = new(
                    player.PlayerId, request.TeamId, request.CompetitionId);
                await _statisticsRepository.SavePlayerSeasonStatisticsAsync(playerSeasonStatistics, cancellationToken);
            }

            await _seasonDivisionRepository.AddTeamToCompetitionDivisionAsync(
                request.CompetitionId, request.DivisionId, request.TeamId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation adding team {TeamId} to season division", request.TeamId);
            return Result.Failure(ex.Message);
        }
    }
}

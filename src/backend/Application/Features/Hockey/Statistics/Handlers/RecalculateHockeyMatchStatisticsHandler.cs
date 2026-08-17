using Application.Common;
using Application.Features.Hockey.Statistics.Commands;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Statistics;
using Domain.Repositories.Hockey;
using Domain.Services.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Statistics.Handlers;

/// <summary>
/// Recalculates match-level hockey statistics from events.
/// </summary>
public class RecalculateHockeyMatchStatisticsHandler
    : IRequestHandler<RecalculateHockeyMatchStatisticsCommand, Result>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RecalculateHockeyMatchStatisticsHandler> _logger;

    public RecalculateHockeyMatchStatisticsHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyTeamRepository teamRepository,
        IHockeyStatisticsRepository statisticsRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RecalculateHockeyMatchStatisticsHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(
        RecalculateHockeyMatchStatisticsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdForStatisticsAsync(request.MatchId);
            if (match is null)
                return Result.NotFound("HockeyMatch", request.MatchId);

            await HockeyStatisticsHandlerSupport.AttachTeamPlayersAsync(match, _teamRepository);

            List<HockeyMatchTeamStatistics> teams = new();
            List<HockeyMatchPlayerStatistics> players = new();
            List<HockeyGoalieMatchStatistics> goalies = new();

            foreach (HockeyMatchTeam matchTeam in match.MatchTeams)
            {
                teams.Add(HockeyStatisticsCalculationService.BuildMatchTeamStatistics(match, matchTeam));
                if (matchTeam.PlayerSelection is null)
                    continue;

                players.AddRange(HockeyStatisticsCalculationService.BuildMatchPlayerStatistics(match, matchTeam));
                goalies.AddRange(HockeyStatisticsCalculationService.BuildGoalieMatchStatistics(match, matchTeam));
            }

            await _statisticsRepository.ReplaceMatchStatisticsAsync(match.Id, teams, players, goalies);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed RecalculateHockeyMatchStatistics for {MatchId}", request.MatchId);
            return Result.Failure("An error occurred while recalculating match statistics.", ex.Flatten());
        }
    }
}

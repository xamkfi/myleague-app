using Application.Common;
using Application.Features.Hockey.Statistics.Commands;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Statistics;
using Domain.Enums.Hockey.Statistics;
using Domain.Repositories.Hockey;
using Domain.Services.Hockey;
using Domain.ValueObjects.Hockey.Rules;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Statistics.Handlers;

/// <summary>
/// Recalculates competition aggregate hockey statistics for a scope.
/// </summary>
public class RecalculateHockeyCompetitionStatisticsHandler
    : IRequestHandler<RecalculateHockeyCompetitionStatisticsCommand, Result>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly HockeyStatisticsCalculationService _calculationService;
    private readonly ILogger<RecalculateHockeyCompetitionStatisticsHandler> _logger;

    public RecalculateHockeyCompetitionStatisticsHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyMatchRepository matchRepository,
        IHockeyTeamRepository teamRepository,
        IHockeyStatisticsRepository statisticsRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RecalculateHockeyCompetitionStatisticsHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _calculationService = new HockeyStatisticsCalculationService();
        _logger = logger;
    }

    public async Task<Result> Handle(
        RecalculateHockeyCompetitionStatisticsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyStatisticsHandlerSupport.ValidateScopeIds(
                request.Scope,
                request.CompetitionDivisionId,
                request.TournamentGroupId,
                request.PlayoffSeriesId);

            HockeyCompetition? competition = await _competitionRepository.GetByIdAsync(request.CompetitionId);
            if (competition is null)
                return Result.NotFound("HockeyCompetition", request.CompetitionId);

            IReadOnlyList<HockeyMatch> allMatches =
                await _matchRepository.GetByCompetitionIdForStatisticsAsync(request.CompetitionId);

            List<HockeyMatch> scopedMatches = allMatches
                .Where(m => HockeyStatisticsHandlerSupport.MatchesScope(
                    m,
                    request.Scope,
                    request.CompetitionDivisionId,
                    request.TournamentGroupId,
                    request.PlayoffSeriesId))
                .ToList();

            List<HockeyMatchTeamStatistics> matchTeamStats = new();
            List<HockeyMatchPlayerStatistics> matchPlayerStats = new();
            List<HockeyGoalieMatchStatistics> matchGoalieStats = new();

            foreach (HockeyMatch match in scopedMatches)
            {
                await HockeyStatisticsHandlerSupport.AttachTeamPlayersAsync(match, _teamRepository);

                foreach (HockeyMatchTeam matchTeam in match.MatchTeams)
                {
                    if (match.CountsTowardTeamStatistics)
                        matchTeamStats.Add(_calculationService.BuildMatchTeamStatistics(match, matchTeam));

                    if (matchTeam.PlayerSelection is null)
                        continue;

                    if (match.CountsTowardPlayerStatistics)
                        matchPlayerStats.AddRange(_calculationService.BuildMatchPlayerStatistics(match, matchTeam));

                    if (match.CountsTowardGoalieStatistics)
                        matchGoalieStats.AddRange(_calculationService.BuildGoalieMatchStatistics(match, matchTeam));
                }
            }

            List<HockeyMatch> standingsMatches = scopedMatches
                .Where(m => m.CountsTowardStandings && m.ResultType is not null)
                .ToList();

            HockeyStandingRules standingRules = competition.GetEffectiveRules().StandingRules;

            HashSet<Guid> teamIds = standingsMatches
                .SelectMany(m => m.MatchTeams)
                .Select(t => t.TeamId)
                .ToHashSet();

            List<HockeyTeamCompetitionStatistics> teamAggregates = teamIds
                .Select(teamId => _calculationService.AggregateTeamCompetitionStatistics(
                    teamId,
                    request.CompetitionId,
                    request.Scope,
                    standingsMatches.Where(m => m.MatchTeams.Any(t => t.TeamId == teamId)),
                    matchTeamStats,
                    standingRules,
                    request.CompetitionDivisionId,
                    request.TournamentGroupId,
                    request.PlayoffSeriesId))
                .ToList();

            HockeyStatisticsHandlerSupport.AssignStandingRanks(teamAggregates);

            List<HockeyPlayerCompetitionStatistics> playerAggregates = matchPlayerStats
                .GroupBy(s => new { s.PlayerId, s.TeamId, s.TeamPlayerId })
                .Select(g => _calculationService.AggregatePlayerCompetitionStatistics(
                    g.Key.PlayerId,
                    g.Key.TeamId,
                    g.Key.TeamPlayerId,
                    request.CompetitionId,
                    request.Scope,
                    g,
                    request.CompetitionDivisionId,
                    request.TournamentGroupId,
                    request.PlayoffSeriesId))
                .ToList();

            List<HockeyGoalieCompetitionStatistics> goalieAggregates = matchGoalieStats
                .GroupBy(s => new { s.PlayerId, s.TeamId, s.TeamPlayerId })
                .Select(g => _calculationService.AggregateGoalieCompetitionStatistics(
                    g.Key.PlayerId,
                    g.Key.TeamId,
                    g.Key.TeamPlayerId,
                    request.CompetitionId,
                    request.Scope,
                    g,
                    request.CompetitionDivisionId,
                    request.TournamentGroupId,
                    request.PlayoffSeriesId))
                .ToList();

            await _statisticsRepository.ReplaceCompetitionStatisticsAsync(
                request.CompetitionId,
                request.Scope,
                request.CompetitionDivisionId,
                request.TournamentGroupId,
                request.PlayoffSeriesId,
                teamAggregates,
                playerAggregates,
                goalieAggregates);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed RecalculateHockeyCompetitionStatistics for {CompetitionId}",
                request.CompetitionId);
            return Result.Failure("An error occurred while recalculating competition statistics.", ex.Flatten());
        }
    }
}

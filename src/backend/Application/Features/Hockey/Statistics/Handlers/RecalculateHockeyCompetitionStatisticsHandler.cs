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
/// Recalculates competition aggregate hockey statistics for one scope, or for several scopes
/// from a single match and roster load.
/// </summary>
public class RecalculateHockeyCompetitionStatisticsHandler
    : IRequestHandler<RecalculateHockeyCompetitionStatisticsCommand, Result>,
      IRequestHandler<RecalculateHockeyCompetitionScopesCommand, Result>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
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
        _logger = logger;
    }

    public Task<Result> Handle(
        RecalculateHockeyCompetitionStatisticsCommand request,
        CancellationToken cancellationToken) =>
        RecalculateAsync(
            request.CompetitionId,
            new[]
            {
                new HockeyStatisticsScopeTarget(
                    request.Scope,
                    request.CompetitionDivisionId,
                    request.TournamentGroupId,
                    request.PlayoffSeriesId)
            },
            cancellationToken);

    public Task<Result> Handle(
        RecalculateHockeyCompetitionScopesCommand request,
        CancellationToken cancellationToken) =>
        RecalculateAsync(request.CompetitionId, request.Scopes, cancellationToken);

    private async Task<Result> RecalculateAsync(
        Guid competitionId,
        IReadOnlyList<HockeyStatisticsScopeTarget> scopes,
        CancellationToken cancellationToken)
    {
        try
        {
            foreach (HockeyStatisticsScopeTarget target in scopes)
            {
                HockeyStatisticsHandlerSupport.ValidateScopeIds(
                    target.Scope,
                    target.CompetitionDivisionId,
                    target.TournamentGroupId,
                    target.PlayoffSeriesId);
            }

            HockeyCompetition? competition = await _competitionRepository.GetByIdAsync(competitionId);
            if (competition is null)
                return Result.NotFound("HockeyCompetition", competitionId);

            IReadOnlyList<HockeyMatch> loadedMatches = await LoadMatchesAsync(competitionId, scopes, cancellationToken);
            await HockeyStatisticsHandlerSupport.AttachTeamPlayersAsync(loadedMatches, _teamRepository, cancellationToken);

            HockeyStandingRules standingRules = competition.GetEffectiveRules().StandingRules;
            foreach (HockeyStatisticsScopeTarget target in scopes)
            {
                List<HockeyMatch> scopedMatches = loadedMatches
                    .Where(m => HockeyStatisticsHandlerSupport.MatchesScope(
                        m,
                        target.Scope,
                        target.CompetitionDivisionId,
                        target.TournamentGroupId,
                        target.PlayoffSeriesId))
                    .ToList();

                await ReplaceScopeAsync(competitionId, target, scopedMatches, standingRules);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed RecalculateHockeyCompetitionStatistics for {CompetitionId}",
                competitionId);
            return Result.Failure("An error occurred while recalculating competition statistics.", ex.Flatten());
        }
    }

    /// <summary>
    /// One scope filters in SQL. Several scopes share one competition-wide load, since every
    /// narrower scope is a subset of the competition.
    /// </summary>
    private Task<IReadOnlyList<HockeyMatch>> LoadMatchesAsync(
        Guid competitionId,
        IReadOnlyList<HockeyStatisticsScopeTarget> scopes,
        CancellationToken cancellationToken)
    {
        if (scopes.Count == 1)
        {
            HockeyStatisticsScopeTarget only = scopes[0];
            return _matchRepository.GetForStatisticsAsync(
                competitionId,
                only.Scope,
                only.CompetitionDivisionId,
                only.TournamentGroupId,
                only.PlayoffSeriesId,
                cancellationToken);
        }

        return _matchRepository.GetForStatisticsAsync(
            competitionId,
            HockeyStatisticsScope.Competition,
            cancellationToken: cancellationToken);
    }

    private async Task ReplaceScopeAsync(
        Guid competitionId,
        HockeyStatisticsScopeTarget target,
        List<HockeyMatch> scopedMatches,
        HockeyStandingRules standingRules)
    {
        List<HockeyMatchTeamStatistics> matchTeamStats = new();
        List<HockeyMatchPlayerStatistics> matchPlayerStats = new();
        List<HockeyGoalieMatchStatistics> matchGoalieStats = new();

        foreach (HockeyMatch match in scopedMatches)
        {
            foreach (HockeyMatchTeam matchTeam in match.MatchTeams)
            {
                if (match.CountsTowardTeamStatistics)
                    matchTeamStats.Add(HockeyStatisticsCalculationService.BuildMatchTeamStatistics(match, matchTeam));

                if (matchTeam.PlayerSelection is null)
                    continue;

                if (match.CountsTowardPlayerStatistics)
                    matchPlayerStats.AddRange(HockeyStatisticsCalculationService.BuildMatchPlayerStatistics(match, matchTeam));

                if (match.CountsTowardGoalieStatistics)
                    matchGoalieStats.AddRange(HockeyStatisticsCalculationService.BuildGoalieMatchStatistics(match, matchTeam));
            }
        }

        List<HockeyMatch> standingsMatches = scopedMatches
            .Where(m => m.CountsTowardStandings && m.StandingResultType is not null)
            .ToList();

        HashSet<Guid> teamIds = standingsMatches
            .SelectMany(m => m.MatchTeams)
            .Select(t => t.TeamId)
            .ToHashSet();

        List<HockeyTeamCompetitionStatistics> teamAggregates = teamIds
            .Select(teamId => HockeyStatisticsCalculationService.AggregateTeamCompetitionStatistics(
                teamId,
                competitionId,
                target.Scope,
                standingsMatches.Where(m => m.MatchTeams.Any(t => t.TeamId == teamId)),
                matchTeamStats,
                standingRules,
                target.CompetitionDivisionId,
                target.TournamentGroupId,
                target.PlayoffSeriesId))
            .ToList();

        HockeyStatisticsHandlerSupport.AssignStandingRanks(teamAggregates, standingRules.TieBreakers);

        List<HockeyPlayerCompetitionStatistics> playerAggregates = matchPlayerStats
            .GroupBy(s => new { s.PlayerId, s.TeamId, s.TeamPlayerId })
            .Select(g => HockeyStatisticsCalculationService.AggregatePlayerCompetitionStatistics(
                g.Key.PlayerId,
                g.Key.TeamId,
                g.Key.TeamPlayerId,
                competitionId,
                target.Scope,
                g,
                target.CompetitionDivisionId,
                target.TournamentGroupId,
                target.PlayoffSeriesId))
            .ToList();

        List<HockeyGoalieCompetitionStatistics> goalieAggregates = matchGoalieStats
            .GroupBy(s => new { s.PlayerId, s.TeamId, s.TeamPlayerId })
            .Select(g => HockeyStatisticsCalculationService.AggregateGoalieCompetitionStatistics(
                g.Key.PlayerId,
                g.Key.TeamId,
                g.Key.TeamPlayerId,
                competitionId,
                target.Scope,
                g,
                target.CompetitionDivisionId,
                target.TournamentGroupId,
                target.PlayoffSeriesId))
            .ToList();

        await _statisticsRepository.ReplaceCompetitionStatisticsAsync(
            competitionId,
            target.Scope,
            target.CompetitionDivisionId,
            target.TournamentGroupId,
            target.PlayoffSeriesId,
            teamAggregates,
            playerAggregates,
            goalieAggregates);
    }
}

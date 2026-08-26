using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Statistics.Commands;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Statistics;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles marking a hockey match as finished and advancing playoff winners like floorball.
/// Recalculates match and competition statistics after the finish is saved.
/// </summary>
public class MarkHockeyMatchFinishedHandler : IRequestHandler<MarkHockeyMatchFinishedCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<MarkHockeyMatchFinishedHandler> _logger;

    public MarkHockeyMatchFinishedHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<MarkHockeyMatchFinishedHandler> logger)
    {
        _matchRepository = matchRepository;
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        MarkHockeyMatchFinishedCommand request,
        CancellationToken cancellationToken)
    {
        Result<HockeyMatchDto> result = await HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(MarkHockeyMatchFinishedCommand),
            async (match, ct) =>
            {
                match.MarkFinished(DateTimeUtc.Normalize(request.ActualEndTime), request.ResultType);
                await AdvancePlayoffWinnerAsync(match, ct);
            },
            cancellationToken);

        if (result.IsSuccess && result.Data is not null)
            await RecalculateStatisticsAsync(result.Data, cancellationToken);

        return result;
    }

    private async Task RecalculateStatisticsAsync(HockeyMatchDto match, CancellationToken cancellationToken)
    {
        Result matchStats = await _mediator.Send(
            new RecalculateHockeyMatchStatisticsCommand(match.Id),
            cancellationToken);
        if (!matchStats.IsSuccess)
        {
            _logger.LogWarning(
                "Match statistics recalc failed after finish {MatchId}: {Error}",
                match.Id,
                matchStats.Error);
        }

        if (match.CompetitionId is not Guid competitionId)
            return;

        await RecalculateScopeAsync(
            competitionId,
            HockeyStatisticsScope.Competition,
            competitionDivisionId: null,
            tournamentGroupId: null,
            playoffSeriesId: null,
            cancellationToken);

        if (match.CompetitionDivisionId is Guid divisionId)
        {
            await RecalculateScopeAsync(
                competitionId,
                HockeyStatisticsScope.Division,
                divisionId,
                tournamentGroupId: null,
                playoffSeriesId: null,
                cancellationToken);
        }

        if (match.TournamentGroupId is Guid groupId)
        {
            await RecalculateScopeAsync(
                competitionId,
                HockeyStatisticsScope.TournamentGroup,
                competitionDivisionId: null,
                groupId,
                playoffSeriesId: null,
                cancellationToken);
        }

        if (match.PlayoffSeriesId is Guid seriesId)
        {
            await RecalculateScopeAsync(
                competitionId,
                HockeyStatisticsScope.PlayoffSeries,
                competitionDivisionId: null,
                tournamentGroupId: null,
                seriesId,
                cancellationToken);
        }
    }

    private async Task RecalculateScopeAsync(
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId,
        Guid? tournamentGroupId,
        Guid? playoffSeriesId,
        CancellationToken cancellationToken)
    {
        Result result = await _mediator.Send(
            new RecalculateHockeyCompetitionStatisticsCommand(
                competitionId,
                scope,
                competitionDivisionId,
                tournamentGroupId,
                playoffSeriesId),
            cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                "Competition statistics recalc failed after finish for {CompetitionId} scope {Scope}: {Error}",
                competitionId,
                scope,
                result.Error);
        }
    }

    private async Task AdvancePlayoffWinnerAsync(HockeyMatch completed, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (completed.PlayoffRound is null && completed.NextMatchId is null && completed.PlayoffSeriesId is null)
            return;

        Guid? winnerTeamId = completed.HomeScore > completed.AwayScore
            ? completed.HomeTeamId
            : completed.AwayScore > completed.HomeScore
                ? completed.AwayTeamId
                : null;
        if (winnerTeamId is not Guid winnerId)
        {
            _logger.LogWarning("Playoff match {MatchId} ended without a winner.", completed.Id);
            return;
        }

        Guid? winnerCompetitionTeamId = completed.HomeScore > completed.AwayScore
            ? completed.HomeMatchTeam?.CompetitionTeamId
            : completed.AwayMatchTeam?.CompetitionTeamId;

        if (completed.NextMatchId is Guid nextMatchId && completed.NextMatchSlot is HockeyTeamSlot nextSlot)
        {
            HockeyMatch? nextMatch = await _matchRepository.GetByIdAsync(nextMatchId);
            if (nextMatch is not null
                && nextMatch.Status is HockeyMatchStatus.Scheduled or HockeyMatchStatus.Warmup)
            {
                HockeyCompetitionTeam? competitionTeam = null;
                if (nextMatch.CompetitionId is Guid competitionId)
                {
                    HockeyCompetition? competition = await _competitionRepository.GetByIdAsync(competitionId);
                    competitionTeam = competition?.Teams.FirstOrDefault(team => team.TeamId == winnerId);
                    if (competitionTeam is null)
                    {
                        _logger.LogWarning(
                            "Could not assign playoff winner {TeamId} to next match {NextMatchId}: competition team missing.",
                            winnerId,
                            nextMatchId);
                    }
                    else
                    {
                        nextMatch.AssignPlayoffTeam(nextSlot, winnerId, competitionTeam);
                    }
                }
                else
                {
                    nextMatch.AssignPlayoffTeam(nextSlot, winnerId);
                }
            }
        }

        if (completed.PlayoffSeriesId is Guid seriesId
            && completed.CompetitionId is Guid seriesCompetitionId
            && winnerCompetitionTeamId is Guid winnerCompTeamId)
        {
            HockeyCompetition? competition = await _competitionRepository.GetByIdAsync(seriesCompetitionId);
            HockeyPlayoffSeries? series = competition?.PlayoffSeries.FirstOrDefault(item => item.Id == seriesId);
            if (competition is null || series is null)
                return;

            IReadOnlyList<HockeyMatch> seriesMatches =
                await _matchRepository.GetByCompetitionIdAsync(seriesCompetitionId);
            int winsNeeded = (series.BestOf / 2) + 1;
            int winnerWins = seriesMatches
                .Where(item => item.Id != completed.Id)
                .Count(item =>
                    item.PlayoffSeriesId == seriesId
                    && item.Status == HockeyMatchStatus.Finished
                    && item.HomeScore != item.AwayScore
                    && (item.HomeScore > item.AwayScore
                        ? item.HomeMatchTeam?.CompetitionTeamId
                        : item.AwayMatchTeam?.CompetitionTeamId) == winnerCompTeamId);
            winnerWins++;

            if (winsNeeded > 0 && winnerWins >= winsNeeded)
                competition.CompletePlayoffSeries(seriesId, winnerCompTeamId);
        }
    }
}

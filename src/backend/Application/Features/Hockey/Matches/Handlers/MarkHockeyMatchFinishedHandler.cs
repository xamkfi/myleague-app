using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Enums.Hockey.Matches;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles marking a hockey match as finished and advancing playoff winners like floorball.
/// </summary>
public class MarkHockeyMatchFinishedHandler : IRequestHandler<MarkHockeyMatchFinishedCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<MarkHockeyMatchFinishedHandler> _logger;

    public MarkHockeyMatchFinishedHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<MarkHockeyMatchFinishedHandler> logger)
    {
        _matchRepository = matchRepository;
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        MarkHockeyMatchFinishedCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);

            match.MarkFinished(request.ActualEndTime, request.ResultType);
            await AdvancePlayoffWinnerAsync(match, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("MarkHockeyMatchFinished succeeded for match {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected MarkHockeyMatchFinished for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid MarkHockeyMatchFinished for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed MarkHockeyMatchFinished for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(
                "An error occurred while performing MarkHockeyMatchFinishedCommand.",
                ex.Flatten());
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
        if (winnerTeamId is null)
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
                    competitionTeam = competition?.Teams.FirstOrDefault(team => team.TeamId == winnerTeamId.Value);
                    if (competitionTeam is null)
                    {
                        _logger.LogWarning(
                            "Could not assign playoff winner {TeamId} to next match {NextMatchId}: competition team missing.",
                            winnerTeamId.Value,
                            nextMatchId);
                    }
                    else
                    {
                        nextMatch.AssignPlayoffTeam(nextSlot, winnerTeamId.Value, competitionTeam);
                    }
                }
                else
                {
                    nextMatch.AssignPlayoffTeam(nextSlot, winnerTeamId.Value);
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

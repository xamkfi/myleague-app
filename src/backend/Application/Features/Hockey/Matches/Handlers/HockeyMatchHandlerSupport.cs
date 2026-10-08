using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Application.Features.Hockey.Statistics.Commands;
using Domain.Entities.Hockey.Matches;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Statistics;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Shared load → mutate → save path for hockey match commands.
/// </summary>
internal static class HockeyMatchHandlerSupport
{
    public static Task<Result<HockeyMatchDto>> MutateAsync(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger logger,
        Guid matchId,
        string operationName,
        Action<HockeyMatch> mutate,
        CancellationToken cancellationToken) =>
        MutateAsync(
            matchRepository,
            unitOfWork,
            logger,
            matchId,
            operationName,
            (match, _) =>
            {
                mutate(match);
                return Task.CompletedTask;
            },
            cancellationToken);

    public static async Task<Result<HockeyMatchDto>> MutateAsync(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger logger,
        Guid matchId,
        string operationName,
        Func<HockeyMatch, CancellationToken, Task> mutate,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await matchRepository.GetByIdAsync(matchId);
            if (match is null)
            {
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", matchId);
            }

            await mutate(match, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("{Operation} succeeded for match {MatchId}", operationName, matchId);
            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Domain rejected {Operation} for {MatchId}", operationName, matchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid {Operation} for {MatchId}", operationName, matchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed {Operation} for {MatchId}", operationName, matchId);
            return Result<HockeyMatchDto>.Failure($"An error occurred while performing {operationName}.", ex.Flatten());
        }
    }

    public static HockeyMatchTeam GetRequiredMatchTeam(HockeyMatch match, Guid matchTeamId) =>
        match.MatchTeams.FirstOrDefault(t => t.Id == matchTeamId)
        ?? throw new InvalidOperationException("Match team is not part of this match.");

    public static HockeyMatchLine GetRequiredMatchLine(HockeyMatchTeam matchTeam, Guid matchLineId) =>
        matchTeam.Lines.FirstOrDefault(l => l.Id == matchLineId)
        ?? throw new InvalidOperationException("Match line is not part of this match team.");

    public static HockeyMatchActivePlayer GetRequiredActivePlayer(HockeyMatchTeam matchTeam, Guid matchActivePlayerId)
    {
        HockeyMatchActivePlayer? player = matchTeam.PlayerSelection?.FindActivePlayer(matchActivePlayerId);
        if (player is null)
        {
            throw new InvalidOperationException("Match active player is not part of this match team's roster.");
        }

        return player;
    }

    /// <summary>
    /// Recalculates statistics after a change to a finished match, such as a lineup correction.
    /// Matches that are not finished are skipped, because their statistics are built when they finish.
    /// </summary>
    public static async Task RecalculateStatisticsIfFinishedAsync(
        IMediator mediator,
        ILogger logger,
        Result<HockeyMatchDto> result,
        CancellationToken cancellationToken)
    {
        if (result.IsSuccess
            && result.Data is not null
            && result.Data.Status == HockeyMatchStatus.Finished.ToString())
        {
            await RecalculateStatisticsAsync(mediator, logger, result.Data, cancellationToken);
        }
    }

    /// <summary>
    /// Recalculates the match statistics and every competition scope the match counts towards.
    /// Failures are logged and do not fail the command that triggered the recalculation.
    /// </summary>
    public static async Task RecalculateStatisticsAsync(
        IMediator mediator,
        ILogger logger,
        HockeyMatchDto match,
        CancellationToken cancellationToken)
    {
        Result matchStats = await mediator.Send(
            new RecalculateHockeyMatchStatisticsCommand(match.Id),
            cancellationToken);
        if (!matchStats.IsSuccess)
        {
            logger.LogWarning(
                "Match statistics recalc failed for {MatchId}: {Error}",
                match.Id,
                matchStats.Error);
        }

        if (match.CompetitionId is not Guid competitionId)
            return;

        List<HockeyStatisticsScopeTarget> scopes = new()
        {
            new HockeyStatisticsScopeTarget(HockeyStatisticsScope.Competition)
        };
        if (match.CompetitionDivisionId is Guid divisionId)
            scopes.Add(new HockeyStatisticsScopeTarget(HockeyStatisticsScope.Division, CompetitionDivisionId: divisionId));
        if (match.TournamentGroupId is Guid groupId)
            scopes.Add(new HockeyStatisticsScopeTarget(HockeyStatisticsScope.TournamentGroup, TournamentGroupId: groupId));
        if (match.PlayoffSeriesId is Guid seriesId)
            scopes.Add(new HockeyStatisticsScopeTarget(HockeyStatisticsScope.PlayoffSeries, PlayoffSeriesId: seriesId));

        Result result = await mediator.Send(
            new RecalculateHockeyCompetitionScopesCommand(competitionId, scopes),
            cancellationToken);
        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Competition statistics recalc failed for {CompetitionId} scopes {Scopes}: {Error}",
                competitionId,
                string.Join(", ", scopes.Select(s => s.Scope)),
                result.Error);
        }
    }
}

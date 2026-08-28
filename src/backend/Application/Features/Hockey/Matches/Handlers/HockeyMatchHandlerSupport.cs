using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Hockey;
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
}

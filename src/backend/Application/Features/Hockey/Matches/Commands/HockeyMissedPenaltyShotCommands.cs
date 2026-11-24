using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Records a missed penalty shot on a hockey match.
/// </summary>
public record RecordHockeyMissedPenaltyShotCommand(
    Guid MatchId,
    Guid MatchTeamId,
    Guid ShooterActivePlayerId,
    int PeriodNumber,
    int TimeInSeconds) : IRequest<Result<HockeyMatchDto>>;

/// <summary>
/// Deletes a missed penalty shot from a hockey match (live-ops undo).
/// </summary>
public record DeleteHockeyMissedPenaltyShotCommand(Guid MatchId, Guid MissedPenaltyShotEventId)
    : IRequest<Result<HockeyMatchDto>>;

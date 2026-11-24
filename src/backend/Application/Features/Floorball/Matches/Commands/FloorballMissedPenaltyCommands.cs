using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using MediatR;

namespace Application.Features.Floorball.Matches.Commands;

/// <summary>
/// Records a missed penalty shot in a floorball match.
/// </summary>
public record RecordFloorballMissedPenaltyCommand(
    Guid MatchId,
    Guid TeamId,
    Guid ShooterPlayerId,
    int PeriodNumber,
    int TimeInSeconds) : IRequest<Result<FloorballMatchDto>>;

/// <summary>
/// Deletes a missed penalty shot from a floorball match.
/// </summary>
public record DeleteFloorballMissedPenaltyCommand(
    Guid MatchId,
    Guid MissedPenaltyEventId) : IRequest<Result<FloorballMatchDto>>;

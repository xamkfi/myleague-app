using Application.Common;
using Application.Features.Football.Matches.DTOs;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

/// <summary>
/// Records a missed penalty kick in a football match.
/// </summary>
public record RecordFootballMissedPenaltyCommand(
    Guid MatchId,
    Guid TeamId,
    Guid ShooterPlayerId,
    int PeriodNumber,
    int TimeInSeconds) : IRequest<Result<FootballMatchDto>>;

/// <summary>
/// Deletes a missed penalty kick from a football match.
/// </summary>
public record DeleteFootballMissedPenaltyCommand(
    Guid MatchId,
    Guid MissedPenaltyEventId) : IRequest<Result<FootballMatchDto>>;

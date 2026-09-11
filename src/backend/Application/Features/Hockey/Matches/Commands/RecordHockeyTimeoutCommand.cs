using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Records a timeout event on a hockey match.
/// </summary>
public record RecordHockeyTimeoutCommand(
    Guid MatchId,
    Guid MatchTeamId,
    int PeriodNumber,
    int TimeInSeconds,
    string? Description = null) : IRequest<Result<HockeyMatchDto>>;

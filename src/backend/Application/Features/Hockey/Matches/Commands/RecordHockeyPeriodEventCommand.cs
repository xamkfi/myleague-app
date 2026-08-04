using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Records a period start/end event on a hockey match.
/// </summary>
public record RecordHockeyPeriodEventCommand(
    Guid MatchId,
    int PeriodNumber,
    int TimeInSeconds,
    HockeyPeriodAction Action,
    string? Description = null) : IRequest<Result<HockeyMatchDto>>;

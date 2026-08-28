using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Sets the current period number of a hockey match.
/// </summary>
public record SetHockeyMatchCurrentPeriodCommand(
    Guid MatchId,
    int PeriodNumber) : IRequest<Result<HockeyMatchDto>>;

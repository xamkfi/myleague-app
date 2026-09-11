using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Admin correction of goals for one match side.
/// </summary>
public record SetHockeyMatchTeamGoalsCommand(
    Guid MatchId,
    HockeyTeamSlot TeamSlot,
    int Goals) : IRequest<Result<HockeyMatchDto>>;

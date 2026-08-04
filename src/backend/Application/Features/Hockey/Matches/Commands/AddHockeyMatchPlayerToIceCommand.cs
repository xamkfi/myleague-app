using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Puts a dressed player on the ice.
/// </summary>
public record AddHockeyMatchPlayerToIceCommand(
    Guid MatchId,
    Guid MatchTeamId,
    Guid MatchActivePlayerId,
    HockeyIceSlot? Slot = null,
    int? Order = null,
    bool? IsGoalie = null,
    bool IsExtraAttacker = false,
    int? PeriodNumber = null,
    int? TimeInSeconds = null,
    Guid? UserId = null) : IRequest<Result<HockeyMatchDto>>;

using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Records a faceoff event on a hockey match.
/// </summary>
public record RecordHockeyFaceoffCommand(
    Guid MatchId,
    Guid WinningMatchTeamId,
    Guid LosingMatchTeamId,
    int PeriodNumber,
    int TimeInSeconds,
    HockeyFaceoffZone Zone,
    HockeyFaceoffSpot Spot,
    Guid? WinningActivePlayerId = null,
    Guid? LosingActivePlayerId = null,
    string? Description = null) : IRequest<Result<HockeyMatchDto>>;

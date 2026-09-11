using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Records a stoppage event on a hockey match.
/// </summary>
public record RecordHockeyStoppageCommand(
    Guid MatchId,
    int PeriodNumber,
    int TimeInSeconds,
    HockeyStoppageReason Reason,
    Guid? ResponsibleMatchTeamId = null,
    Guid? ResponsibleActivePlayerId = null,
    HockeyFaceoffZone? NextFaceoffZone = null,
    HockeyFaceoffSpot? NextFaceoffSpot = null,
    string? RuleReference = null,
    string? Description = null) : IRequest<Result<HockeyMatchDto>>;

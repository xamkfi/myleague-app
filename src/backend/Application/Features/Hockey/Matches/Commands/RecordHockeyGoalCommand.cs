using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Records a goal event on a hockey match.
/// </summary>
public record RecordHockeyGoalCommand(
    Guid MatchId,
    Guid ScoringMatchTeamId,
    Guid ScorerActivePlayerId,
    int PeriodNumber,
    int TimeInSeconds,
    HockeyGoalStrength GoalStrength,
    Guid? PrimaryAssistActivePlayerId = null,
    Guid? SecondaryAssistActivePlayerId = null,
    Guid? GoalieActivePlayerId = null,
    bool WasEmptyNet = false,
    string? Description = null) : IRequest<Result<HockeyMatchDto>>;

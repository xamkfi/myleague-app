using Application.Common;
using Application.Features.Football.Matches.DTOs;
using Domain.Enums.Football;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

public record RecordGoalCommand(
    Guid MatchId,
    Guid ScoringTeamId,
    Guid ScoringPlayerId,
    Guid? AssistingPlayerId,
    int PeriodNumber,
    int TimeInSeconds,
    string? Description,
    FootballGoalType? GoalType = null) : IRequest<Result<FootballMatchDto>>;

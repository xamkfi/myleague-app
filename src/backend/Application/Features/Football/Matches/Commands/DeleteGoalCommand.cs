using Application.Common;
using Application.Features.Football.Matches.DTOs;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

public record DeleteGoalCommand(Guid MatchId, Guid GoalEventId) : IRequest<Result<FootballMatchDto>>;

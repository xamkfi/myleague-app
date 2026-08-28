using Application.Common;
using Application.Features.Football.Matches.DTOs;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

public record AssignMatchTeamsCommand(
    Guid MatchId,
    Guid? HomeTeamId,
    Guid? AwayTeamId) : IRequest<Result<FootballMatchDto>>;

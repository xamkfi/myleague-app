using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Assigns home and away teams to a hockey match.
/// </summary>
public record AddHomeAwayTeamsToHockeyMatchCommand(
    Guid MatchId,
    Guid HomeTeamId,
    Guid AwayTeamId) : IRequest<Result<HockeyMatchDto>>;

using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Sets the result type of a hockey match.
/// </summary>
public record SetHockeyMatchResultTypeCommand(
    Guid MatchId,
    HockeyMatchResultType? ResultType) : IRequest<Result<HockeyMatchDto>>;

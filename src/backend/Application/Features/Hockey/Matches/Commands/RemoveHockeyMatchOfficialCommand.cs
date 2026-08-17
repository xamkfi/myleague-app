using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Removes an official assignment from a hockey match.
/// </summary>
public record RemoveHockeyMatchOfficialCommand(
    Guid MatchId,
    Guid OfficialId) : IRequest<Result<HockeyMatchDto>>;

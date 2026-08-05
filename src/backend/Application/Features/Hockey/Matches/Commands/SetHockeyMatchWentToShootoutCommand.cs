using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;
namespace Application.Features.Hockey.Matches.Commands;
/// <summary>
/// Sets whether a hockey match went to shootout.
/// </summary>
public record SetHockeyMatchWentToShootoutCommand(
    Guid MatchId,
    bool WentToShootout) : IRequest<Result<HockeyMatchDto>>;



using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using MediatR;

namespace Application.Features.Hockey.Teams.Commands;

/// <summary>
/// Command to deactivate a line on a hockey team.
/// </summary>
public record RemoveHockeyLineCommand(Guid TeamId, Guid LineId) : IRequest<Result<HockeyTeamDto>>;

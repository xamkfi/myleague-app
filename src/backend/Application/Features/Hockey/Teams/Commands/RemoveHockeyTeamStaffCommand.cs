using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using MediatR;

namespace Application.Features.Hockey.Teams.Commands;

/// <summary>
/// Command to remove staff from a hockey team.
/// </summary>
public record RemoveHockeyTeamStaffCommand(Guid TeamId, Guid StaffId) : IRequest<Result<HockeyTeamDto>>;

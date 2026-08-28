using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using MediatR;

namespace Application.Features.Hockey.Teams.Commands;

/// <summary>
/// Command: SetHockeyTeamActiveStatus.
/// </summary>
public record SetHockeyTeamActiveStatusCommand(Guid TeamId, bool IsActive) : IRequest<Result<HockeyTeamDto>>;

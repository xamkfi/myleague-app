using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using MediatR;

namespace Application.Features.Hockey.Teams.Commands;

/// <summary>
/// Command: UpdateHockeyTeamLogo.
/// </summary>
public record UpdateHockeyTeamLogoCommand(Guid TeamId, string? LogoUrl) : IRequest<Result<HockeyTeamDto>>;

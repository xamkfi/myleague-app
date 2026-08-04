using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Hockey.Teams;
using MediatR;

namespace Application.Features.Hockey.Teams.Commands;

/// <summary>
/// Command to add a hockey player to a team roster.
/// </summary>
public record AddPlayerToHockeyTeamCommand(
    Guid TeamId,
    Guid PlayerId,
    HockeyPosition Position,
    Guid? CompetitionId = null,
    int? JerseyNumber = null,
    int? RequestedJerseyNumber = null,
    HockeyRosterStatus RosterStatus = HockeyRosterStatus.Active) : IRequest<Result<HockeyTeamDto>>;

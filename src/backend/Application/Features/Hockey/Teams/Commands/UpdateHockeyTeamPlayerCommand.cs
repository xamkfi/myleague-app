using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Hockey.Teams;
using MediatR;

namespace Application.Features.Hockey.Teams.Commands;

/// <summary>
/// Command to update a hockey team roster membership.
/// </summary>
public record UpdateHockeyTeamPlayerCommand(
    Guid TeamId,
    Guid PlayerId,
    HockeyPosition Position,
    int? JerseyNumber,
    HockeyRosterStatus RosterStatus,
    HockeyCaptainRole CaptainRole,
    Guid? CompetitionId = null) : IRequest<Result<HockeyTeamDto>>;

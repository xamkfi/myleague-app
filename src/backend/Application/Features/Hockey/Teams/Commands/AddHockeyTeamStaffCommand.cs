using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Hockey.Teams;
using MediatR;

namespace Application.Features.Hockey.Teams.Commands;

/// <summary>
/// Command to add staff to a hockey team.
/// </summary>
public record AddHockeyTeamStaffCommand(
    Guid TeamId,
    Guid PersonId,
    HockeyTeamStaffRole Role,
    Guid? CompetitionId = null) : IRequest<Result<HockeyTeamDto>>;

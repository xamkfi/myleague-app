using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Enums.Hockey.Teams;
using MediatR;

namespace Application.Features.Hockey.Teams.Commands;

/// <summary>
/// Command to add a line to a hockey team.
/// </summary>
public record AddHockeyLineCommand(
    Guid TeamId,
    string Name,
    int LineNumber,
    HockeyLineType LineType,
    Guid? CompetitionId = null) : IRequest<Result<HockeyTeamDto>>;

using System;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.TeamManager
{
    /// <summary>
    /// Command for updating the team assignment of a floorball team manager
    /// </summary>
    /// <param name="Id">The ID of the team manager to update</param>
    /// <param name="TeamId">The new team ID this manager will be responsible for</param>
    public record UpdateFloorballTeamManagerTeamCommand(
        Guid Id,
        Guid TeamId) : IRequest<Result<FloorballTeamManagerDto>>;
} 
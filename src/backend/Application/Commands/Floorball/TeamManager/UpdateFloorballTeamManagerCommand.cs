using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.TeamManager
{
    /// <summary>
    /// Command for updating a floorball team manager
    /// </summary>
    /// <param name="Id">The ID of the team manager to update</param>
    /// <param name="IsActive">Whether the team manager is active</param>
    public record UpdateFloorballTeamManagerCommand(
        Guid Id,
        bool IsActive) : IRequest<Result<FloorballTeamManagerDto>>;
} 
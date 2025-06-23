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
    /// Command for creating a new floorball team manager
    /// </summary>
    /// <param name="PersonId">The ID of the person who will be the team manager</param>
    /// <param name="TeamId">The ID of the team this manager will be responsible for</param>
    public record CreateFloorballTeamManagerCommand(
        Guid PersonId,
        Guid TeamId) : IRequest<Result<FloorballTeamManagerDto>>;
} 
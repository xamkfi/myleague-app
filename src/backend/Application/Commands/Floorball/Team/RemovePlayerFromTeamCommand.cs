using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Team
{
    /// <summary>
    /// Command for removing a player from a floorball team
    /// </summary>
    /// <param name="TeamId"></param>
    /// <param name="PlayerId"></param>
    public record RemovePlayerFromTeamCommand(
        Guid TeamId,
        Guid PlayerId) : IRequest<Result<FloorballTeamDto>>;
}

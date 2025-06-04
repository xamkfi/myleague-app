using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using Domain.Enums.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Team
{
    /// <summary>
    /// Command for adding a new player to a floorball team
    /// </summary>
    /// <param name="TeamId"></param>
    /// <param name="PlayerId"></param>
    /// <param name="Position"></param>
    /// <param name="JerseyNumber"></param>
    public record AddPlayerToTeamCommand(
        Guid TeamId,
        Guid PlayerId,
        FloorballPosition Position,
        int? JerseyNumber) : IRequest<Result<FloorballTeamDto>>;
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using Domain.Enums.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Player
{
    /// <summary>
    /// Command for creating a new floorball player
    /// </summary>
    /// <param name="PersonId"></param>
    public record CreateFloorballPlayerCommand(
        Guid PersonId) : IRequest<Result<FloorballPlayerDto>>;
}

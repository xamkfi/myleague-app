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
    /// Command for updating a floorball player
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Position"></param>
    /// <param name="IsActive"></param>
    public record UpdateFloorballPlayerCommand(
        Guid Id,
        bool IsActive) : IRequest<Result<FloorballPlayerDto>>;
}

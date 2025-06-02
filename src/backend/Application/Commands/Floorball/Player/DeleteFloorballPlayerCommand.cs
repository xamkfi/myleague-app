using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Player
{
    /// <summary>
    /// Command for deleting a floorball player
    /// </summary>
    /// <param name="Id"></param>
    public record DeleteFloorballPlayerCommand(Guid Id) : IRequest<Result<FloorballPlayerDto>>; 
}

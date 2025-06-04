using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Referee
{
    /// <summary>
    /// Command for deleting a floorball referee
    /// </summary>
    /// <param name="Id"></param>
    public record DeleteFloorballRefereeCommand(
        Guid Id) : IRequest<Result<FloorballRefereeDto>>;
}

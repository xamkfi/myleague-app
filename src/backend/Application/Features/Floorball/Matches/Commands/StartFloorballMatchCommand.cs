using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command for starting a floorball match
    /// </summary>
    /// <param name="Id"></param>
    public record StartFloorballMatchCommand(
        Guid Id) : IRequest<Result<FloorballMatchDto>>;
}

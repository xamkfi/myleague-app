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
    /// Command for completing a floorball match
    /// </summary>
    /// <param name="Id"></param>
    public record CompleteFloorballMatchCommand(
        Guid Id) : IRequest<Result<FloorballMatchDto>>;
}

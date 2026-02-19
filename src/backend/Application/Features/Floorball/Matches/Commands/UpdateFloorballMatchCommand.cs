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
    /// Command for updating a floorball match
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="ScheduledDateTime"></param>
    /// <param name="Venue"></param>
    public record UpdateFloorballMatchCommand(
        Guid Id,
        DateTime ScheduledDateTime,
        string? Venue) : IRequest<Result<FloorballMatchDto>>;
}

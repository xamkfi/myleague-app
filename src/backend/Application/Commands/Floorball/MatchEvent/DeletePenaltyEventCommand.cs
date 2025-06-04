using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.MatchEvent
{
    /// <summary>
    /// Command for deleting a penalty event from a floorball match
    /// </summary>
    /// <param name="Id"></param>
    public record DeletePenaltyEventCommand(Guid Id) : IRequest<Result<FloorballPenaltyEventDto>>;
} 
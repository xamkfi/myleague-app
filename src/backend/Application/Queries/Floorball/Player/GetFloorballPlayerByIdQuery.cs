using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Player
{
    /// <summary>
    /// Query for retrieving a floorball player by Id
    /// </summary>
    /// <param name="Id"></param>
    public record GetFloorballPlayerByIdQuery(Guid Id) : IRequest<Result<FloorballPlayerDto>>;
}

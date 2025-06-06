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
    /// Query for retrieving all active floorball players
    /// </summary>
    public record GetActiveFloorballPlayersQuery() : IRequest<Result<IEnumerable<FloorballPlayerDto>>>;
}

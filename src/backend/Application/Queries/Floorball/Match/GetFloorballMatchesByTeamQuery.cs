using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Match
{
    /// <summary>
    /// Query for retrieving floorball matches by team
    /// </summary>
    /// <param name="TeamId"></param>
    public record GetFloorballMatchesByTeamQuery(Guid TeamId) : IRequest<Result<IEnumerable<FloorballMatchDto>>>;
}

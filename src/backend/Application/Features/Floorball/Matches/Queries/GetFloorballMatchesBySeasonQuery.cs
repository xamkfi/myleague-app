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
    /// Query for retrieving all floorball matches in a season
    /// </summary>
    /// <param name="SeasonId"></param>
    public record GetFloorballMatchesBySeasonQuery(Guid SeasonId) : IRequest<Result<IEnumerable<FloorballMatchDto>>>;
}

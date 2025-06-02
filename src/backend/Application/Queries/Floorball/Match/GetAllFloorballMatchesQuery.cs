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
    /// Query for retrieving all floorball matches
    /// </summary>
    public record GetAllFloorballMatchesQuery() : IRequest<Result<IEnumerable<FloorballMatchDto>>>;
}

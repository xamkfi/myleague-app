using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Season
{
    /// <summary>
    /// Query for retrieving all floorball seasons
    /// </summary>
    public record GetAllFloorballSeasonsQuery() : IRequest<Result<IEnumerable<FloorballSeasonDto>>>;
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using MediatR;

namespace Application.Features.Floorball.Seasons.Queries
{
    /// <summary>
    /// Query for retrieving all floorball seasons
    /// </summary>
    public record GetAllFloorballSeasonsQuery() : IRequest<Result<IEnumerable<FloorballSeasonDto>>>;
}

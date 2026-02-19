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
    /// Query for retrieving a floorball season by id
    /// </summary>
    /// <param name="Id"></param>
    public record GetFloorballSeasonByIdQuery(Guid Id) : IRequest<Result<FloorballSeasonDto>>;
}

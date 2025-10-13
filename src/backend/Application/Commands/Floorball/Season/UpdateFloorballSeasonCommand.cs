using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Season
{
    /// <summary>
    /// Command for updating a floorball season
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Name"></param>
    /// <param name="StartDate"></param>
    /// <param name="EndDate"></param>
    public record UpdateFloorballSeasonCommand(
        Guid Id,
        string Name,
        DateTime StartDate,
        DateTime EndDate) : IRequest<Result<FloorballSeasonDto>>;
}

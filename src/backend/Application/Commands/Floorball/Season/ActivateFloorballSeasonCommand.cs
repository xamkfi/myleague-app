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
    /// Command for activating a floorball season
    /// </summary>
    /// <param name="Id"></param>
    public record ActivateFloorballSeasonCommand(
        Guid Id) : IRequest<Result<FloorballSeasonDto>>;
}

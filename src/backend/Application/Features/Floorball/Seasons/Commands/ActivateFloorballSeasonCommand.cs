using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using MediatR;

namespace Application.Features.Floorball.Seasons.Commands
{
    /// <summary>
    /// Command for activating a floorball season
    /// </summary>
    /// <param name="Id"></param>
    public record ActivateFloorballSeasonCommand(
        Guid Id) : IRequest<Result<FloorballSeasonDto>>;
}

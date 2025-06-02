using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command for creating a floorball match
    /// </summary>
    /// <param name="SeasonId"></param>
    /// <param name="HomeTeamId"></param>
    /// <param name="AwayTeamId"></param>
    /// <param name="ScheduledDateTime"></param>
    /// <param name="Venue"></param>
    public record CreateFloorballMatchCommand(
        Guid SeasonId,
        Guid HomeTeamId,
        Guid AwayTeamId,
        DateTime ScheduledDateTime,
        string? Venue) : IRequest<Result<FloorballMatchDto>>;
}

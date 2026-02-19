using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Domain.Enums.Floorball;
using MediatR;

namespace Application.Features.Floorball.Seasons.Commands
{
    /// <summary>
    /// Command for creating a floorball season
    /// </summary>
    /// <param name="Name">The name of the season</param>
    /// <param name="DivisionIds">The list of division IDs to associate with this season</param>
    /// <param name="StartDate">The start date of the season</param>
    /// <param name="EndDate">The end date of the season</param>
    /// <param name="NumberOfPeriods">Number of regular periods (default: 2)</param>
    /// <param name="PeriodDurationMinutes">Duration in minutes per regular period (default: 15)</param>
    /// <param name="AllowOvertime">Whether overtime is allowed (default: true)</param>
    /// <param name="OvertimeDurationMinutes">Duration in minutes for overtime (default: 5)</param>
    /// <param name="AllowShootout">Whether shootout is allowed (default: true)</param>
    public record CreateFloorballSeasonCommand(
        string Name,
        IEnumerable<Guid> DivisionIds,
        DateTime StartDate,
        DateTime EndDate,
        int NumberOfPeriods = 2,
        int PeriodDurationMinutes = 15,
        bool AllowOvertime = true,
        int OvertimeDurationMinutes = 5,
        bool AllowShootout = true) : IRequest<Result<FloorballSeasonDto>>;
}

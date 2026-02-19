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
    /// <param name="NumberOfPeriods">Number of regular periods (default: 2)</param>
    /// <param name="PeriodDurationMinutes">Duration in minutes per regular period (default: 15)</param>
    /// <param name="AllowOvertime">Whether overtime is allowed (default: true)</param>
    /// <param name="OvertimeDurationMinutes">Duration in minutes for overtime (default: 5)</param>
    /// <param name="AllowShootout">Whether shootout is allowed (default: true)</param>
    public record UpdateFloorballSeasonCommand(
        Guid Id,
        string Name,
        DateTime StartDate,
        DateTime EndDate,
        int NumberOfPeriods = 2,
        int PeriodDurationMinutes = 15,
        bool AllowOvertime = true,
        int OvertimeDurationMinutes = 5,
        bool AllowShootout = true) : IRequest<Result<FloorballSeasonDto>>;
}

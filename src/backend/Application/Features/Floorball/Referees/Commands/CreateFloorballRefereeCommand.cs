using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using MediatR;

namespace Application.Features.Floorball.Referees.Commands
{
    /// <summary>
    /// Command for creating a floorball referee
    /// </summary>
    /// <param name="PersonId"></param>
    /// <param name="LicenseIssueDate"></param>
    /// <param name="LicenseExpiryDate"></param>
    public record CreateFloorballRefereeCommand(
        Guid PersonId,
        DateTime LicenseIssueDate,
        DateTime LicenseExpiryDate) : IRequest<Result<FloorballRefereeDto>>;
}

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
    /// Command for updating a floorball referee
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="LicenseIssueDate"></param>
    /// <param name="LicenseExpiryDate"></param>
    /// <param name="MatchesOfficiated"></param>
    /// <param name="IsActive"></param>
    public record UpdateFloorballRefereeCommand(
        Guid Id,
        DateTime? LicenseIssueDate,
        DateTime? LicenseExpiryDate,
        int MatchesOfficiated,
        bool IsActive) : IRequest<Result<FloorballRefereeDto>>;
}

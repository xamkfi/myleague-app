using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using MediatR;

namespace Application.Features.Football.Referees.Commands
{
    /// <summary>
    /// Command for creating a football referee
    /// </summary>
    /// <param name="PersonId"></param>
    /// <param name="LicenseIssueDate"></param>
    /// <param name="LicenseExpiryDate"></param>
    public record CreateFootballRefereeCommand(
        Guid PersonId,
        DateTime LicenseIssueDate,
        DateTime LicenseExpiryDate) : IRequest<Result<FootballRefereeDto>>;
}

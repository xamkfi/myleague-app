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
    /// Command for updating a football referee
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="LicenseIssueDate"></param>
    /// <param name="LicenseExpiryDate"></param>
    /// <param name="MatchesOfficiated"></param>
    /// <param name="IsActive"></param>
    public record UpdateFootballRefereeCommand(
        Guid Id,
        DateTime? LicenseIssueDate,
        DateTime? LicenseExpiryDate,
        int MatchesOfficiated,
        bool IsActive) : IRequest<Result<FootballRefereeDto>>;
}

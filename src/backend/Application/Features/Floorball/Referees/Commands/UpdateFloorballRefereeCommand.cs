using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Referee
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

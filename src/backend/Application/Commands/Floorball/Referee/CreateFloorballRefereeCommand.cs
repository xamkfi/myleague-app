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
    /// Command for creating a floorball referee
    /// </summary>
    /// <param name="PersonId"></param>
    /// <param name="LicenseIssuedDate"></param>
    /// <param name="LicenseExpiryDate"></param>
    /// <param name="MatchesOfficiated"></param>
    public record CreateFloorballRefereeCommand(
        Guid PersonId,
        DateTime? LicenseIssuedDate,
        DateTime? LicenseExpiryDate,
        int MatchesOfficiated) : IRequest<Result<FloorballRefereeDto>>;
}

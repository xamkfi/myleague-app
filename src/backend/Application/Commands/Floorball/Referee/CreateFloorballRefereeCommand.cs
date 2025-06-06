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
    /// <param name="LicenseIssueDate"></param>
    /// <param name="LicenseExpiryDate"></param>
    public record CreateFloorballRefereeCommand(
        Guid PersonId,
        DateTime LicenseIssueDate,
        DateTime LicenseExpiryDate) : IRequest<Result<FloorballRefereeDto>>;
}

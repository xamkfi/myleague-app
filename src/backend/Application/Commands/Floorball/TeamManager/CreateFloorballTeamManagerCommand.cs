using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.TeamManager
{
    /// <summary>
    /// Command for creating a new floorball team manager
    /// </summary>
    /// <param name="PersonId"></param>
    /// <param name="PrimaryResponsibility"></param>
    /// <param name="YearsOfExperience"></param>
    public record CreateFloorballTeamManagerCommand(
        Guid PersonId,
        string? PrimaryResponsibility,
        int YearsOfExperience) : IRequest<Result<FloorballTeamManagerDto>>;
} 
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
    /// Command for updating a floorball team manager
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="IsActive"></param>
    /// <param name="PrimaryResponsibility"></param>
    /// <param name="YearsOfExperience"></param>
    public record UpdateFloorballTeamManagerCommand(
        Guid Id,
        bool IsActive,
        string? PrimaryResponsibility,
        int YearsOfExperience) : IRequest<Result<FloorballTeamManagerDto>>;
} 
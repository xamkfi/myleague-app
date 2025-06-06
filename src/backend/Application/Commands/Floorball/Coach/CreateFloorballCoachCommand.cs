using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Coach
{
    /// <summary>
    /// Command for creating a new floorball coach
    /// </summary>
    /// <param name="PersonId"></param>
    /// <param name="YearsOfExperience"></param>
    /// <param name="CertificationLevel"></param>
    /// <param name="Specialization"></param>
    public record CreateFloorballCoachCommand(
        Guid PersonId,
        int YearsOfExperience,
        string? CertificationLevel,
        string? Specialization) : IRequest<Result<FloorballCoachDto>>;
} 
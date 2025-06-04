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
    /// Command for updating a floorball coach
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="IsActive"></param>
    /// <param name="YearsOfExperience"></param>
    /// <param name="CertificationLevel"></param>
    /// <param name="Specialization"></param>
    public record UpdateFloorballCoachCommand(
        Guid Id,
        bool IsActive,
        int YearsOfExperience,
        string? CertificationLevel,
        string? Specialization) : IRequest<Result<FloorballCoachDto>>;
} 
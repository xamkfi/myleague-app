using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Common;

namespace Application.DTOs.Floorball
{
    /// <summary>
    /// Data Transfer Object for FloorballCoach entity
    /// </summary>
    /// <param name="Id">The unique identifier of the coach</param>
    /// <param name="PersonId">The ID of the person this coach profile belongs to</param>
    /// <param name="Person">The person information for this coach</param>
    /// <param name="IsActive">Whether the coach is currently active</param>
    /// <param name="YearsOfExperience">The coaching experience in years</param>
    /// <param name="CertificationLevel">The coach's certification level (if any)</param>
    public record FloorballCoachDto(
        Guid Id,
        Guid PersonId,
        PersonDto Person,
        bool IsActive,
        int YearsOfExperience,
        string? CertificationLevel,
        string? Specialization);
}

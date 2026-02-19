using System;
using Application.DTOs.Common;

namespace Application.DTOs.Floorball
{
    /// <summary>
    /// Data Transfer Object for FloorballTeamManager entity
    /// </summary>
    /// <param name="Id">The unique identifier of the team manager</param>
    /// <param name="PersonId">The ID of the person this team manager profile belongs to</param>
    /// <param name="Person">The person information for this team manager</param>
    /// <param name="TeamId">The ID of the team this manager is responsible for</param>
    /// <param name="IsActive">Whether the team manager is currently active</param>
    public record FloorballTeamManagerDto(
        Guid Id,
        Guid PersonId,
        PersonDto Person,
        Guid TeamId,
        bool IsActive);
}

using System;
using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;

namespace Application.Features.Floorball.TeamManagers.DTOs
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

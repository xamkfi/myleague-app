using System;
using System.Collections.Generic;
using Application.DTOs.Common;
using Domain.Enums.Floorball;

namespace Application.DTOs.Floorball
{
    /// <summary>
    /// Data Transfer Object for FloorballTeam entity
    /// </summary>
    /// <param name="Id">The unique identifier of the team</param>
    /// <param name="Name">The name of the team</param>
    /// <param name="Division">The division level of the team</param>
    /// <param name="Club">The club this team belongs to</param>
    /// <param name="HomeArena">The team's home arena</param>
    /// <param name="PrimaryJerseyColor">The team's primary jersey color</param>
    /// <param name="SecondaryJerseyColor">The team's secondary jersey color</param>
    /// <param name="HasActiveMembers">Whether the team has any active members</param>
    /// <param name="Roster">The team's roster of players</param>
    public record FloorballTeamDto(
        Guid Id,
        string Name,
        FloorballDivision Division,
        ClubDto Club,
        string HomeArena,
        string PrimaryJerseyColor,
        string SecondaryJerseyColor,
        bool HasActiveMembers,
        IReadOnlyCollection<FloorballTeamPlayerDto> Roster);
}

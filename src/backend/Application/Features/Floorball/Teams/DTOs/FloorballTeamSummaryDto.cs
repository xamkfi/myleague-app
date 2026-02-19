using System;
using Application.DTOs.Common;
using Domain.Enums.Common;

namespace Application.DTOs.Floorball
{
    /// <summary>
    /// Lightweight Data Transfer Object for FloorballTeam entity without roster
    /// </summary>
    /// <param name="Id">The unique identifier of the team</param>
    /// <param name="Name">The name of the team</param>
    /// <param name="DivisionId">The division ID of the team</param>
    /// <param name="Club">The club this team belongs to</param>
    /// <param name="HomeArena">The team's home arena</param>
    /// <param name="PrimaryJerseyColor">The team's primary jersey color</param>
    /// <param name="SecondaryJerseyColor">The team's secondary jersey color</param>
    /// <param name="LogoUrl">The team's logo URL</param>
    /// <param name="HasActiveMembers">Whether the team has any active members</param>
    /// <param name="TeamCategory">The category of the team (Adult, Youth, Women)</param>
    public record FloorballTeamSummaryDto(
        Guid Id,
        string Name,
        Guid? DivisionId,
        ClubDto Club,
        string HomeArena,
        string PrimaryJerseyColor,
        string SecondaryJerseyColor,
        string? LogoUrl,
        bool HasActiveMembers,
        TeamCategory TeamCategory);
}

